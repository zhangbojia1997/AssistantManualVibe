using System;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Sql;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.Meeting.Actions
{
    /// <summary>
    /// Deletes a meeting and all of its child data (FR-MEET). Fails when the
    /// meeting does not exist.
    /// </summary>
    public class RemoveMeetingActionAsync : MeetingActionBase<BusinessResult<bool>>
    {
        /// <summary>
        /// Remove-meeting request payload.
        /// </summary>
        private readonly RemoveMeetingDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Remove-meeting request payload.</param>
        public RemoveMeetingActionAsync(IADOConfigurable adoConfigurable, RemoveMeetingDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <inheritdoc />
        protected override async Task<BusinessResult<bool>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || (this._dto.MeetingId == Guid.Empty))
                {
                    return BusinessResult<bool>.Fail("A meeting id is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<bool>.Fail("The access token is invalid or has expired.");
                }

                int exists = await this.RunObjQueryAsync(
                    MeetingSql.MeetingExists,
                    MeetingSql.IdParameters(this._dto.MeetingId),
                    row => Convert.ToInt32(row[0]));

                if (exists == 0)
                {
                    return BusinessResult<bool>.Fail("The meeting does not exist.");
                }

                await this.RunNonQueryAsync(MeetingSql.DeleteMeeting, MeetingSql.IdParameters(this._dto.MeetingId));
                return BusinessResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<bool>.Fail("Failed to remove the meeting.", ex);
            }
        }
    }
}
