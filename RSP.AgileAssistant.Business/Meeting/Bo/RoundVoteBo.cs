using System;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a single vote cast within a round, as stored in
    /// the <c>Vibe_RoundVote_Table</c> table.
    /// </summary>
    public class RoundVoteBo
    {
        /// <summary>
        /// Identifier of the user who cast the vote.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The card value the user voted with.
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
