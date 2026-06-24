using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies that the persisted settings for a given user identifier match the
    /// expected values after an update-settings action runs.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class UserSettingsByIdCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _userId;

        private readonly string? _expectedFullName;

        private readonly string? _expectedDefaultDeckId;

        private readonly string? _expectedDefaultGroup;

        private readonly string? _expectedJiraEmail;

        private readonly string? _expectedJiraToken;

        private readonly bool _expectedUseDefaults;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingsByIdCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="userId">Identifier of the user to inspect.</param>
        /// <param name="expectedFullName">Expected stored full name.</param>
        /// <param name="expectedDefaultDeckId">Expected stored default deck identifier.</param>
        /// <param name="expectedDefaultGroup">Expected stored default group.</param>
        /// <param name="expectedJiraEmail">Expected stored Jira email.</param>
        /// <param name="expectedJiraToken">Expected stored Jira token.</param>
        /// <param name="expectedUseDefaults">Expected stored "apply defaults" flag.</param>
        public UserSettingsByIdCondition(
            IADOConfigurable adoConfigurable,
            string userId,
            string? expectedFullName,
            string? expectedDefaultDeckId,
            string? expectedDefaultGroup,
            string? expectedJiraEmail,
            string? expectedJiraToken,
            bool expectedUseDefaults)
        {
            this._adoConfigurable = adoConfigurable;
            this._userId = userId;
            this._expectedFullName = expectedFullName;
            this._expectedDefaultDeckId = expectedDefaultDeckId;
            this._expectedDefaultGroup = expectedDefaultGroup;
            this._expectedJiraEmail = expectedJiraEmail;
            this._expectedJiraToken = expectedJiraToken;
            this._expectedUseDefaults = expectedUseDefaults;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"User {this._userId} settings equal the expected values";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();

            StoredSettings? stored = connection.QuerySingleOrDefault<StoredSettings>(
                @"SELECT FullName, DefaultDeckId, DefaultGroup, JiraEmail, JiraToken, UseDefaults
                  FROM Vibe_User_Table WHERE Id = @Id;",
                new { Id = this._userId });

            if (stored == null)
            {
                logginLogger.LogInfo($"User {this._userId} was not found.");
                return false;
            }

            return (stored.FullName == this._expectedFullName)
                && (stored.DefaultDeckId == this._expectedDefaultDeckId)
                && (stored.DefaultGroup == this._expectedDefaultGroup)
                && (stored.JiraEmail == this._expectedJiraEmail)
                && (stored.JiraToken == this._expectedJiraToken)
                && (stored.UseDefaults == this._expectedUseDefaults);
        }

        /// <summary>
        /// Row projection used to read back the persisted user settings.
        /// </summary>
        private sealed class StoredSettings
        {
            public string? FullName { get; set; }

            public string? DefaultDeckId { get; set; }

            public string? DefaultGroup { get; set; }

            public string? JiraEmail { get; set; }

            public string? JiraToken { get; set; }

            public bool UseDefaults { get; set; }
        }
    }
}
