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
    /// Unit tests for <see cref="StartVotingRoundActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class StartVotingRoundActionAsyncTest : ActionTestBase
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
        public async Task StartVotingRound_WhenMeetingExists_CreatesVotingRound()
        {
            Guid hostId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            StartVotingRoundDto dto = new StartVotingRoundDto
            {
                MeetingId = meetingId,
                VotingOn = Guid.NewGuid(),
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<StartVotingRoundActionAsync>[] conditions = new IUnitTestCondition<StartVotingRoundActionAsync>[]
            {
                new RoundStatusCondition<StartVotingRoundActionAsync>(ado, meetingId.ToString(), RoundBo.StatusVoting, 1),
            };

            await this.TestActionAsync<StartVotingRoundActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task StartVotingRound_WhenMeetingMissing_Fails()
        {
            Guid hostId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            StartVotingRoundDto dto = new StartVotingRoundDto
            {
                MeetingId = Guid.NewGuid(),
                VotingOn = Guid.NewGuid(),
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<StartVotingRoundActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Fail(),
                this._mocks);
        }
    }
}
