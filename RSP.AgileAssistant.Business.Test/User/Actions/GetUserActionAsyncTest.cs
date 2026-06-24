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
using RSP.AgileAssistant.Business.User.Actions;
using RSP.AgileAssistant.Business.User.Dto;
using RSP.AgileAssistant.Business.User.Vo;
using RSP.Common.DataAccess;

namespace RSP.AgileAssistant.Business.Test.User.Actions
{
    /// <summary>
    /// Unit tests for <see cref="GetUserActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class GetUserActionAsyncTest : ActionTestBase
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
        public async Task GetUser_WhenUserExists_ReturnsUser()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "alice", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "alice" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            GetUserDto dto = new GetUserDto { UserId = userId, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<GetUserActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks);
        }

        [Test]
        public async Task GetUser_WhenUserMissing_Fails()
        {
            Guid userId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "ghost" });

            IADOConfigurable ado = this.GetADOConfiguration(false);
            GetUserDto dto = new GetUserDto { UserId = userId, AccessToken = AccessToken };
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<GetUserActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }
    }
}
