using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RSP.AgileAssistant.Business.Auth.Bo;

namespace RSP.AgileAssistant.Business.Auth
{
    /// <summary>
    /// Default <see cref="ITokenService"/> implementation that signs tokens with
    /// a symmetric secret using the HMAC-SHA256 algorithm.
    /// </summary>
    public class TokenService : ITokenService
    {
        /// <summary>
        /// Number of days an issued access token remains valid.
        /// </summary>
        private const int TOKEN_LIFETIME_DAYS = 7;

        /// <summary>
        /// Minimum acceptable length, in bytes, of the signing secret. HMAC-SHA256
        /// requires a key of at least 256 bits.
        /// </summary>
        private const int MINIMUM_SECRET_LENGTH = 32;

        /// <summary>
        /// Symmetric signing key derived from the configured secret.
        /// </summary>
        private readonly SymmetricSecurityKey _signingKey;

        /// <summary>
        /// Initializes the service with the configured signing secret.
        /// </summary>
        /// <param name="secret">Symmetric secret used to sign and validate tokens.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the secret is missing or shorter than the minimum length.
        /// </exception>
        public TokenService(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret) || (Encoding.UTF8.GetByteCount(secret) < MINIMUM_SECRET_LENGTH))
            {
                throw new ArgumentException("JWT signing secret must be configured and at least 32 bytes long.", nameof(secret));
            }

            this._signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        /// <inheritdoc />
        public string CreateToken(Guid userId, string userName)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User id must not be empty.", nameof(userId));
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName ?? string.Empty),
            };

            SigningCredentials credentials = new SigningCredentials(this._signingKey, SecurityAlgorithms.HmacSha256);
            DateTime now = DateTime.UtcNow;
            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.AddDays(TOKEN_LIFETIME_DAYS),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc />
        public TokenClaim? DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = this._signingKey,
                ClockSkew = TimeSpan.Zero,
            };

            try
            {
                ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParameters, out _);

                string? userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdValue, out Guid userId))
                {
                    return null;
                }

                return new TokenClaim
                {
                    UserId = userId,
                    UserName = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                };
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
