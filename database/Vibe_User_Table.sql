-- =============================================================================
-- FR-AUTH: Vibe_User_Table table
-- Strong entity for application users and their authentication state.
-- Primary key is a string GUID stored as NVARCHAR(36).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vibe_User_Table')
BEGIN
    CREATE TABLE Vibe_User_Table
    (
        Id              NVARCHAR(36)    NOT NULL,
        UserName        NVARCHAR(MAX)   NULL,
        RefreshToken    NVARCHAR(MAX)   NOT NULL,
        FullName        NVARCHAR(MAX)   NULL,
        DefaultDeckId   NVARCHAR(36)    NULL,
        DefaultGroup    NVARCHAR(100)   NULL,
        JiraEmail       NVARCHAR(MAX)   NULL,
        JiraToken       NVARCHAR(MAX)   NULL,
        ScrumMaster     BIT             NOT NULL CONSTRAINT DF_Vibe_User_Table_ScrumMaster DEFAULT (0),
        UseDefaults     BIT             NOT NULL CONSTRAINT DF_Vibe_User_Table_UseDefaults DEFAULT (0),
        LastActiveDate  DATETIME2(7)    NOT NULL,
        CONSTRAINT PK_Vibe_User_Table PRIMARY KEY NONCLUSTERED (Id)
    );
END
GO
