using System;
using System.Collections.Generic;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a voting round returned to API clients.
    /// </summary>
    public class RoundVo
    {
        /// <summary>
        /// Unique identifier of the round.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the topic the round is voting on, if any.
        /// </summary>
        public Guid? TopicId { get; set; }

        /// <summary>
        /// Sequential round number (per topic).
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// Current status of the round (<c>Voting</c> or <c>Done</c>).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Votes recorded for the round.
        /// </summary>
        public List<RoundVoteVo> Votes { get; set; } = new List<RoundVoteVo>();
    }
}
