using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Consts;
using RSP.AgileAssistant.Business.Meeting.Actions;
using RSP.AgileAssistant.Business.Meeting.Bo;
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Vo;
using RSP.AgileAssistant.Business.Test.Base;
using RSP.AgileAssistant.Business.Test.UnitTestConditions;
using RSP.Common.DataAccess;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.Meeting.Actions
{
    /// <summary>
    /// Unit tests for <see cref="EndVotingRoundActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class EndVotingRoundActionAsyncTest : ActionTestBase
    {
        private const string AccessToken = "active-access-token";

        private Mock<ITokenService> _mockTokenService = null!;

        private IDictionary<string, object> _mocks = null!;

        [SetUp]
        public void InitializeMocks()
        {
            this._mockTokenService = new Mock<ITokenService>();
            this._mocks = new Dictionary<string, object>
            {
                { IocConsts.TOKEN_SERVICE, this._mockTokenService.Object },
            };
        }

        [Test]
        public async Task EndVotingRound_WhenRoundActive_PersistsVotesAndMarksDone()
        {
            Guid hostId = Guid.NewGuid();
            Guid voterId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid topicId = Guid.NewGuid();
            Guid roundId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(voterId, "voter", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());
            this.InsertRound(meetingId, roundId, topicId, 1, RoundBo.StatusVoting);
            this.SetMeetingVoting(meetingId, topicId, roundId);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            EndVotingRoundDto dto = new EndVotingRoundDto
            {
                MeetingId = meetingId,
                Votes = new List<RoundVoteDto>
                {
                    new RoundVoteDto { UserId = voterId, Value = "8" },
                },
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<EndVotingRoundActionAsync>[] conditions = new IUnitTestCondition<EndVotingRoundActionAsync>[]
            {
                new RoundStatusCondition<EndVotingRoundActionAsync>(ado, meetingId.ToString(), RoundBo.StatusDone, 1),
                new VoteExistsCondition<EndVotingRoundActionAsync>(ado, roundId.ToString(), voterId.ToString(), "8"),
            };

            await this.TestActionAsync<EndVotingRoundActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task EndVotingRound_WhenNoActiveRound_Fails()
        {
            Guid hostId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            EndVotingRoundDto dto = new EndVotingRoundDto
            {
                MeetingId = meetingId,
                Votes = new List<RoundVoteDto>(),
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<EndVotingRoundActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Fail(),
                this._mocks);
        }
    }
}
