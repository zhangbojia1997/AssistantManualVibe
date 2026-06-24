using System;
using Dapper;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using RSP.Common.DataAccess;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.Base
{
    /// <summary>
    /// Base class for action unit tests. Ensures an isolated LocalDB database
    /// containing the <c>Vibe_User_Table</c> schema exists before each test and
    /// clears the table before and after each test for repeatability.
    /// </summary>
    internal class ActionTestBase : UnitTestBase
    {
        /// <summary>
        /// Schema for the <c>Vibe_User_Table</c> table, created on demand for tests.
        /// </summary>
        private const string AppUserSchema = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_User_Table')
BEGIN
    CREATE TABLE Vibe_User_Table
    (
        Id              NVARCHAR(36)        NOT NULL,
        UserName        NVARCHAR(MAX)       NULL,
        RefreshToken    NVARCHAR(MAX)       NOT NULL,
        FullName        NVARCHAR(MAX)       NULL,
        DefaultDeckId   NVARCHAR(36)        NULL,
        DefaultGroup    NVARCHAR(100)       NULL,
        JiraEmail       NVARCHAR(MAX)       NULL,
        JiraToken       NVARCHAR(MAX)       NULL,
        ScrumMaster     BIT                 NOT NULL CONSTRAINT DF_Vibe_User_Table_ScrumMaster DEFAULT (0),
        UseDefaults     BIT                 NOT NULL CONSTRAINT DF_Vibe_User_Table_UseDefaults DEFAULT (0),
        LastActiveDate  DATETIME2(7)        NOT NULL,
        CONSTRAINT PK_Vibe_User_Table PRIMARY KEY NONCLUSTERED (Id)
    );
END";

        /// <summary>
        /// Schema for the meeting aggregate tables (FR-MEET / FR-VOTE), created on
        /// demand for tests. Mirrors the physical model with cascading deletes.
        /// </summary>
        private const string MeetingSchema = @"
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
        IsRunning       BIT             NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_IsRunning DEFAULT (1),
        LastActiveDate  DATETIME2(7)    NOT NULL,
        JiraEmail       NVARCHAR(MAX)   NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraEmail DEFAULT (''),
        JiraToken       NVARCHAR(MAX)   NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraToken DEFAULT (''),
        JiraConnected   BIT             NOT NULL CONSTRAINT DF_Vibe_Meeting_Table_JiraConnected DEFAULT (0),
        CreatedOn       BIGINT          NOT NULL,
        CONSTRAINT PK_Vibe_Meeting_Table PRIMARY KEY NONCLUSTERED (Id)
    );
END
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
END";

        /// <summary>
        /// Clears every meeting and user table in foreign-key safe order.
        /// </summary>
        private const string ClearAllTables = @"
IF OBJECT_ID('Vibe_TopicTask_To_TopicTask', 'U') IS NOT NULL DELETE FROM Vibe_TopicTask_To_TopicTask;
IF OBJECT_ID('Vibe_RoundVote_Table', 'U') IS NOT NULL DELETE FROM Vibe_RoundVote_Table;
IF OBJECT_ID('Vibe_ParticipantPoker_Table', 'U') IS NOT NULL DELETE FROM Vibe_ParticipantPoker_Table;
IF OBJECT_ID('Vibe_TopicTask_Table', 'U') IS NOT NULL DELETE FROM Vibe_TopicTask_Table;
IF OBJECT_ID('Vibe_Round_Table', 'U') IS NOT NULL DELETE FROM Vibe_Round_Table;
IF OBJECT_ID('Vibe_Participant_Table', 'U') IS NOT NULL DELETE FROM Vibe_Participant_Table;
IF OBJECT_ID('Vibe_MeetingGroup', 'U') IS NOT NULL DELETE FROM Vibe_MeetingGroup;
IF OBJECT_ID('Vibe_Meeting_Table', 'U') IS NOT NULL DELETE FROM Vibe_Meeting_Table;
IF OBJECT_ID('Vibe_User_Table', 'U') IS NOT NULL DELETE FROM Vibe_User_Table;";

        /// <summary>
        /// Provisions the test database and clears existing data before a test.
        /// </summary>
        [SetUp]
        public void PrepareDatabase()
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ado.ConnectionString);
            string databaseName = builder.InitialCatalog;

            SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder(ado.ConnectionString)
            {
                InitialCatalog = "master",
            };

            using (SqlConnection master = new SqlConnection(masterBuilder.ConnectionString))
            {
                master.Open();
                master.Execute($"IF DB_ID(N'{databaseName}') IS NULL CREATE DATABASE [{databaseName}];");
            }

            using (SqlConnection connection = new SqlConnection(ado.ConnectionString))
            {
                connection.Open();
                connection.Execute(AppUserSchema);
                connection.Execute(MeetingSchema);
                connection.Execute(ClearAllTables);
            }
        }

        /// <summary>
        /// Clears test data after a test completes.
        /// </summary>
        [TearDown]
        public void CleanDatabase()
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(ClearAllTables);
        }

        /// <summary>
        /// Inserts a user row used as pre-existing test data.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="userName">Login name.</param>
        /// <param name="refreshToken">Stored refresh token, if any.</param>
        protected void InsertUser(Guid id, string userName, string? refreshToken)
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(
                @"INSERT INTO Vibe_User_Table (Id, UserName, RefreshToken, FullName, ScrumMaster, UseDefaults, LastActiveDate)
                  VALUES (@Id, @UserName, ISNULL(@RefreshToken, ''), NULL, 0, 0, SYSUTCDATETIME());",
                new { Id = id.ToString(), UserName = userName, RefreshToken = refreshToken });
        }

        /// <summary>
        /// Inserts a meeting row used as pre-existing test data.
        /// </summary>
        /// <param name="id">Meeting identifier.</param>
        /// <param name="hostId">Host (owner) identifier.</param>
        /// <param name="topic">Meeting topic.</param>
        /// <param name="deckId">Default deck identifier.</param>
        /// <param name="isRunning">Whether the meeting is running.</param>
        protected void InsertMeeting(Guid id, Guid hostId, string topic, Guid deckId, bool isRunning = true)
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(
                @"INSERT INTO Vibe_Meeting_Table
                    (Id, HostId, Topic, DeckId, VotingOn, VotingRound, IsRunning, LastActiveDate, JiraEmail, JiraToken, JiraConnected, CreatedOn)
                  VALUES
                    (@Id, @HostId, @Topic, @DeckId, NULL, NULL, @IsRunning, SYSUTCDATETIME(), '', '', 0, @CreatedOn);",
                new
                {
                    Id = id.ToString(),
                    HostId = hostId.ToString(),
                    Topic = topic,
                    DeckId = deckId.ToString(),
                    IsRunning = isRunning,
                    CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                });
        }

        /// <summary>
        /// Inserts a participant row used as pre-existing test data.
        /// </summary>
        /// <param name="meetingId">Owning meeting identifier.</param>
        /// <param name="participantId">Participant identifier.</param>
        /// <param name="userId">Underlying user identifier.</param>
        /// <param name="name">Participant display name.</param>
        /// <param name="deckId">Participant deck identifier.</param>
        /// <param name="group">Participant group.</param>
        protected void InsertParticipant(
            Guid meetingId,
            Guid participantId,
            Guid userId,
            string name,
            Guid deckId,
            string group = "")
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(
                @"INSERT INTO Vibe_Participant_Table
                    (Id, UserId, [Name], DeckId, [Group], IsPickedPoker, SelectedPokerId, MeetingId)
                  VALUES
                    (@Id, @UserId, @Name, @DeckId, @Group, 0, NULL, @MeetingId);",
                new
                {
                    Id = participantId.ToString(),
                    UserId = userId.ToString(),
                    Name = name,
                    DeckId = deckId.ToString(),
                    Group = group,
                    MeetingId = meetingId.ToString(),
                });
        }

        /// <summary>
        /// Inserts a voting round row used as pre-existing test data.
        /// </summary>
        /// <param name="meetingId">Owning meeting identifier.</param>
        /// <param name="roundId">Round identifier.</param>
        /// <param name="topicId">Topic identifier, if any.</param>
        /// <param name="roundNumber">Round number.</param>
        /// <param name="status">Round status.</param>
        protected void InsertRound(Guid meetingId, Guid roundId, Guid? topicId, int roundNumber, string status)
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(
                @"INSERT INTO Vibe_Round_Table (Id, TopicId, RoundNumber, [Status], MeetingId)
                  VALUES (@Id, @TopicId, @RoundNumber, @Status, @MeetingId);",
                new
                {
                    Id = roundId.ToString(),
                    TopicId = topicId?.ToString(),
                    RoundNumber = roundNumber,
                    Status = status,
                    MeetingId = meetingId.ToString(),
                });
        }

        /// <summary>
        /// Sets the active voting round on a meeting.
        /// </summary>
        /// <param name="meetingId">Meeting identifier.</param>
        /// <param name="votingOn">Topic currently voted on, if any.</param>
        /// <param name="votingRound">Active round identifier, if any.</param>
        protected void SetMeetingVoting(Guid meetingId, Guid? votingOn, Guid? votingRound)
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            using SqlConnection connection = new SqlConnection(ado.ConnectionString);
            connection.Open();
            connection.Execute(
                @"UPDATE Vibe_Meeting_Table SET VotingOn = @VotingOn, VotingRound = @VotingRound WHERE Id = @Id;",
                new
                {
                    Id = meetingId.ToString(),
                    VotingOn = votingOn?.ToString(),
                    VotingRound = votingRound?.ToString(),
                });
        }
    }
}
