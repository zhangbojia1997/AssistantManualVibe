using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for starting a new voting round (FR-VOTE).
    /// </summary>
    public class StartVotingRoundDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Identifier of the topic being voted on, if any.
        /// </summary>
        public Guid? VotingOn { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
