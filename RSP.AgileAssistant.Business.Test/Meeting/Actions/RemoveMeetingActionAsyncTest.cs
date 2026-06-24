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
using RSP.AgileAssistant.Business.Test.Base;
using RSP.AgileAssistant.Business.Test.UnitTestConditions;
using RSP.Common.DataAccess;
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.Meeting.Actions
{
    /// <summary>
    /// Unit tests for <see cref="RemoveMeetingActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class RemoveMeetingActionAsyncTest : ActionTestBase
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
        public async Task RemoveMeeting_WhenMeetingExists_DeletesMeeting()
        {
            Guid hostId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            RemoveMeetingDto dto = new RemoveMeetingDto { MeetingId = meetingId, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<RemoveMeetingActionAsync>[] conditions = new IUnitTestCondition<RemoveMeetingActionAsync>[]
            {
                new MeetingExistsCondition<RemoveMeetingActionAsync>(ado, meetingId.ToString(), false),
            };

            await this.TestActionAsync<RemoveMeetingActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Success(true),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task RemoveMeeting_WhenMeetingMissing_Fails()
        {
            Guid hostId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            RemoveMeetingDto dto = new RemoveMeetingDto { MeetingId = Guid.NewGuid(), AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<RemoveMeetingActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Fail(),
                this._mocks);
        }
    }
}
