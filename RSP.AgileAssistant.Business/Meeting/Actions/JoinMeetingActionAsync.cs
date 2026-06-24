using System;
using System.Linq;
using System.Threading.Tasks;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Vo;
using RSP.AgileAssistant.Business.User.Bo;
using RSP.Common.DataAccess;
using RSP.Common.Logging;

namespace RSP.AgileAssistant.Business.Meeting.Actions
{
    /// <summary>
    /// Adds the caller to a meeting as a participant (FR-MEET). A participant with
    /// the same name reuses its existing identity (re-join). The participant's deck
    /// defaults to the user's default deck and falls back to the meeting deck.
    /// </summary>
    public class JoinMeetingActionAsync : MeetingActionBase<BusinessResult<MeetingVo>>
    {
        /// <summary>
        /// Join-meeting request payload.
        /// </summary>
        private readonly JoinMeetingDto _dto;

        /// <summary>
        /// Initializes the action with the database configuration and request.
        /// </summary>
        /// <param name="adoConfigurable">Database connection configuration.</param>
        /// <param name="dto">Join-meeting request payload.</param>
        public JoinMeetingActionAsync(IADOConfigurable adoConfigurable, JoinMeetingDto dto)
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

                UserBo? user = await this.GetUserAsync(claim.UserId);
                Guid deckId = ResolveDeckId(user, meeting.DeckId);
                string group = this._dto.Group ?? string.Empty;

                ParticipantBo? existing = meeting.Participants
                    .FirstOrDefault(p => string.Equals(p.Name, this._dto.UserName, StringComparison.Ordinal));

                if (existing != null)
                {
                    existing.UserId = claim.UserId;
                    existing.DeckId = deckId;
                    existing.Group = group;
                }
                else
                {
                    meeting.Participants.Add(new ParticipantBo
                    {
                        Id = Guid.NewGuid(),
                        UserId = claim.UserId,
                        Name = this._dto.UserName,
                        DeckId = deckId,
                        Group = group,
                        IsPickedPoker = false,
                        SelectedPoker = null,
                    });
                }

                meeting.LastActiveDate = DateTime.UtcNow;
                await this.SaveMeetingSnapshotAsync(meeting);
                return BusinessResult<MeetingVo>.Success(MeetingMapper.ToVo(meeting));
            }
            catch (Exception ex)
            {
                loggingLogger.LogFatal(ex, ex.Message);
                return BusinessResult<MeetingVo>.Fail("Failed to join the meeting.", ex);
            }
        }

        /// <summary>
        /// Resolves the deck a joining participant should use: the user's default
        /// deck when set, otherwise the meeting's deck.
        /// </summary>
        /// <param name="user">The joining user, if found.</param>
        /// <param name="meetingDeckId">The meeting's default deck.</param>
        private static Guid ResolveDeckId(UserBo? user, Guid meetingDeckId)
        {
            if (user != null
                && !string.IsNullOrWhiteSpace(user.DefaultDeckId)
                && Guid.TryParse(user.DefaultDeckId, out Guid defaultDeck)
                && defaultDeck != Guid.Empty)
            {
                return defaultDeck;
            }

            return meetingDeckId;
        }
    }
}
