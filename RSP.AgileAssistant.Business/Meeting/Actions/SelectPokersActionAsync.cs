using System;
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
    /// Records (or clears) a participant's selected card during a voting round
    /// (FR-VOTE). A null selection clears the participant's pick.
    /// </summary>
    public class SelectPokersActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// Select-pokers request payload.
        /// </summary>
        private readonly SelectPokersDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Select-pokers request payload.</param>
        public SelectPokersActionAsync(IADOConfigurable adoConfigurable, SelectPokersDto dto)
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

                if (string.IsNullOrWhiteSpace(this._dto.UserName))
                {
                    return BusinessResult<MeetingVo>.Fail("A user name is required.");
                }

                TokenClaim? claim = this.TokenService.DecodeToken(this._dto.AccessToken ?? string.Empty);
                if (claim == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The access token is invalid or has expired.");
                }

                MeetingBo? meeting = await this.LoadMeetingAsync(this._dto.MeetingId);
                if (meeting == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The meeting does not exist.");
                }

                ParticipantBo? participant = meeting.Participants
                    .FirstOrDefault(p => string.Equals(p.Name, this._dto.UserName, StringComparison.Ordinal));
                if (participant == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The participant is not part of the meeting.");
                }

                if (this._dto.SelectedPoker == null)
                {
                    participant.SelectedPoker = null;
                    participant.IsPickedPoker = false;
                }
                else
                {
                    ParticipantPokerDto poker = this._dto.SelectedPoker;
                    participant.SelectedPoker = new ParticipantPokerBo
                    {
                        Id = poker.Id == Guid.Empty ? Guid.NewGuid() : poker.Id,
                        ParticipantName = string.IsNullOrEmpty(poker.ParticipantName) ? participant.Name : poker.ParticipantName,
                        PokerId = poker.PokerId,
                        OriginalPokerValue = poker.OriginalPokerValue,
                        PokerValue = poker.PokerValue,
                    };
                    participant.IsPickedPoker = true;
                }

                meeting.LastActiveDate = DateTime.UtcNow;
                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<MeetingVo>.Success(MeetingMapper.ToVo(meeting));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<MeetingVo>.Fail("Failed to record the card selection.", ex);
            }
        }
    }
}
