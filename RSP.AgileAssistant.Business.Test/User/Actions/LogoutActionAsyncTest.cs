using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Auth.Bo;
using RSP.AgileAssistant.Business.Base.Bo;
using RSP.AgileAssistant.Business.Consts;
using RSP.AgileAssistant.Business.Test.Base;
using RSP.AgileAssistant.Business.Test.UnitTestConditions;
using RSP.AgileAssistant.Business.User.Actions;
using RSP.AgileAssistant.Business.User.Dto;
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Test.User.Actions
{
    /// <summary>
    /// Unit tests for <see cref="LogoutActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class LogoutActionAsyncTest : ActionTestBase
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
        public async Task Logout_WithValidToken_ClearsRefreshToken()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "alice", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "alice" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            LogoutDto dto = new LogoutDto { UserId = userId, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<LogoutActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Success(true),
                this._mocks,
                new UserTokenByIdCondition<LogoutActionAsync>(ado, userId, string.Empty));
        }

        [Test]
        public async Task Logout_WithMismatchedToken_Fails()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "alice", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(It.IsAny<string>()))
                .Returns((TokenClaim?)null);

            IADOConfigurable ado = this.GetADOConfiguration(false);
            LogoutDto dto = new LogoutDto { UserId = userId, AccessToken = "wrong" };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<LogoutActionAsync, BusinessResult<bool>>(
                constructorArgs,
                BusinessResult<bool>.Fail(),
                this._mocks);
        }
    }
}
