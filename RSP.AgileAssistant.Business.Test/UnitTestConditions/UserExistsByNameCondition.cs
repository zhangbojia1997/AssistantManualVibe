using System;
using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies that a user with the given login name exists with the expected
    /// stored refresh token.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class UserExistsByNameCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _userName;

        private readonly string _expectedToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExistsByNameCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="userName">Expected login name.</param>
        /// <param name="expectedToken">Expected stored refresh token.</param>
        public UserExistsByNameCondition(IADOConfigurable adoConfigurable, string userName, string expectedToken)
        {
            this._adoConfigurable = adoConfigurable;
            this._userName = userName;
            this._expectedToken = expectedToken;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"User '{this._userName}' exists with expected token";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                @"SELECT COUNT(1) FROM Vibe_User_Table
                  WHERE UserName = @UserName AND RefreshToken = @RefreshToken;",
                new { UserName = this._userName, RefreshToken = this._expectedToken });

            return matches == 1;
        }
    }
}
