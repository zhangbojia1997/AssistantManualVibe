using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.Meeting.Dto
{
    /// <summary>
    /// Input payload for listing meetings (FR-MEET). The join dropdown only shows
    /// running meetings, so callers can request a running-only result set.
    /// </summary>
    public class GetMeetingsDto : DtoBase
    {
        /// <summary>
        /// When <c>true</c>, only meetings with <c>IsRunning == true</c> are
        /// returned. Defaults to <c>true</c> to match the join-meeting dropdown.
        /// </summary>
        public bool RunningOnly { get; set; } = true;

        /// <summary>
        /// The caller's current access token, used to authorize the request.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
