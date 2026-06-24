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
    /// Creates and starts a new meeting (FR-MEET). The caller becomes the host and
    /// the meeting is marked running so it appears in the join dropdown.
    /// </summary>
    public class StartMeetingActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// Add-meeting request payload.
        /// </summary>
        private readonly AddMeetingDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Add-meeting request payload.</param>
        public StartMeetingActionAsync(IADOConfigurable adoConfigurable, AddMeetingDto dto)
            : base(adoConfigurable)
        {
            this._dto = dto;
        }

        /// <inheritdoc />
        protected override async Task<BusinessResult<MeetingVo>> RunDataOperationAsync(ILoggingLogger loggingLogger)
        {
            try
            {
                if ((this._dto == null) || string.IsNullOrWhiteSpace(this._dto.Topic))
                {
                    return BusinessResult<MeetingVo>.Fail("A meeting topic is required.");
                }

                if (this._dto.DeckId == Guid.Empty)
                {
                    return BusinessResult<MeetingVo>.Fail("A deck id is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The access token is invalid or has expired.");
                }

                DateTime now = DateTime.UtcNow;
                MeetingBo meeting = new MeetingBo
                {
                    Id = Guid.NewGuid(),
                    HostId = claim.UserId,
                    Topic = this._dto.Topic,
                    DeckId = this._dto.DeckId,
                    Status = MeetingBo.StatusRunning,
                    LastActiveDate = now,
                    CreatedOn = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
                    Groups = this._dto.Groups ?? new System.Collections.Generic.List<string>(),
                };

                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<MeetingVo>.Success(MeetingMapper.ToVo(meeting));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<MeetingVo>.Fail("Failed to start the meeting.", ex);
            }
        }
    }
}
