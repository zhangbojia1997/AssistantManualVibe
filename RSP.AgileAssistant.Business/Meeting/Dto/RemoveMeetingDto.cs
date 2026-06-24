using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for deleting a meeting (FR-MEET).
    /// </summary>
    public class RemoveMeetingDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting to delete.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
