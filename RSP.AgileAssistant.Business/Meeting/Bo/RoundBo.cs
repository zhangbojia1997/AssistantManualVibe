using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Bo
{
    /// <summary>
    /// Internal representation of a voting round as stored in the
    /// <c>Vibe_Round_Table</c> table.
    /// </summary>
    public class RoundBo
    {
        /// <summary>
        /// Round status indicating an in-progress vote.
        /// </summary>
        public const string StatusVoting = "Voting";

        /// <summary>
        /// Round status indicating a completed (revealed) vote.
        /// </summary>
        public const string StatusDone = "Done";

        /// <summary>
        /// Unique identifier of the round.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the topic the round is voting on, if any.
        /// </summary>
        public Guid? TopicId { get; set; }

        /// <summary>
        /// Sequential round number (per topic), incremented for re-votes.
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// Current status of the round (<see cref="StatusVoting"/> or
        /// <see cref="StatusDone"/>).
        /// </summary>
        public string Status { get; set; } = StatusVoting;

        /// <summary>
        /// Votes recorded for the round (populated when the round is done).
        /// </summary>
        public List<RoundVoteBo> Votes { get; set; } = new List<RoundVoteBo>();
    }
}
