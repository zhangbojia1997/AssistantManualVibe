-- =============================================================================
-- FR-MEET / FR-VOTE: Meeting aggregate physical tables (SQL Server).
-- Mirrors ER diagram 2.3.2 of the front/back-end technical design.
-- All GUIDs are stored as NVARCHAR(36) strings; every foreign key uses
-- ON DELETE CASCADE so deleting a meeting cleans up its children.
-- The Vibe_Meeting_Table carries a Status flag used by the polling model:
-- 1 while the meeting is active, 0 once the host leaves, the meeting is
-- deleted, ended or expires. The "join" dropdown only lists Status = 1 meetings.
-- =============================================================================

-- ----------------------------------------------------------------------------
-- Meeting (aggregate root)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_Meeting_Table')
BEGIN
    CREATE TABLE Vibe_Meeting_Table
    (
        Id              NVARCHAR(36)    NOT NULL,
        HostId          NVARCHAR(36)    NULL,
        Topic           NVARCHAR(MAX)   NOT NULL,
        DeckId          NVARCHAR(36)    NOT NULL,
        VotingOn        NVARCHAR(36)    NULL,
        VotingRound     NVARCHAR(36)    NULL,
        Status          INT             NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_Status DEFAULT (1),
        LastActiveDate  DATETIME2(7)    NOT NULL,
        JiraEmail       NVARCHAR(MAX)   NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraEmail DEFAULT (''),
        JiraToken       NVARCHAR(MAX)   NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraToken DEFAULT (''),
        JiraConnected   BIT             NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraConnected DEFAULT (0),
        CreatedOn       BIGINT          NOT NULL,
        CONSTRAINT PK_Vibe_Meeting_Table PRIMARY KEY NONCLUSTERED (Id)
    );
END
GO

-- Migration: add Status to pre-existing Vibe_Meeting_Table instances that
-- were created before the column was introduced. Prevents "Invalid column
-- name 'Status'." errors at runtime and backfills from the legacy IsRunning
-- column when it exists.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Vibe_Meeting_Table') AND name = 'Status')
BEGIN
    ALTER TABLE Vibe_Meeting_Table
        ADD Status INT NOT NULL
            CONSTRAINT DF_Vibe_Meeting_Table_Status DEFAULT (1);

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('Vibe_Meeting_Table') AND name = 'IsRunning')
    BEGIN
        UPDATE Vibe_Meeting_Table
        SET Status = CASE WHEN IsRunning = 1 THEN 1 ELSE 0 END;
    END
END
GO

