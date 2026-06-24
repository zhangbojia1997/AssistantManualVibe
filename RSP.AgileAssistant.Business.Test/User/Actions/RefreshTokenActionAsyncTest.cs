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
using RSP.AgileAssistant.Business.User.Vo;
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Test.User.Actions
{
    /// <summary>
    /// Unit tests for <see cref="RefreshTokenActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class RefreshTokenActionAsyncTest : ActionTestBase
    {
        private const string OldToken = "old-access-token";

        private const string NewToken = "new-access-token";

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
        public async Task Refresh_WithCurrentToken_IssuesNewToken()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "alice", OldToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(OldToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "alice" });
            this._mockTokenService
                .Setup(service => service.CreateToken(userId, "alice"))
                .Returns(NewToken);

            IADOConfigurable ado = this.GetADOConfiguration(false);
            RefreshTokenDto dto = new RefreshTokenDto { AccessToken = OldToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<RefreshTokenActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks,
                new UserTokenByIdCondition<RefreshTokenActionAsync>(ado, userId, NewToken));
        }

        [Test]
        public async Task Refresh_WithInvalidToken_Fails()
        {
            this._mockTokenService
                .Setup(service => service.DecodeToken(It.IsAny<string>()))
                .Returns((TokenClaim?)null);

            IADOConfigurable ado = this.GetADOConfiguration(false);
            RefreshTokenDto dto = new RefreshTokenDto { AccessToken = "tampered" };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<RefreshTokenActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }

        [Test]
        public async Task Refresh_WithSupersededToken_Fails()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "alice", NewToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(OldToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "alice" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            RefreshTokenDto dto = new RefreshTokenDto { AccessToken = OldToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<RefreshTokenActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }
    }
}
