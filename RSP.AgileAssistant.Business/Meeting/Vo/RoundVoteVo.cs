using System;

namespace RSP.AgileAssistant.Business.Meeting.Vo
{
    /// <summary>
    /// Output representation of a single vote within a round. The configured JSON
    /// serializer applies the camelCase contract expected by the front-end.
    /// </summary>
    public class RoundVoteVo
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
