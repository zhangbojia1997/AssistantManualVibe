using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for retrieving a single meeting by identifier (FR-MEET). Also
    /// serves as the per-second heartbeat polling entry point.
    /// </summary>
    public class FindMeetingDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting to retrieve.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
