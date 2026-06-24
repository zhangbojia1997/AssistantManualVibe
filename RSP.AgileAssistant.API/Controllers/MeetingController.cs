using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Meeting.Actions;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Vo;
using RSP.Common;
using RSP.Common.DataAccess;
using RSP.Common.Logging;
using RSP.Common.MVC;

namespace RSP.AgileAssistant.API.Controllers
{
    /// <summary>
    /// Endpoints for meeting management (FR-MEET) and the voting / estimation core
    /// state machine (FR-VOTE). All real-time updates are delivered through client
    /// polling: write operations simply persist state and return the latest
    /// meeting snapshot; no broadcast is performed.
    /// </summary>
    [ApiController]
    [EnableCors(ConfigConst.ALLOWED_ORIGIN_TAG)]
    [Route("api/[controller]/[action]")]
    public class MeetingController : RSPAPIController
    {
        /// <summary>
        /// Prefix of the HTTP bearer authorization scheme.
        /// </summary>
        private const string BEARER_PREFIX = "Bearer ";

        /// <summary>
        /// Initializes the controller with the framework dependencies.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="commonLogger">Common logger.</param>
        public MeetingController(IConfiguration configuration, ILoggingLogger commonLogger)
            : base(configuration, commonLogger)
        {
        }

        /// <summary>
        /// Lists the meetings available to join (running meetings only).
        /// </summary>
        [HttpGet]
        public async Task<BusinessResult<List<MeetingVo>>> GetMeetings()
        {
            GetMeetingsDto dto = new GetMeetingsDto { AccessToken = this.GetBearerToken() };
            IADOConfigurable config = this.GetADOConfiguration(true);
            return await this.LaunchActionAsync(() => new GetMeetingsActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Retrieves a single meeting by identifier (polling heartbeat).
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to retrieve.</param>
        [HttpGet]
        public async Task<BusinessResult<MeetingVo>> FindMeeting([FromQuery] Guid meetingId)
        {
            FindMeetingDto dto = new FindMeetingDto { MeetingId = meetingId, AccessToken = this.GetBearerToken() };
            IADOConfigurable config = this.GetADOConfiguration(true);
            return await this.LaunchActionAsync(() => new FindMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Creates and starts a new meeting with the caller as host.
        /// </summary>
        /// <param name="dto">Add-meeting request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<MeetingVo>> Start([FromBody] AddMeetingDto dto)
        {
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new StartMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Joins a meeting under a named group.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to join.</param>
        /// <param name="userName">Display name to join with.</param>
        /// <param name="group">Group to join under.</param>
        [HttpGet]
        public async Task<BusinessResult<MeetingVo>> Join(
            [FromQuery] Guid meetingId,
            [FromQuery] string userName,
            [FromQuery] string group)
        {
            JoinMeetingDto dto = new JoinMeetingDto
            {
                MeetingId = meetingId,
                UserName = userName,
                Group = group,
                AccessToken = this.GetBearerToken(),
            };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new JoinMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Joins a meeting without selecting a group.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to join.</param>
        /// <param name="userName">Display name to join with.</param>
        [HttpGet]
        public async Task<BusinessResult<MeetingVo>> JoinNoGroup(
            [FromQuery] Guid meetingId,
            [FromQuery] string userName)
        {
            JoinMeetingDto dto = new JoinMeetingDto
            {
                MeetingId = meetingId,
                UserName = userName,
                Group = string.Empty,
                AccessToken = this.GetBearerToken(),
            };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new JoinMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Leaves a meeting. When the host leaves, the meeting is ended.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to leave.</param>
        /// <param name="userName">Display name of the participant leaving.</param>
        [HttpPost]
        public async Task<BusinessResult<bool>> Leave(
            [FromQuery] Guid meetingId,
            [FromQuery] string userName)
        {
            LeaveMeetingDto dto = new LeaveMeetingDto
            {
                MeetingId = meetingId,
                UserName = userName,
                AccessToken = this.GetBearerToken(),
            };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new LeaveMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Records or clears the caller's card selection for the current round.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting.</param>
        /// <param name="dto">Select-pokers request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<MeetingVo>> SelectPokers(
            [FromQuery] Guid meetingId,
            [FromBody] SelectPokersDto dto)
        {
            dto.MeetingId = meetingId;
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new SelectPokersActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Switches a participant's deck during a meeting.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting.</param>
        /// <param name="userName">Display name of the participant.</param>
        /// <param name="deckId">Identifier of the new deck.</param>
        [HttpPost]
        public async Task<BusinessResult<bool>> UpdateParticipantDeck(
            [FromQuery] Guid meetingId,
            [FromQuery] string userName,
            [FromQuery] Guid deckId)
        {
            UpdateParticipantDeckDto dto = new UpdateParticipantDeckDto
            {
                MeetingId = meetingId,
                UserName = userName,
                DeckId = deckId,
                AccessToken = this.GetBearerToken(),
            };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new UpdateParticipantDeckActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Deletes a meeting and all of its data.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting to delete.</param>
        [HttpGet]
        [Route("/api/[controller]s/{meetingId}")]
        public async Task<BusinessResult<bool>> RemoveMeeting(Guid meetingId)
        {
            RemoveMeetingDto dto = new RemoveMeetingDto { MeetingId = meetingId, AccessToken = this.GetBearerToken() };
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new RemoveMeetingActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Starts a new voting round for the meeting.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting.</param>
        /// <param name="dto">Start-voting-round request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<MeetingVo>> StartVotingRound(
            [FromQuery] Guid meetingId,
            [FromBody] StartVotingRoundDto dto)
        {
            dto.MeetingId = meetingId;
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new StartVotingRoundActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Ends the current voting round, persisting the submitted votes.
        /// </summary>
        /// <param name="meetingId">Identifier of the meeting.</param>
        /// <param name="dto">End-voting-round request payload.</param>
        [HttpPost]
        public async Task<BusinessResult<MeetingVo>> EndVotingRound(
            [FromQuery] Guid meetingId,
            [FromBody] EndVotingRoundDto dto)
        {
            dto.MeetingId = meetingId;
            dto.AccessToken = this.GetBearerToken();
            IADOConfigurable config = this.GetADOConfiguration(true, IsolationLevel.ReadCommitted);
            return await this.LaunchActionAsync(() => new EndVotingRoundActionAsync(config, dto), dto);
        }

        /// <summary>
        /// Extracts the bearer token from the <c>Authorization</c> request header.
        /// </summary>
        /// <returns>The token without its scheme prefix, or an empty string.</returns>
        private string GetBearerToken()
        {
            string header = this.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return header.Substring(BEARER_PREFIX.Length).Trim();
        }
    }
}
