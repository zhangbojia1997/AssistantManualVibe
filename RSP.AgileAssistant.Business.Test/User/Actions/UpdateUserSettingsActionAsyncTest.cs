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
using RSP.Common.UnitTest;

namespace RSP.AgileAssistant.Business.Test.User.Actions
{
    /// <summary>
    /// Unit tests for <see cref="UpdateUserSettingsActionAsync"/>.
    /// </summary>
    [TestFixture]
    internal class UpdateUserSettingsActionAsyncTest : ActionTestBase
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
        public async Task UpdateSettings_WhenTokenMatchesUser_PersistsSettings()
        {
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();
            this.InsertUser(userId, "alice", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "alice" });

            UpdateUserSettingsDto dto = new UpdateUserSettingsDto
            {
                UserId = userId,
                FullName = "Alice Smith",
                Group = "Team A",
                JiraEmail = "alice@example.com",
                JiraToken = "jira-token",
                DeckId = deckId,
                UseDefaults = true,
                AccessToken = AccessToken,
            };

            IADOConfigurable ado = this.GetADOConfiguration(false);
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<UpdateUserSettingsActionAsync>[] conditions = new IUnitTestCondition<UpdateUserSettingsActionAsync>[]
            {
                new UserSettingsByIdCondition<UpdateUserSettingsActionAsync>(
                    ado,
                    userId.ToString(),
                    "Alice Smith",
                    deckId.ToString(),
                    "Team A",
                    "alice@example.com",
                    "jira-token",
                    true),
            };

            await this.TestActionAsync<UpdateUserSettingsActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task UpdateSettings_WhenJiraCredentialsPartial_KeepsExistingAndUpdatesOthers()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "bob", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "bob" });

            UpdateUserSettingsDto dto = new UpdateUserSettingsDto
            {
                UserId = userId,
                FullName = "Bob Jones",
                Group = "Team B",
                JiraEmail = "bob@example.com",
                JiraToken = null,
                DeckId = null,
                UseDefaults = false,
                AccessToken = AccessToken,
            };

            IADOConfigurable ado = this.GetADOConfiguration(false);
            List<object> constructorArgs = new List<object> { ado, dto };

            IUnitTestCondition<UpdateUserSettingsActionAsync>[] conditions = new IUnitTestCondition<UpdateUserSettingsActionAsync>[]
            {
                new UserSettingsByIdCondition<UpdateUserSettingsActionAsync>(
                    ado,
                    userId.ToString(),
                    "Bob Jones",
                    null,
                    "Team B",
                    null,
                    null,
                    false),
            };

            await this.TestActionAsync<UpdateUserSettingsActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Success(new UserVo()),
                this._mocks,
                conditions: conditions);
        }

        [Test]
        public async Task UpdateSettings_WhenTokenInvalid_Fails()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "carol", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns((TokenClaim?)null);

            UpdateUserSettingsDto dto = new UpdateUserSettingsDto
            {
                UserId = userId,
                FullName = "Carol",
                AccessToken = AccessToken,
            };

            IADOConfigurable ado = this.GetADOConfiguration(false);
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<UpdateUserSettingsActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }

        [Test]
        public async Task UpdateSettings_WhenTokenBelongsToAnotherUser_Fails()
        {
            Guid userId = Guid.NewGuid();
            this.InsertUser(userId, "dave", AccessToken);

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = Guid.NewGuid(), UserName = "intruder" });

            UpdateUserSettingsDto dto = new UpdateUserSettingsDto
            {
                UserId = userId,
                FullName = "Dave",
                AccessToken = AccessToken,
            };

            IADOConfigurable ado = this.GetADOConfiguration(false);
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<UpdateUserSettingsActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }

        [Test]
        public async Task UpdateSettings_WhenUserMissing_Fails()
        {
            Guid userId = Guid.NewGuid();

            this._mockTokenService
                .Setup(service => service.DecodeToken(AccessToken))
                .Returns(new TokenClaim { UserId = userId, UserName = "ghost" });

            UpdateUserSettingsDto dto = new UpdateUserSettingsDto
            {
                UserId = userId,
                FullName = "Ghost",
                AccessToken = AccessToken,
            };

            IADOConfigurable ado = this.GetADOConfiguration(false);
            List<object> constructorArgs = new List<object> { ado, dto };

            await this.TestActionAsync<UpdateUserSettingsActionAsync, BusinessResult<UserVo>>(
                constructorArgs,
                BusinessResult<UserVo>.Fail(),
                this._mocks);
        }
    }
}
