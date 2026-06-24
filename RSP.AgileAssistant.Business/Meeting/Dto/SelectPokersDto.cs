using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for a participant selecting (or clearing) a card (FR-VOTE).
    /// </summary>
    public class SelectPokersDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Display name of the participant selecting the card.
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// The selected card, or <c>null</c> to clear the participant's selection.
        /// </summary>
        public ParticipantPokerDto? SelectedPoker { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
