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
using RSP.AgileAssistant.Business.Meeting.Dto;
using RSP.AgileAssistant.Business.Meeting.Vo;
using RSP.AgileAssistant.Business.Test.Base;
using RSP.AgileAssistant.Business.Test.UnitTestConditions;
using RSP.Common.DataAccess;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.Meeting.Actions
{
    /// <summary>
    /// Unit tests for <see cref="JoinMeetingActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class JoinMeetingActionAsyncTest : ActionTestBase
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
        public async Task JoinMeeting_WhenNewParticipant_AddsParticipant()
        {
            Guid hostId = Guid.NewGuid();
            Guid joinerId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(joinerId, "alice", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = joinerId, UserName = "alice" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            JoinMeetingDto dto = new JoinMeetingDto
            {
                MeetingId = meetingId,
                UserName = "alice",
                Group = "Team A",
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<JoinMeetingActionAsync>[] conditions = new IUnitTestCondition<JoinMeetingActionAsync>[]
            {
                new ParticipantExistsCondition<JoinMeetingActionAsync>(ado, meetingId.ToString(), "alice", true),
            };

            await this.TestActionAsync<JoinMeetingActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task JoinMeeting_WhenMeetingMissing_Fails()
        {
            Guid joinerId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = joinerId, UserName = "alice" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            JoinMeetingDto dto = new JoinMeetingDto
            {
                MeetingId = Guid.NewGuid(),
                UserName = "alice",
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<JoinMeetingActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Fail(),
                this._mocks);
        }
    }
}
