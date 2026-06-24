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
    /// Retrieves a single user by identifier. The caller must present a valid
    /// access token whose identity matches the requested user.
    /// </summary>
    public class GetUserActionAsync : SqlActionAsyncBase<BusinessResult<UserVo>>
    {
        /// <summary>
        /// Get-user request payload.
        /// </summary>
        private readonly GetUserDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Get-user request payload.</param>
        public GetUserActionAsync(IADOConfigurable adoConfigurable, GetUserDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <summary>
        /// JWT token service used to authorize the request.
        /// </summary>
        [IOCInjection(InjectionKey = IocConsts.TOKEN_SERVICE)]
        public ITokenService TokenService { get; private set; } = null!;

        /// <inheritdoc />
        protected override async Task<BusinessResult<UserVo>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || (this._dto.UserId == Guid.Empty))
                {
                    return BusinessResult<UserVo>.Fail("A user id is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<UserVo>.Fail("The access token is invalid or has expired.");
                }

                UserBo? user = await this.RunObjQueryAsync(
                    UserSql.SelectById,
                    UserSql.SelectByIdParameters(this._dto.UserId),
                    UserSql.MapUser);

                return user != null
                    ? BusinessResult<UserVo>.Success(UserMapper.ToVo(user))
                    : BusinessResult<UserVo>.Fail("The user does not exist.");
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<UserVo>.Fail("Failed to retrieve the user.", ex);
            }
        }
    }
}
