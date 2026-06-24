using System;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Vo;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.Meeting.Actions
{
    /// <summary>
    /// Retrieves a single meeting by identifier (FR-MEET). Also serves as the
    /// per-second heartbeat polling entry point for clients.
    /// </summary>
    public class FindMeetingActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// Find-meeting request payload.
        /// </summary>
        private readonly FindMeetingDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Find-meeting request payload.</param>
        public FindMeetingActionAsync(IADOConfigurable adoConfigurable, FindMeetingDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <inheritdoc />
        protected override async Task<BusinessResult<MeetingVo>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || (this._dto.MeetingId == Guid.Empty))
                {
                    return BusinessResult<MeetingVo>.Fail("A meeting id is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The access token is invalid or has expired.");
                }

                MeetingBo? meeting = await this.LoadMeetingAsync(this._dto.MeetingId);
                return meeting != null
                    ? BusinessResult<MeetingVo>.Success(MeetingMapper.ToVo(meeting))
                    : BusinessResult<MeetingVo>.Fail("The meeting does not exist.");
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<MeetingVo>.Fail("Failed to retrieve the meeting.", ex);
            }
        }
    }
}
