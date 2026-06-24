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
    /// Unit tests for <see cref="LeaveMeetingActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class LeaveMeetingActionAsyncTest : ActionTestBase
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
        public async Task LeaveMeeting_WhenHostLeaves_EndsMeeting()
        {
            Guid hostId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", deckId);
            this.InsertParticipant(meetingId, Guid.NewGuid(), hostId, "host", deckId);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            LeaveMeetingDto dto = new LeaveMeetingDto
            {
                MeetingId = meetingId,
                UserName = "host",
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<LeaveMeetingActionAsync>[] conditions = new IUnitTestCondition<LeaveMeetingActionAsync>[]
            {
                new MeetingStatusCondition<LeaveMeetingActionAsync>(ado, meetingId.ToString(), 0),
                new ParticipantExistsCondition<LeaveMeetingActionAsync>(ado, meetingId.ToString(), "host", false),
            };

            await this.TestActionAsync<LeaveMeetingActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Success(true),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task LeaveMeeting_WhenGuestLeaves_RemovesParticipantOnly()
        {
            Guid hostId = Guid.NewGuid();
            Guid guestId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(guestId, "guest", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", deckId);
            this.InsertParticipant(meetingId, Guid.NewGuid(), hostId, "host", deckId);
            this.InsertParticipant(meetingId, Guid.NewGuid(), guestId, "guest", deckId);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = guestId, UserName = "guest" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            LeaveMeetingDto dto = new LeaveMeetingDto
            {
                MeetingId = meetingId,
                UserName = "guest",
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<LeaveMeetingActionAsync>[] conditions = new IUnitTestCondition<LeaveMeetingActionAsync>[]
            {
                new MeetingStatusCondition<LeaveMeetingActionAsync>(ado, meetingId.ToString(), 1),
                new ParticipantExistsCondition<LeaveMeetingActionAsync>(ado, meetingId.ToString(), "guest", false),
                new ParticipantExistsCondition<LeaveMeetingActionAsync>(ado, meetingId.ToString(), "host", true),
            };

            await this.TestActionAsync<LeaveMeetingActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Success(true),
                this._mocks,
                conditions: conditions);
        }
    }
}
