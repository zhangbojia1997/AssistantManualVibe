using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies a participant's picked-card state: the <c>IsPickedPoker</c> flag and
    /// whether a backing card row exists.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class ParticipantPokerCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _meetingId;

        private readonly string _name;

        private readonly bool _expectedPicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantPokerCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="meetingId">Owning meeting identifier.</param>
        /// <param name="name">Participant name to check.</param>
        /// <param name="expectedPicked">Whether the participant is expected to have picked a card.</param>
        public ParticipantPokerCondition(IADOConfigurable adoConfigurable, string meetingId, string name, bool expectedPicked)
        {
            this._adoConfigurable = adoConfigurable;
            this._meetingId = meetingId;
            this._name = name;
            this._expectedPicked = expectedPicked;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Participant '{this._name}' IsPickedPoker == {this._expectedPicked}";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();

            bool? isPicked = connection.QuerySingleOrDefault<bool?>(
                "SELECT IsPickedPoker FROM Vibe_Participant_Table WHERE MeetingId = @MeetingId AND [Name] = @Name;",
                new { MeetingId = this._meetingId, Name = this._name });

            if (isPicked != this._expectedPicked)
            {
                return false;
            }

            int pokerRows = connection.ExecuteScalar<int>(
                @"SELECT COUNT(1) FROM Vibe_ParticipantPoker_Table pp
                  INNER JOIN Vibe_Participant_Table p ON p.SelectedPokerId = pp.Id
                  WHERE p.MeetingId = @MeetingId AND p.[Name] = @Name;",
                new { MeetingId = this._meetingId, Name = this._name });

            return this._expectedPicked ? pokerRows == 1 : pokerRows == 0;
        }
    }
}
