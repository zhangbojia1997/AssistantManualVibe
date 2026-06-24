using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RSP.AgileAssistant.Business.Auth;
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
    /// Unit tests for <see cref="LoginActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class LoginActionAsyncTest : ActionTestBase
    {
        private const string IssuedToken = "issued-access-token";

        private Mock<ITokenService> _mockTokenService = null!;

        private IDictionary<string, object> _mocks = null!;

        [SetUp]
        public void InitializeMocks()
        {
            this._mockTokenService = new Mock<ITokenService>();
            this._mockTokenService
                .Setup(service => service.CreateToken(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(IssuedToken);

            this._mocks = new Dictionary<string, object>
            {
                { IocConsts.TOKEN_SERVICE, this._mockTokenService.Object },
            };
        }

        [Test]
        public async Task Login_NewNamedUser_RegistersAndIssuesToken()
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            LoginDto dto = new LoginDto
            {
                UserName = "alice",
                IsGuest = false,
                ScrumMaster = false,
            };

            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<LoginActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks,
                new UserExistsByNameCondition<LoginActionAsync>(ado, "alice", IssuedToken));
        }

        [Test]
        public async Task Login_Guest_CreatesGuestAccount()
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            LoginDto dto = new LoginDto
            {
                IsGuest = true,
            };

            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<LoginActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks,
                new GuestUserCreatedCondition<LoginActionAsync>(ado, IssuedToken));
        }

        [Test]
        public async Task Login_NonGuestWithoutUserName_Fails()
        {
            IADOConfigurable ado = this.GetADOConfiguration(false);
            LoginDto dto = new LoginDto
            {
                UserName = null,
                IsGuest = false,
            };

            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<LoginActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }
    }
}
