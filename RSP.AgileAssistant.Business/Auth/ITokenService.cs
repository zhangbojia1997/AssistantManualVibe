using System;
using RSP.AgileAssistant.Business.Auth.Bo;

namespace RSP.AgileAssistant.Business.Auth
{
    /// <summary>
    /// Issues and validates self-contained JWT access tokens for the product.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Creates a signed JWT access token for the supplied user identity.
        /// </summary>
        /// <param name="userId">Unique identifier of the user.</param>
        /// <param name="userName">User name to embed as a claim.</param>
        /// <returns>The signed JWT access token.</returns>
        string CreateToken(Guid userId, string userName);

        /// <summary>
        /// Validates a JWT access token and decodes its identity claims.
        /// </summary>
        /// <param name="token">The JWT access token to validate.</param>
        /// <returns>
        /// The decoded <see cref="TokenClaim"/> when the token is valid and
        /// unexpired; otherwise <c>null</c>.
        /// </returns>
        TokenClaim? DecodeToken(string token);
    }
}
