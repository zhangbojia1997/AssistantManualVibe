using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for ending the current voting round (FR-VOTE). Carries the
    /// votes to persist before the round is marked done.
    /// </summary>
    public class EndVotingRoundDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Votes cast during the round, to be saved against it.
        /// </summary>
        public List<RoundVoteDto> Votes { get; set; } = new List<RoundVoteDto>();

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
