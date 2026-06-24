using System;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth;
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
    /// Authenticates a user and issues a JWT access token. Unknown users are
    /// auto-registered, and anonymous guest accounts are created on demand,
    /// matching the product's onboarding flow.
    /// </summary>
    public class LoginActionAsync : SqlActionAsyncBase<BusinessResult<UserVo>>
    {
        /// <summary>
        /// Prefix applied to generated guest account names.
        /// </summary>
        private const string GUEST_NAME_PREFIX = "Guest-";

        /// <summary>
        /// Login request payload.
        /// </summary>
        private readonly LoginDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Login request payload.</param>
        public LoginActionAsync(IADOConfigurable adoConfigurable, LoginDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <summary>
        /// JWT token service used to issue access tokens.
        /// </summary>
        [IOCInjection(InjectionKey = IocConsts.TOKEN_SERVICE)]
        public ITokenService TokenService { get; private set; } = null!;

        /// <inheritdoc />
        protected override async Task<BusinessResult<UserVo>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                this.ValidateInput();

                DateTime now = DateTime.UtcNow;
                UserBo user = await this.ResolveUserAsync(now);

                user.ScrumMaster = this._dto.ScrumMaster;
                string accessToken = this.TokenService.CreateToken(user.Id, user.UserName);
                user.RefreshToken = accessToken;
                user.LastActiveDate = now;

                await this.RunNonQueryAsync(
                    UserSql.UpdateRefreshToken,
                    UserSql.UpdateRefreshTokenParameters(user.Id, accessToken, user.ScrumMaster, now));

                return BusinessResult<UserVo>.Success(UserMapper.ToVo(user, accessToken));
            }
            catch (ArgumentException ex)
            {
                loggingLogger.LogWarning(ex.Message);
                return BusinessResult<UserVo>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<UserVo>.Fail("Login failed due to an unexpected error.", ex);
            }
        }

        /// <summary>
        /// Validates the login request payload.
        /// </summary>
        private void ValidateInput()
        {
            if (this._dto == null)
            {
                throw new ArgumentException("Login request must not be null.");
            }

            if (!this._dto.IsGuest && string.IsNullOrWhiteSpace(this._dto.UserName))
            {
                throw new ArgumentException("User name is required for a non-guest login.");
            }
        }

        /// <summary>
        /// Resolves the user for the request: creates a guest account, returns an
        /// existing user, or auto-registers a new named user.
        /// </summary>
        /// <param name="now">Current UTC timestamp used for new records.</param>
        private async Task<UserBo> ResolveUserAsync(DateTime now)
        {
            if (this._dto.IsGuest)
            {
                return await this.CreateUserAsync(GUEST_NAME_PREFIX + Guid.NewGuid(), true, now);
            }

            string userName = this._dto.UserName!.Trim();
            UserBo? existing = await this.RunObjQueryAsync(
                UserSql.SelectByUserName,
                UserSql.SelectByUserNameParameters(userName),
                UserSql.MapUser);

            return existing ?? await this.CreateUserAsync(userName, false, now);
        }

        /// <summary>
        /// Inserts a new user row and returns the created business object.
        /// </summary>
        /// <param name="userName">Login name for the new user.</param>
        /// <param name="isGuest">Whether the account is a guest account.</param>
        /// <param name="now">Current UTC timestamp.</param>
        private async Task<UserBo> CreateUserAsync(string userName, bool isGuest, DateTime now)
        {
            UserBo user = new UserBo
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                IsGuest = isGuest,
                ScrumMaster = this._dto.ScrumMaster,
                LastActiveDate = now,
            };

            await this.RunNonQueryAsync(
                UserSql.Insert,
                UserSql.InsertParameters(user, now));

            return user;
        }
    }
}
