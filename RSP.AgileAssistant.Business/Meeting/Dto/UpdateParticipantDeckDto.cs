using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for switching a participant's deck during a meeting (FR-VOTE).
    /// </summary>
    public class UpdateParticipantDeckDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Display name of the participant whose deck is changing.
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the new deck to use.
        /// </summary>
        [Required]
        public Guid DeckId { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
