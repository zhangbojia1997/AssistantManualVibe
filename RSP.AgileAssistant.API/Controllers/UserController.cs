using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.User.Actions;
using RSP.AgileAssistant.Business.User.Dto;
using RSP.AgileAssistant.Business.User.Vo;
using RSP.Common;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.MVC;

namespace RSP.AgileAssistant.API.Controllers
{
    /// <summary>
    /// Endpoints for user authentication and session management (FR-AUTH):
    /// login / auto-registration, token refresh, logout and user lookup.
    /// </summary>
    [ApiController]
    [EnableCors(ConfigConst.ALLOWED_ORIGIN_TAG)]
    [Route("api/[controller]/[action]")]
    public class UserController : RSPAPIController
    {
        /// <summary>
        /// Prefix of the HTTP bearer authorization scheme.
        /// </summary>
        private const string BEARER_PREFIX = "Bearer ";

        /// <summary>
        /// Initializes the controller with the framework dependencies.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="commonLogger">Common logger.</param>
        public UserController(IConfiguration configuration, ILoggingLogger commonLogger)
            : base(configuration, commonLogger)
        {
        }

        /// <summary>
        /// Authenticates a user, auto-registering unknown or guest users, and
        /// returns the user profile together with a freshly issued access token.
        /// </summary>
        /// <param name="dto">Login request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<UserVo>> Login([FromBody] LoginDto dto)
        {
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new LoginActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Re-issues an access token for the caller based on the bearer token
        /// presented in the <c>Authorization</c> header.
        /// </summary>
        [HttpPost]
        public async Task<BusinessResult<UserVo>> Refresh()
        {
            RefreshTokenDto dto = new RefreshTokenDto { AccessToken = this.GetBearerToken() };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new RefreshTokenActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Logs the specified user out by revoking their stored refresh token.
        /// </summary>
        /// <param name="dto">Logout request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<bool>> Logout([FromBody] LogoutDto dto)
        {
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new LogoutActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Retrieves a user profile by identifier.
        /// </summary>
        /// <param name="dto">Get-user request payload.</param>
        [HttpGet]
        public async Task<BusinessResult<UserVo>> GetUser([FromQuery] GetUserDto dto)
        {
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true);
            return await this.LaunchActionAsync(() => new GetUserActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Persists the caller's user preferences (FR-USER): full name, default
        /// deck, default group, Jira credentials and the "apply defaults" toggle.
        /// </summary>
        /// <param name="userId">Identifier of the user whose settings to update.</param>
        /// <param name="dto">Update-settings request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<UserVo>> UpdateSettings([FromQuery] Guid userId, [FromBody] UpdateUserSettingsDto dto)
        {
            dto.UserId = userId;
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new UpdateUserSettingsActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Extracts the bearer token from the <c>Authorization</c> request header.
        /// </summary>
        /// <returns>The token without its scheme prefix, or an empty string.</returns>
        private string GetBearerToken()
        {
            string header = this.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return header.Substring(BEARER_PREFIX.Length).Trim();
        }
    }
}
