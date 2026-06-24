using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies whether a meeting with the given identifier exists.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class MeetingExistsCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _meetingId;

        private readonly bool _shouldExist;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingExistsCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="meetingId">Meeting identifier to check.</param>
        /// <param name="shouldExist">Whether the meeting is expected to exist.</param>
        public MeetingExistsCondition(IADOConfigurable adoConfigurable, string meetingId, bool shouldExist)
        {
            this._adoConfigurable = adoConfigurable;
            this._meetingId = meetingId;
            this._shouldExist = shouldExist;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Meeting '{this._meetingId}' exists == {this._shouldExist}";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int matches = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Vibe_Meeting_Table WHERE Id = @Id;",
                new { Id = this._meetingId });

            return this._shouldExist ? matches == 1 : matches == 0;
        }
    }
}
