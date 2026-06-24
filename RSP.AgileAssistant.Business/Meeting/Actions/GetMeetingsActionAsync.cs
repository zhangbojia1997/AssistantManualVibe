using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lists meetings for the join dropdown (FR-MEET). By default only running
    /// meetings are returned so users cannot join an ended session.
    /// </summary>
    public class GetMeetingsActionAsync : MeetingActionBase<BusinessResult<List<MeetingVo>>>
    {
        /// <summary>
        /// Get-meetings request payload.
        /// </summary>
        private readonly GetMeetingsDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Get-meetings request payload.</param>
        public GetMeetingsActionAsync(IADOConfigurable adoConfigurable, GetMeetingsDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <inheritdoc />
        protected override async Task<BusinessResult<List<MeetingVo>>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                TokenClaim? claim = this.TokenService.DecodeToken(this._dto?.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<List<MeetingVo>>.Fail("The access token is invalid or has expired.");
                }

                bool runningOnly = this._dto?.RunningOnly ?? true;
                List<MeetingBo> meetings = await this.LoadMeetingsAsync(runningOnly);

                List<MeetingVo> result = meetings.Select(MeetingMapper.ToVo).ToList();
                return BusinessResult<List<MeetingVo>>.Success(result);
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<List<MeetingVo>>.Fail("Failed to retrieve the meetings.", ex);
            }
        }
    }
}
