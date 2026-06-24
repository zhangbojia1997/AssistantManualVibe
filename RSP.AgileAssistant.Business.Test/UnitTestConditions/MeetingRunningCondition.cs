using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies a meeting's running flag.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class MeetingRunningCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _meetingId;

        private readonly bool _expectedRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingRunningCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="meetingId">Meeting identifier to check.</param>
        /// <param name="expectedRunning">Expected running flag.</param>
        public MeetingRunningCondition(IADOConfigurable adoConfigurable, string meetingId, bool expectedRunning)
        {
            this._adoConfigurable = adoConfigurable;
            this._meetingId = meetingId;
            this._expectedRunning = expectedRunning;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Meeting '{this._meetingId}' IsRunning == {this._expectedRunning}";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Vibe_Meeting_Table WHERE Id = @Id AND IsRunning = @IsRunning;",
                new { Id = this._meetingId, IsRunning = this._expectedRunning });

            return matches == 1;
        }
    }
}
