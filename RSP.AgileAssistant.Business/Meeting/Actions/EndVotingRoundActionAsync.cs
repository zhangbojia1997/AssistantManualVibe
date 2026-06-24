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
    /// Ends the current voting round (FR-VOTE). The submitted votes are persisted,
    /// the round is marked done so its results are displayed under the polling
    /// model, and participants' selections are cleared.
    /// </summary>
    public class EndVotingRoundActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// End-voting-round request payload.
        /// </summary>
        private readonly EndVotingRoundDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">End-voting-round request payload.</param>
        public EndVotingRoundActionAsync(IADOConfigurable adoConfigurable, EndVotingRoundDto dto)
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
                if (meeting == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The meeting does not exist.");
                }

                if (meeting.VotingRound == null)
                {
                    return BusinessResult<MeetingVo>.Fail("There is no active voting round.");
                }

                RoundBo? round = meeting.Rounds.FirstOrDefault(r => r.Id == meeting.VotingRound.Value);
                if (round == null)
                {
                    return BusinessResult<MeetingVo>.Fail("The active voting round does not exist.");
                }

                round.Votes = (this._dto.Votes ?? new System.Collections.Generic.List<RoundVoteDto>())
                    .Select(v => new RoundVoteBo { UserId = v.UserId, Value = v.Value })
                    .ToList();
                round.Status = RoundBo.StatusDone;

                foreach (ParticipantBo participant in meeting.Participants)
                {
                    participant.SelectedPoker = null;
                    participant.IsPickedPoker = false;
                }

                meeting.LastActiveDate = DateTime.UtcNow;
                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<MeetingVo>.Success(MeetingMapper.ToVo(meeting));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<MeetingVo>.Fail("Failed to end the voting round.", ex);
            }
        }
    }
}
