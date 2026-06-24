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
    /// Switches a participant's deck during a meeting (FR-VOTE).
    /// </summary>
    public class UpdateParticipantDeckActionAsync : MeetingActionBase<BusinessResult<bool>>
    {
        /// <summary>
        /// Update-participant-deck request payload.
        /// </summary>
        private readonly UpdateParticipantDeckDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Update-participant-deck request payload.</param>
        public UpdateParticipantDeckActionAsync(IADOConfigurable adoConfigurable, UpdateParticipantDeckDto dto)
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

                if (this._dto.DeckId == Guid.Empty)
                {
                    return BusinessResult<bool>.Fail("A deck id is required.");
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
                if (participant == null)
                {
                    return BusinessResult<bool>.Fail("The participant is not part of the meeting.");
                }

                participant.DeckId = this._dto.DeckId;
                meeting.LastActiveDate = DateTime.UtcNow;
                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<bool>.Fail("Failed to update the participant deck.", ex);
            }
        }
    }
}
