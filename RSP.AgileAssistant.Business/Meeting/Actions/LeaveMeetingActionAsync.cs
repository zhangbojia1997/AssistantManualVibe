using System;
using System.Linq;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.Meeting.Actions
{
    /// <summary>
    /// Removes the caller from a meeting (FR-MEET). When the host leaves, the
    /// meeting is ended (marked not running) and its participants are cleared.
    /// </summary>
    public class LeaveMeetingActionAsync : MeetingActionBase<BusinessResult<bool>>
    {
        /// <summary>
        /// Leave-meeting request payload.
        /// </summary>
        private readonly LeaveMeetingDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Leave-meeting request payload.</param>
        public LeaveMeetingActionAsync(IADOConfigurable adoConfigurable, LeaveMeetingDto dto)
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

                if (string.IsNullOrWhiteSpace(this._dto.UserName))
                {
                    return BusinessResult<bool>.Fail("A user name is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<bool>.Fail("The access token is invalid or has expired.");
                }

                MeetingBo? meeting = await this.LoadMeetingAsync(this._dto.MeetingId);
                if (meeting == null)
                {
                    return BusinessResult<bool>.Fail("The meeting does not exist.");
                }

                ParticipantBo? participant = meeting.Participants
                    .FirstOrDefault(p => string.Equals(p.Name, this._dto.UserName, StringComparison.Ordinal));

                bool isHost = participant != null && meeting.HostId == participant.UserId;

                if (isHost)
                {
                    meeting.Status = MeetingBo.StatusEnded;
                    meeting.Participants.Clear();
                }
                else if (participant != null)
                {
                    meeting.Participants.Remove(participant);
                }

                meeting.LastActiveDate = DateTime.UtcNow;
                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<bool>.Fail("Failed to leave the meeting.", ex);
            }
        }
    }
}
