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
    /// Unit tests for <see cref="StartMeetingActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class StartMeetingActionAsyncTest : ActionTestBase
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
        public async Task StartMeeting_WhenValid_PersistsMeeting()
        {
            Guid hostId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            AddMeetingDto dto = new AddMeetingDto
            {
                Topic = "Sprint planning",
                DeckId = Guid.NewGuid(),
                Groups = new List<string> { "Team A" },
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<StartMeetingActionAsync>[] conditions = new IUnitTestCondition<StartMeetingActionAsync>[]
            {
                new MeetingCountCondition<StartMeetingActionAsync>(ado, 1),
            };

            await this.TestActionAsync<StartMeetingActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task StartMeeting_WhenTopicMissing_Fails()
        {
            Guid hostId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            AddMeetingDto dto = new AddMeetingDto
            {
                Topic = string.Empty,
                DeckId = Guid.NewGuid(),
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<StartMeetingActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Fail(),
                this._mocks);
        }
    }
}
