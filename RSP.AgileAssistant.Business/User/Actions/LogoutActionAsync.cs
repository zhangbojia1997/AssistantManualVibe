using System;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Actions;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Consts;
using RSP.AgileAssistant.Business.User.Dto;
using RSP.AgileAssistant.Business.User.Sql;
using RSP.Common;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.User.Actions
{
    /// <summary>
    /// Logs a user out by clearing their stored refresh token, which immediately
    /// revokes any outstanding access tokens for that user.
    /// </summary>
    public class LogoutActionAsync : SqlActionAsyncBase<BusinessResult<bool>>
    {
        /// <summary>
        /// Logout request payload.
        /// </summary>
        private readonly LogoutDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Logout request payload.</param>
        public LogoutActionAsync(IADOConfigurable adoConfigurable, LogoutDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <summary>
        /// JWT token service used to authorize the logout request.
        /// </summary>
        [IOCInjection(InjectionKey = IocConsts.TOKEN_SERVICE)]
        public ITokenService TokenService { get; private set; } = null!;

        /// <inheritdoc />
        protected override async Task<BusinessResult<bool>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || (this._dto.UserId == Guid.Empty))
                {
                    return BusinessResult<bool>.Fail("A user id is required to log out.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if ((claim == null) || (claim.UserId != this._dto.UserId))
                {
                    return BusinessResult<bool>.Fail("The access token is invalid for this user.");
                }

                int affected = await this.RunNonQueryAsync(
                    UserSql.ClearRefreshToken,
                    UserSql.ClearRefreshTokenParameters(this._dto.UserId, DateTime.UtcNow));

                return affected > 0
                    ? BusinessResult<bool>.Success(true)
                    : BusinessResult<bool>.Fail("The user does not exist.");
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<bool>.Fail("Failed to log the user out.", ex);
            }
        }
    }
}
