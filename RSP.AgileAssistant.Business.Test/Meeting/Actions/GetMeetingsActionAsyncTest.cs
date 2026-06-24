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
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Test.Meeting.Actions
{
    /// <summary>
    /// Unit tests for <see cref="GetMeetingsActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class GetMeetingsActionAsyncTest : ActionTestBase
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
        public async Task GetMeetings_WhenRunningOnly_ReturnsSuccess()
        {
            Guid hostId = Guid.NewGuid();
            this.InsertUser(hostId, "host", AccessToken);
            this.InsertMeeting(Guid.NewGuid(), hostId, "Running", Guid.NewGuid(), status: 1);
            this.InsertMeeting(Guid.NewGuid(), hostId, "Ended", Guid.NewGuid(), status: 0);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = hostId, UserName = "host" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            GetMeetingsDto dto = new GetMeetingsDto { RunningOnly = true, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<GetMeetingsActionAsync, BusinessResult<List<MeetingVo>>>(
                constructorArgs,
                BusinessResult<List<MeetingVo>>.Success(new List<MeetingVo>()),
                this._mocks);
        }

        [Test]
        public async Task GetMeetings_WhenTokenInvalid_Fails()
        {
            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns((TokenClaim?)null);

            IADOConfigurable ado = this.GetADOConfiguration(false);
            GetMeetingsDto dto = new GetMeetingsDto { RunningOnly = true, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<GetMeetingsActionAsync, BusinessResult<List<MeetingVo>>>(
                constructorArgs,
                BusinessResult<List<MeetingVo>>.Fail(),
                this._mocks);
        }
    }
}
