using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for creating (starting) a new meeting (FR-MEET).
    /// </summary>
    public class AddMeetingDto : DtoBase
    {
        /// <summary>
        /// Discussion topic / title of the meeting.
        /// </summary>
        [Required]
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the host creating the meeting. Populated from the caller's
        /// access token by the controller.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Identifier of the deck used for estimation.
        /// </summary>
        [Required]
        public Guid DeckId { get; set; }

        /// <summary>
        /// Groups participating in the meeting.
        /// </summary>
        public List<string> Groups { get; set; } = new List<string>();

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
