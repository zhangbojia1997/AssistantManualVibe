using System;
using NUnit.Framework;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Auth.Bo;

namespace RSP.AgileAssistant.Business.Test.Auth
{
    /// <summary>
    /// Deterministic unit tests for <see cref="TokenService"/> covering token
    /// round-tripping and validation failure modes.
    /// </summary>
    [TestFixture]
    internal class TokenServiceTest
    {
        private const string ValidSecret = "unit-test-signing-secret-value-0123456789";

        [Test]
        public void CreateToken_ThenDecode_RoundTripsClaims()
        {
            TokenService service = new TokenService(ValidSecret);
            Guid userId = Guid.NewGuid();
            const string userName = "alice";

            string token = service.CreateToken(userId, userName);
            TokenClaim? claim = service.DecodeToken(token);

            Assert.That(claim, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(claim!.UserId, Is.EqualTo(userId));
                Assert.That(claim.UserName, Is.EqualTo(userName));
            });
        }

        [Test]
        public void DecodeToken_WithMalformedToken_ReturnsNull()
        {
            TokenService service = new TokenService(ValidSecret);

            TokenClaim? claim = service.DecodeToken("not-a-valid-token");

            Assert.That(claim, Is.Null);
        }

        [Test]
        public void DecodeToken_WithDifferentSecret_ReturnsNull()
        {
            TokenService issuer = new TokenService(ValidSecret);
            TokenService verifier = new TokenService("a-completely-different-secret-0123456789");
            string token = issuer.CreateToken(Guid.NewGuid(), "bob");

            TokenClaim? claim = verifier.DecodeToken(token);

            Assert.That(claim, Is.Null);
        }

        [Test]
        public void DecodeToken_WithNullOrWhitespace_ReturnsNull()
        {
            TokenService service = new TokenService(ValidSecret);

            Assert.Multiple(() =>
            {
                Assert.That(service.DecodeToken(null!), Is.Null);
                Assert.That(service.DecodeToken(string.Empty), Is.Null);
                Assert.That(service.DecodeToken("   "), Is.Null);
            });
        }

        [Test]
        public void Constructor_WithShortSecret_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TokenService("too-short"));
        }

        [Test]
        public void Constructor_WithEmptySecret_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TokenService(string.Empty));
        }

        [Test]
        public void CreateToken_WithEmptyUserId_Throws()
        {
            TokenService service = new TokenService(ValidSecret);

            Assert.Throws<ArgumentException>(() => service.CreateToken(Guid.Empty, "alice"));
        }
    }
}
