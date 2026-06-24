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
    /// Starts a new voting round for a meeting topic (FR-VOTE). The round number
    /// increments per topic and participants' previous selections are cleared.
    /// </summary>
    public class StartVotingRoundActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// Start-voting-round request payload.
        /// </summary>
        private readonly StartVotingRoundDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Start-voting-round request payload.</param>
        public StartVotingRoundActionAsync(IADOConfigurable adoConfigurable, StartVotingRoundDto dto)
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

                int lastRoundNumber = meeting.Rounds
                    .Where(r => r.TopicId == this._dto.VotingOn)
                    .Select(r => r.RoundNumber)
                    .DefaultIfEmpty(0)
                    .Max();

                RoundBo round = new RoundBo
                {
                    Id = Guid.NewGuid(),
                    TopicId = this._dto.VotingOn,
                    RoundNumber = lastRoundNumber + 1,
                    Status = RoundBo.StatusVoting,
                };
                meeting.Rounds.Add(round);

                meeting.VotingOn = this._dto.VotingOn;
                meeting.VotingRound = round.Id;

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
                return BusinessResult<MeetingVo>.Fail("Failed to start the voting round.", ex);
            }
        }
    }
}
