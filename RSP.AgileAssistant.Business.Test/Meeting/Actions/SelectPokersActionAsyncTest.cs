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
    /// Unit tests for <see cref="SelectPokersActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class SelectPokersActionAsyncTest : ActionTestBase
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
        public async Task SelectPokers_WhenCardChosen_MarksParticipantPicked()
        {
            Guid hostId = Guid.NewGuid();
            Guid guestId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(guestId, "guest", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", deckId);
            this.InsertParticipant(meetingId, Guid.NewGuid(), guestId, "guest", deckId);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = guestId, UserName = "guest" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            SelectPokersDto dto = new SelectPokersDto
            {
                MeetingId = meetingId,
                UserName = "guest",
                SelectedPoker = new ParticipantPokerDto
                {
                    PokerId = Guid.NewGuid(),
                    OriginalPokerValue = "5",
                    PokerValue = "5",
                },
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<SelectPokersActionAsync>[] conditions = new IUnitTestCondition<SelectPokersActionAsync>[]
            {
                new ParticipantPokerCondition<SelectPokersActionAsync>(ado, meetingId.ToString(), "guest", true),
            };

            await this.TestActionAsync<SelectPokersActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task SelectPokers_WhenCleared_RemovesSelection()
        {
            Guid hostId = Guid.NewGuid();
            Guid guestId = Guid.NewGuid();
            Guid meetingId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertUser(guestId, "guest", AccessToken);
            this.InsertMeeting(meetingId, hostId, "Sprint planning", deckId);
            this.InsertParticipant(meetingId, Guid.NewGuid(), guestId, "guest", deckId);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = guestId, UserName = "guest" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            SelectPokersDto dto = new SelectPokersDto
            {
                MeetingId = meetingId,
                UserName = "guest",
                SelectedPoker = null,
                AccessToken = AccessToken,
            };
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<SelectPokersActionAsync>[] conditions = new IUnitTestCondition<SelectPokersActionAsync>[]
            {
                new ParticipantPokerCondition<SelectPokersActionAsync>(ado, meetingId.ToString(), "guest", false),
            };

            await this.TestActionAsync<SelectPokersActionAsync, BusinessResult<MeetingVo>>(
                constructorArgs,
                BusinessResult<MeetingVo>.Success(new MeetingVo()),
                this._mocks,
                conditions: conditions);
        }
    }
}
