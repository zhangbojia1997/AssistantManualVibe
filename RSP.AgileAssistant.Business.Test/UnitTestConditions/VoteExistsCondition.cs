using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies that a vote with a given value exists for a round and user.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class VoteExistsCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _roundId;

        private readonly string _userId;

        private readonly string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoteExistsCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="roundId">Owning round identifier.</param>
        /// <param name="userId">Voting user identifier.</param>
        /// <param name="value">Expected vote value.</param>
        public VoteExistsCondition(IADOConfigurable adoConfigurable, string roundId, string userId, string value)
        {
            this._adoConfigurable = adoConfigurable;
            this._roundId = roundId;
            this._userId = userId;
            this._value = value;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Round '{this._roundId}' has vote '{this._value}' from user '{this._userId}'";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Vibe_RoundVote_Table WHERE RoundId = @RoundId AND UserId = @UserId AND [Value] = @Value;",
                new { RoundId = this._roundId, UserId = this._userId, Value = this._value });

            return matches == 1;
        }
    }
}
