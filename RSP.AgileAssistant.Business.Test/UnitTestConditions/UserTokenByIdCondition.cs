using System;
using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies that the <c>RefreshToken</c> stored for a given user identifier
    /// matches an expected value (or is null when the token was cleared).
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class UserTokenByIdCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly Guid _userId;

        private readonly string? _expectedToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTokenByIdCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="userId">User identifier to inspect.</param>
        /// <param name="expectedToken">Expected stored refresh token, or null.</param>
        public UserTokenByIdCondition(IADOConfigurable adoConfigurable, Guid userId, string? expectedToken)
        {
            this._adoConfigurable = adoConfigurable;
            this._userId = userId;
            this._expectedToken = expectedToken;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"User {this._userId} refresh token equals expected value";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            string? storedToken = connection.QuerySingleOrDefault<string?>(
                "SELECT RefreshToken FROM Vibe_User_Table WHERE Id = @Id;",
                new { Id = this._userId.ToString() });

            return string.Equals(storedToken, this._expectedToken, StringComparison.Ordinal);
        }
    }
}
