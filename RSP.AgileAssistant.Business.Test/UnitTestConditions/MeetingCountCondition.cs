using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies the total number of meeting rows.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class MeetingCountCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly int _expectedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeetingCountCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="expectedCount">Expected number of meeting rows.</param>
        public MeetingCountCondition(IADOConfigurable adoConfigurable, int expectedCount)
        {
            this._adoConfigurable = adoConfigurable;
            this._expectedCount = expectedCount;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Meeting table has {this._expectedCount} row(s)";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();
            int count = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM Vibe_Meeting_Table;");
            return count == this._expectedCount;
        }
    }
}
