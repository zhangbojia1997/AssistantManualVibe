using System;
using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies that exactly one guest user was created with the expected stored
    /// refresh token and a generated <c>Guest-</c> login name.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class GuestUserCreatedCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _expectedToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuestUserCreatedCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="expectedToken">Expected stored refresh token.</param>
        public GuestUserCreatedCondition(IADOConfigurable adoConfigurable, string expectedToken)
        {
            this._adoConfigurable = adoConfigurable;
            this._expectedToken = expectedToken;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => "Exactly one guest user created with expected token";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                @"SELECT COUNT(1) FROM Vibe_User_Table
                  WHERE RefreshToken = @RefreshToken AND UserName LIKE 'Guest-%';",
                new { RefreshToken = this._expectedToken });

            return matches == 1;
        }
    }
}
