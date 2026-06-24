using Dapper;
using Microsoft.Data.SqlClient;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.UnitTestConditions
{
    /// <summary>
    /// Verifies whether a participant with a given name exists in a meeting,
    /// optionally checking its deck.
    /// </summary>
    /// <typeparam name="TAction">Action under test.</typeparam>
    internal sealed class ParticipantExistsCondition<TAction> : IUnitTestCondition<TAction>
    {
        private readonly IADOConfigurable _adoConfigurable;

        private readonly string _meetingId;

        private readonly string _name;

        private readonly bool _shouldExist;

        private readonly string? _expectedDeckId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantExistsCondition{TAction}"/> class.
        /// </summary>
        /// <param name="adoConfigurable">Database configuration.</param>
        /// <param name="meetingId">Owning meeting identifier.</param>
        /// <param name="name">Participant name to check.</param>
        /// <param name="shouldExist">Whether the participant is expected to exist.</param>
        /// <param name="expectedDeckId">Expected deck identifier, when verifying.</param>
        public ParticipantExistsCondition(
            IADOConfigurable adoConfigurable,
            string meetingId,
            string name,
            bool shouldExist,
            string? expectedDeckId = null)
        {
            this._adoConfigurable = adoConfigurable;
            this._meetingId = meetingId;
            this._name = name;
            this._shouldExist = shouldExist;
            this._expectedDeckId = expectedDeckId;
        }

        /// <summary>
        /// Human-readable description used when the condition fails.
        /// </summary>
        public string Condition => $"Participant '{this._name}' in meeting '{this._meetingId}' exists == {this._shouldExist}";

        /// <summary>
        /// Executes the verification against the test database.
        /// </summary>
        public bool Execute(TAction testee, object acturalResult, object expectedResult, ILoggingLogger logginLogger)
        {
            using SqlConnection connection = new SqlConnection(this._adoConfigurable.ConnectionString);
            connection.Open();

            int matches = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Vibe_Participant_Table WHERE MeetingId = @MeetingId AND [Name] = @Name;",
                new { MeetingId = this._meetingId, Name = this._name });

            if (this._shouldExist && matches != 1)
            {
                return false;
            }

            if (!this._shouldExist)
            {
                return matches == 0;
            }

            if (this._expectedDeckId != null)
            {
                int deckMatches = connection.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM Vibe_Participant_Table WHERE MeetingId = @MeetingId AND [Name] = @Name AND DeckId = @DeckId;",
                    new { MeetingId = this._meetingId, Name = this._name, DeckId = this._expectedDeckId });
                return deckMatches == 1;
            }

            return true;
        }
    }
}
