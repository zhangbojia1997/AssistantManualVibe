-- =============================================================================
-- FR-AUTH: AppUser table
-- Strong entity for application users and their authentication state.
-- Primary key is a non-sequential GUID (RSP strong-entity convention).
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AppUser')
BEGIN
    CREATE TABLE AppUser
    (
        Id              UNIQUEIDENTIFIER    NOT NULL,
        UserName        NVARCHAR(256)       NOT NULL,
        RefreshToken    NVARCHAR(MAX)       NULL,
        FullName        NVARCHAR(256)       NULL,
        ScrumMaster     BIT                 NOT NULL CONSTRAINT DF_AppUser_ScrumMaster DEFAULT (0),
        IsGuest         BIT                 NOT NULL CONSTRAINT DF_AppUser_IsGuest DEFAULT (0),
        LastActiveDate  DATETIME2(7)        NOT NULL,
        CONSTRAINT PK_AppUser PRIMARY KEY NONCLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_AppUser_UserName ON AppUser (UserName);
END
GO
