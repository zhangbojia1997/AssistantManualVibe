using System;
using System.ComponentModel.DataAnnotations;
using RSP.Common.Business;

namespace RSP.AgileAssistant.Business.User.Dto
{
    /// <summary>
    /// Input payload for logging a user out (revoking their refresh token).
    /// </summary>
    public class LogoutDto : DtoBase
    {
        /// <summary>
        /// Unique identifier of the user to log out.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// The caller's current access token, used to authorize the logout.
        /// </summary>
        public string? AccessToken { get; set; }
    }
}
