using System;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Actions;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Consts;
using RSP.AgileAssistant.Business.User.Bo;
using RSP.AgileAssistant.Business.User.Dto;
using RSP.AgileAssistant.Business.User.Sql;
using RSP.AgileAssistant.Business.User.Vo;
using RSP.Common;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.User.Actions
{
    /// <summary>
    /// Validates a presented access token and issues a fresh one. The presented
    /// token must match the token currently stored for the user, so logged-out or
    /// superseded tokens cannot be refreshed.
    /// </summary>
    public class RefreshTokenActionAsync : SqlActionAsyncBase<BusinessResult<UserVo>>
    {
        /// <summary>
        /// Refresh request payload.
        /// </summary>
        private readonly RefreshTokenDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Refresh request payload.</param>
        public RefreshTokenActionAsync(IADOConfigurable adoConfigurable, RefreshTokenDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <summary>
        /// JWT token service used to validate and re-issue access tokens.
        /// </summary>
        [IOCInjection(InjectionKey = IocConsts.TOKEN_SERVICE)]
        public ITokenService TokenService { get; private set; } = null!;

        /// <inheritdoc />
        protected override async Task<BusinessResult<UserVo>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || string.IsNullOrWhiteSpace(this._dto.AccessToken))
                {
                    return BusinessResult<UserVo>.Fail("An access token is required to refresh.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken);
                if (claim == null)
                {
                    return BusinessResult<UserVo>.Fail("The access token is invalid or has expired.");
                }

                UserBo? user = await this.RunObjQueryAsync(
                    UserSql.SelectById,
                    UserSql.SelectByIdParameters(claim.UserId),
                    UserSql.MapUser);

                if (user == null)
                {
                    return BusinessResult<UserVo>.Fail("The user no longer exists.");
                }

                if (!string.Equals(user.RefreshToken, this._dto.AccessToken, StringComparison.Ordinal))
                {
                    return BusinessResult<UserVo>.Fail("The access token has been revoked.");
                }

                DateTime now = DateTime.UtcNow;
                string newToken = this.TokenService.CreateToken(user.Id, user.UserName);
                user.RefreshToken = newToken;
                user.LastActiveDate = now;

                await this.RunNonQueryAsync(
                    UserSql.UpdateRefreshToken,
                    UserSql.UpdateRefreshTokenParameters(user.Id, newToken, user.ScrumMaster, now));

                return BusinessResult<UserVo>.Success(UserMapper.ToVo(user, newToken));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<UserVo>.Fail("Failed to refresh the access token.", ex);
            }
        }
    }
}
