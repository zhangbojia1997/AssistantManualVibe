using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for joining a meeting (FR-MEET). The same-named participant
    /// reuses its existing identifier when re-joining.
    /// </summary>
    public class JoinMeetingDto : DtoBase
    {
        /// <summary>
        /// Identifier of the meeting to join.
        /// </summary>
        [Required]
        public Guid MeetingId { get; set; }

        /// <summary>
        /// Display name the participant joins with.
        /// </summary>
        [Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Group the participant joins under. Empty for the "no group" flow.
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// The caller's current access token, used to authorize the request and to
        /// resolve the participant's user identity and default deck.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
