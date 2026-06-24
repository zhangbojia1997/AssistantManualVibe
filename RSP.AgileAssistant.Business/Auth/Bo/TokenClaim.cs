using System;

namespace RSP.AgileAssistant.Business.Auth.Bo
{
    /// <summary>
    /// Identity information decoded from a validated JWT access token.
    /// </summary>
    public class TokenClaim
    {
        /// <summary>
        /// Unique identifier of the authenticated user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User name of the authenticated user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
