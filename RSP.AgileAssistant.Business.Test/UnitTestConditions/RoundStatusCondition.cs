using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies the number of rounds in a meeting with a given status.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class RoundStatusCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _meetingId;

        private readonly string _status;

        private readonly int _expectedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundStatusCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="meetingId">Owning meeting identifier.</param>
        /// <param name="status">Round status to match.</param>
        /// <param name="expectedCount">Expected number of matching rounds.</param>
        public RoundStatusCondition(IADOConfigurable adoConfigurable, string meetingId, string status, int expectedCount)
        {
            this._adoConfigurable = adoConfigurable;
            this._meetingId = meetingId;
            this._status = status;
            this._expectedCount = expectedCount;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Meeting '{this._meetingId}' has {this._expectedCount} '{this._status}' round(s)";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Vibe_Round_Table WHERE MeetingId = @MeetingId AND [Status] = @Status;",
                new { MeetingId = this._meetingId, Status = this._status });

            return matches == this._expectedCount;
        }
    }
}
