using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.User.Dto
{
    /// <summary>
    /// Input payload for the login / auto-registration endpoint.
    /// </summary>
    public class LoginDto : DtoBase
    {
        /// <summary>
        /// Login name supplied by the user. Ignored for guest logins.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Password supplied by the user. Reserved for future credential
        /// validation against the corporate directory.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// When <c>true</c>, an anonymous guest account is created and used.
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// Indicates whether the user should be assigned the scrum master role.
        /// </summary>
        public bool ScrumMaster { get; set; }
    }
}
