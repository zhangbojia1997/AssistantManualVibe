using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace RSP.AgileAssistant.Business.User.Sql
{
    /// <summary>
    /// Parameterized SQL statements and parameter builders for the
    /// <c>Vibe_User_Table</c> table. All values are bound through
    /// <see cref="DbParameter"/> instances to prevent SQL injection.
    /// </summary>
    internal static class UserSql
    {
        /// <summary>
        /// Selects a single user by its unique identifier.
        /// </summary>
        internal const string SelectById = @"
SELECT
    Id,
    UserName,
    RefreshToken,
    FullName,
    DefaultDeckId,
    DefaultGroup,
    JiraEmail,
    JiraToken,
    ScrumMaster,
    UseDefaults,
    LastActiveDate
FROM
    Vibe_User_Table
WHERE
    Id = @Id";

        /// <summary>
        /// Selects a single user by its login name.
        /// </summary>
        internal const string SelectByUserName = @"
SELECT
    Id,
    UserName,
    RefreshToken,
    FullName,
    DefaultDeckId,
    DefaultGroup,
    JiraEmail,
    JiraToken,
    ScrumMaster,
    UseDefaults,
    LastActiveDate
FROM
    Vibe_User_Table
WHERE
    UserName = @UserName";

        /// <summary>
        /// Inserts a new user row.
        /// </summary>
        internal const string Insert = @"
INSERT INTO Vibe_User_Table
    (Id, UserName, RefreshToken, FullName, DefaultDeckId, DefaultGroup, JiraEmail, JiraToken, ScrumMaster, UseDefaults, LastActiveDate)
VALUES
    (@Id, @UserName, @RefreshToken, @FullName, @DefaultDeckId, @DefaultGroup, @JiraEmail, @JiraToken, @ScrumMaster, @UseDefaults, @LastActiveDate)";

        /// <summary>
        /// Updates the stored refresh token, scrum master flag and activity
        /// timestamp for an existing user.
        /// </summary>
        internal const string UpdateRefreshToken = @"
UPDATE Vibe_User_Table
SET
    RefreshToken = @RefreshToken,
    ScrumMaster = @ScrumMaster,
    LastActiveDate = @LastActiveDate
WHERE
    Id = @Id";

        /// <summary>
        /// Clears the stored refresh token (logout / token revocation).
        /// <c>RefreshToken</c> is non-nullable, so it is reset to an empty string.
        /// </summary>
        internal const string ClearRefreshToken = @"
UPDATE Vibe_User_Table
SET
    RefreshToken = '',
    LastActiveDate = @LastActiveDate
WHERE
    Id = @Id";

        /// <summary>
        /// Updates a user's persisted preferences (FR-USER): full name, default
        /// deck, default group, Jira credentials and the "apply defaults" toggle.
        /// </summary>
        internal const string UpdateSettings = @"
UPDATE Vibe_User_Table
SET
    FullName = @FullName,
    DefaultDeckId = @DefaultDeckId,
    DefaultGroup = @DefaultGroup,
    JiraEmail = @JiraEmail,
    JiraToken = @JiraToken,
    UseDefaults = @UseDefaults,
    LastActiveDate = @LastActiveDate
WHERE
    Id = @Id";

        /// <summary>
        /// Builds parameters for <see cref="SelectById"/>.
        /// </summary>
        /// <param name="id">User identifier to match.</param>
        internal static DbParameter[] SelectByIdParameters(Guid id)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
            };
        }

        /// <summary>
        /// Builds parameters for <see cref="SelectByUserName"/>.
        /// </summary>
        /// <param name="userName">User name to match.</param>
        internal static DbParameter[] SelectByUserNameParameters(string userName)
        {
            return new DbParameter[]
            {
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = userName },
            };
        }

        /// <summary>
        /// Builds parameters for <see cref="Insert"/>.
        /// </summary>
        /// <param name="user">User business object to persist.</param>
        /// <param name="lastActiveDate">Activity timestamp.</param>
        internal static DbParameter[] InsertParameters(Bo.UserBo user, DateTime lastActiveDate)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = user.Id.ToString() },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = (object?)user.UserName ?? DBNull.Value },
                new SqlParameter("@RefreshToken", SqlDbType.NVarChar) { Value = (object?)user.RefreshToken ?? string.Empty },
                new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = (object?)user.FullName ?? DBNull.Value },
                new SqlParameter("@DefaultDeckId", SqlDbType.NVarChar, 36) { Value = (object?)user.DefaultDeckId ?? DBNull.Value },
                new SqlParameter("@DefaultGroup", SqlDbType.NVarChar, 100) { Value = (object?)user.DefaultGroup ?? DBNull.Value },
                new SqlParameter("@JiraEmail", SqlDbType.NVarChar) { Value = (object?)user.JiraEmail ?? DBNull.Value },
                new SqlParameter("@JiraToken", SqlDbType.NVarChar) { Value = (object?)user.JiraToken ?? DBNull.Value },
                new SqlParameter("@ScrumMaster", SqlDbType.Bit) { Value = user.ScrumMaster },
                new SqlParameter("@UseDefaults", SqlDbType.Bit) { Value = user.UseDefaults },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = lastActiveDate },
            };
        }

        /// <summary>
        /// Builds parameters for <see cref="UpdateRefreshToken"/>.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="refreshToken">New access token to persist.</param>
        /// <param name="scrumMaster">Scrum master role flag.</param>
        /// <param name="lastActiveDate">Activity timestamp.</param>
        internal static DbParameter[] UpdateRefreshTokenParameters(
            Guid id,
            string? refreshToken,
            bool scrumMaster,
            DateTime lastActiveDate)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
                new SqlParameter("@RefreshToken", SqlDbType.NVarChar) { Value = (object?)refreshToken ?? string.Empty },
                new SqlParameter("@ScrumMaster", SqlDbType.Bit) { Value = scrumMaster },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = lastActiveDate },
            };
        }

        /// <summary>
        /// Builds parameters for <see cref="ClearRefreshToken"/>.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="lastActiveDate">Activity timestamp.</param>
        internal static DbParameter[] ClearRefreshTokenParameters(Guid id, DateTime lastActiveDate)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = id.ToString() },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = lastActiveDate },
            };
        }

        /// <summary>
        /// Builds parameters for <see cref="UpdateSettings"/>.
        /// </summary>
        /// <param name="user">User business object holding the values to persist.</param>
        /// <param name="lastActiveDate">Activity timestamp.</param>
        internal static DbParameter[] UpdateSettingsParameters(Bo.UserBo user, DateTime lastActiveDate)
        {
            return new DbParameter[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar, 36) { Value = user.Id.ToString() },
                new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = (object?)user.FullName ?? DBNull.Value },
                new SqlParameter("@DefaultDeckId", SqlDbType.NVarChar, 36) { Value = (object?)user.DefaultDeckId ?? DBNull.Value },
                new SqlParameter("@DefaultGroup", SqlDbType.NVarChar, 100) { Value = (object?)user.DefaultGroup ?? DBNull.Value },
                new SqlParameter("@JiraEmail", SqlDbType.NVarChar) { Value = (object?)user.JiraEmail ?? DBNull.Value },
                new SqlParameter("@JiraToken", SqlDbType.NVarChar) { Value = (object?)user.JiraToken ?? DBNull.Value },
                new SqlParameter("@UseDefaults", SqlDbType.Bit) { Value = user.UseDefaults },
                new SqlParameter("@LastActiveDate", SqlDbType.DateTime2) { Value = lastActiveDate },
            };
        }

        /// <summary>
        /// Maps a data row from the <c>Vibe_User_Table</c> table to a
        /// <see cref="Bo.UserBo"/>.
        /// </summary>
        /// <param name="row">Source data row.</param>
        internal static Bo.UserBo MapUser(DataRow row)
        {
            return new Bo.UserBo
            {
                Id = Guid.Parse((string)row["Id"]),
                UserName = row["UserName"] == DBNull.Value ? string.Empty : (string)row["UserName"],
                RefreshToken = row["RefreshToken"] == DBNull.Value ? null : (string)row["RefreshToken"],
                FullName = row["FullName"] == DBNull.Value ? null : (string)row["FullName"],
                DefaultDeckId = row["DefaultDeckId"] == DBNull.Value ? null : (string)row["DefaultDeckId"],
                DefaultGroup = row["DefaultGroup"] == DBNull.Value ? null : (string)row["DefaultGroup"],
                JiraEmail = row["JiraEmail"] == DBNull.Value ? null : (string)row["JiraEmail"],
                JiraToken = row["JiraToken"] == DBNull.Value ? null : (string)row["JiraToken"],
                ScrumMaster = (bool)row["ScrumMaster"],
                UseDefaults = (bool)row["UseDefaults"],
                LastActiveDate = (DateTime)row["LastActiveDate"],
            };
        }
    }
}
