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
    /// Unit tests for <see cref="UpdateParticipantDeckActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class UpdateParticipantDeckActionAsyncTest : ActionTestBase
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
        public async Task UpdateParticipantDeck_WhenParticipantExists_ChangesDeck()
        {
            Guid hostId = Guid.NewGuid();
            Guid guestId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid oldDeck = Guid.NewGuid();
            Guid newDeck = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(guestId, "guest", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", oldDeck);
            this.InsertParticipant(meetingId, Guid.NewGuid(), guestId, "guest", oldDeck);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = guestId, UserName = "guest" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            UpdateParticipantDeckDto dto = new UpdateParticipantDeckDto
            {
                MeetingId = meetingId,
                UserName = "guest",
                DeckId = newDeck,
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<UpdateParticipantDeckActionAsync>[] conditions = new IUnitTestCondition<UpdateParticipantDeckActionAsync>[]
            {
                new ParticipantExistsCondition<UpdateParticipantDeckActionAsync>(
                    ado,
                    meetingId.ToString(),
                    "guest",
                    true,
                    newDeck.ToString()),
            };

            await this.TestActionAsync<UpdateParticipantDeckActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Success(true),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task UpdateParticipantDeck_WhenParticipantMissing_Fails()
        {
            Guid hostId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", Guid.NewGuid());

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            UpdateParticipantDeckDto dto = new UpdateParticipantDeckDto
            {
                MeetingId = meetingId,
                UserName = "ghost",
                DeckId = Guid.NewGuid(),
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<UpdateParticipantDeckActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Fail(),
                this._mocks);
        }
    }
}
