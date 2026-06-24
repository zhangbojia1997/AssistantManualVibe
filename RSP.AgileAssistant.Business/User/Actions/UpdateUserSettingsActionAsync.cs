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
    /// Updates a user's persisted preferences (FR-USER): full name, default deck,
    /// default group, Jira credentials and the "apply defaults" toggle. The caller
    /// must present a valid access token that belongs to the user being updated.
    /// </summary>
    public class UpdateUserSettingsActionAsync : SqlActionAsyncBase<BusinessResult<UserVo>>
    {
        /// <summary>
        /// Update-settings request payload.
        /// </summary>
        private readonly UpdateUserSettingsDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Update-settings request payload.</param>
        public UpdateUserSettingsActionAsync(IADOConfigurable adoConfigurable, UpdateUserSettingsDto dto)
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

                if (claim.UserId != this._dto.UserId)
                {
                    return BusinessResult<UserVo>.Fail("The access token does not match the requested user.");
                }

                UserBo? user = await this.RunObjQueryAsync(
                    UserSql.SelectById,
                    UserSql.SelectByIdParameters(this._dto.UserId),
                    UserSql.MapUser);

                if (user == null)
                {
                    return BusinessResult<UserVo>.Fail("The user does not exist.");
                }

                DateTime now = DateTime.UtcNow;
                this.ApplySettings(user);
                user.LastActiveDate = now;

                await this.RunNonQueryAsync(
                    UserSql.UpdateSettings,
                    UserSql.UpdateSettingsParameters(user, now));

                return BusinessResult<UserVo>.Success(UserMapper.ToVo(user));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<UserVo>.Fail("Failed to update the user settings.", ex);
            }
        }

        /// <summary>
        /// Applies the requested settings onto the user business object. Jira
        /// credentials are only changed when both the email and token are supplied,
        /// preserving any previously stored credentials otherwise.
        /// </summary>
        /// <param name="user">User business object to mutate.</param>
        private void ApplySettings(UserBo user)
        {
            user.FullName = this._dto.FullName;
            user.DefaultDeckId = this._dto.DeckId?.ToString();
            user.DefaultGroup = this._dto.Group;
            user.UseDefaults = this._dto.UseDefaults;

            if (!string.IsNullOrWhiteSpace(this._dto.JiraEmail) && !string.IsNullOrWhiteSpace(this._dto.JiraToken))
            {
                user.JiraEmail = this._dto.JiraEmail;
                user.JiraToken = this._dto.JiraToken;
            }
        }
    }
}
