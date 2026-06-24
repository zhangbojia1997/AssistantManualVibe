using System;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// A single vote within an <see cref="EndVotingRoundDto"/> payload (FR-VOTE).
    /// The configured JSON serializer maps these to the camelCase front-end
    /// contract on the wire.
    /// </summary>
    public class RoundVoteDto
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
