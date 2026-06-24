using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for leaving a meeting (FR-MEET). When the leaving participant
    /// is the host, the meeting is ended.
    /// </summary>
    public class LeaveMeetingDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting to leave.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Display name of the participant leaving.
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