-- ----------------------------------------------------------------------------
-- Round (voting round, child of meeting)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_Round_Table')
BEGIN
    CREATE TABLE Vibe_Round_Table
    (
        Id          NVARCHAR(36)    NOT NULL,
        TopicId     NVARCHAR(36)    NULL,
        RoundNumber INT             NOT NULL,
        [Status]    NVARCHAR(100)   NOT NULL,
        MeetingId   NVARCHAR(36)    NOT NULL,
        CONSTRAINT PK_Vibe_Round_Table PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Vibe_Round_Meeting FOREIGN KEY (MeetingId)
            REFERENCES Vibe_Meeting_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- Participant (child of meeting)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_Participant_Table')
BEGIN
    CREATE TABLE Vibe_Participant_Table
    (
        Id              NVARCHAR(36)    NOT NULL,
        UserId          NVARCHAR(36)    NOT NULL,
        [Name]          NVARCHAR(MAX)   NOT NULL,
        DeckId          NVARCHAR(36)    NOT NULL,
        [Group]         NVARCHAR(MAX)   NOT NULL,
        IsPickedPoker   BIT             NOT NULL CONSTRAINT DF_Vibe_Participant_Table_IsPickedPoker DEFAULT (0),
        SelectedPokerId NVARCHAR(36)    NULL,
        MeetingId       NVARCHAR(36)    NULL,
        CONSTRAINT PK_Vibe_Participant_Table PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Vibe_Participant_Meeting FOREIGN KEY (MeetingId)
            REFERENCES Vibe_Meeting_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- ParticipantPoker (current hand, child of participant)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_ParticipantPoker_Table')
BEGIN
    CREATE TABLE Vibe_ParticipantPoker_Table
    (
        Id                  NVARCHAR(36)    NOT NULL,
        ParticipantName     NVARCHAR(MAX)   NOT NULL,
        PokerId             NVARCHAR(36)    NOT NULL,
        OriginalPokerValue  NVARCHAR(MAX)   NOT NULL,
        PokerValue          NVARCHAR(MAX)   NOT NULL,
        ParticipantId       NVARCHAR(36)    NULL,
        CONSTRAINT PK_Vibe_ParticipantPoker_Table PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Vibe_ParticipantPoker_Participant FOREIGN KEY (ParticipantId)
            REFERENCES Vibe_Participant_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- RoundVote (a single vote, child of round; references a user)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_RoundVote_Table')
BEGIN
    CREATE TABLE Vibe_RoundVote_Table
    (
        Id      INT             NOT NULL IDENTITY (1, 1),
        [Value] NVARCHAR(100)   NOT NULL,
        UserId  NVARCHAR(36)    NOT NULL,
        RoundId NVARCHAR(36)    NOT NULL,
        CONSTRAINT PK_Vibe_RoundVote_Table PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Vibe_RoundVote_User FOREIGN KEY (UserId)
            REFERENCES Vibe_User_Table (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Vibe_RoundVote_Round FOREIGN KEY (RoundId)
            REFERENCES Vibe_Round_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- MeetingGroup (groups belonging to a meeting; composite PK)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_MeetingGroup')
BEGIN
    CREATE TABLE Vibe_MeetingGroup
    (
        [Group]     NVARCHAR(100)   NOT NULL,
        MeetingId   NVARCHAR(36)    NOT NULL,
        CONSTRAINT PK_Vibe_MeetingGroup PRIMARY KEY (MeetingId, [Group]),
        CONSTRAINT FK_Vibe_MeetingGroup_Meeting FOREIGN KEY (MeetingId)
            REFERENCES Vibe_Meeting_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- TopicTask (shared by SubTopic and TopicTask; child of meeting)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_TopicTask_Table')
BEGIN
    CREATE TABLE Vibe_TopicTask_Table
    (
        Id          NVARCHAR(36)    NOT NULL,
        Topic       NVARCHAR(MAX)   NOT NULL,
        [Desc]      NVARCHAR(MAX)   NOT NULL,
        Points      FLOAT           NULL,
        JiraKey     NVARCHAR(MAX)   NULL,
        IssueType   NVARCHAR(MAX)   NOT NULL,
        JiraStatus  NVARCHAR(MAX)   NOT NULL,
        MeetingId   NVARCHAR(36)    NULL,
        CONSTRAINT PK_Vibe_TopicTask_Table PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Vibe_TopicTask_Meeting FOREIGN KEY (MeetingId)
            REFERENCES Vibe_Meeting_Table (Id) ON DELETE CASCADE
    );
END
GO

-- ----------------------------------------------------------------------------
-- TopicTask_To_TopicTask (parent SubTopic Id1 -> child Task Id2)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_TopicTask_To_TopicTask')
BEGIN
    CREATE TABLE Vibe_TopicTask_To_TopicTask
    (
        Id1 NVARCHAR(36)    NOT NULL,
        Id2 NVARCHAR(36)    NOT NULL,
        CONSTRAINT PK_Vibe_TopicTask_To_TopicTask PRIMARY KEY (Id1, Id2),
        CONSTRAINT FK_Vibe_TopicTask_To_TopicTask_Parent FOREIGN KEY (Id1)
            REFERENCES Vibe_TopicTask_Table (Id) ON DELETE CASCADE
    );
END
GO
