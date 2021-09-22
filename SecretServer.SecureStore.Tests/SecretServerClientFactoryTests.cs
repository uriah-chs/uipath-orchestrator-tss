using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Clients;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Models;
using Thycotic.SecretServer.Sdk.Infrastructure.Models;

namespace SecretServer.SecureStore.Tests
{
    [TestClass]
    public class SecretServerClientFactoryTests
    {
        private SecretServerClientFactory _clientFactory;
        private ISecretServerClient _client;
        private SecretServerContext _context;

        private readonly string _secretServerUrl = "https://expected.Secret.Server.URL";
        private readonly string _ruleName = "expected Rule Name";
        private readonly string _ruleKey = "expected Rule Key";
        private const CacheStrategy _cacheStrategy = CacheStrategy.Never;
        private readonly string _resetToken = "expected Reset Token";

        [TestInitialize]
        public void TestInitialize()
        {
            _client = Substitute.For<ISecretServerClient>();
            _clientFactory = new SecretServerClientFactory(_client);

            _context = new SecretServerContext
            {
                SecretServerUrl = _secretServerUrl,
                RuleName = _ruleName,
                RuleKey = _ruleKey,
                ResetToken = _resetToken
            };
        }

        [TestMethod]
        public void TestClientCreation()
        {
            var actual = _clientFactory.GetClient(_context);

            Assert.IsTrue(actual is ISecretServerClient);
        }

        [TestMethod]
        public void TestClientGetsConfigured()
        {
            var actual = _clientFactory.GetClient(_context);

            _client.Received().Configure(Arg.Is<ConfigSettings>(x =>
                x.SecretServerUrl == _secretServerUrl
                && x.RuleName == _ruleName
                && x.RuleKey == _ruleKey
                && x.CacheStrategy == _cacheStrategy
                && x.ResetToken == _resetToken));
        }

        [TestMethod]
        public void TestSuccessCallsConfigureOnlyOnce()
        {
            var actual = _clientFactory.GetClient(_context);

            _client.Received(1).Configure(Arg.Any<ConfigSettings>());
        }


        [TestMethod]
        public void TestRetries()
        {
            _client
                .When(x => x.Configure(Arg.Any<ConfigSettings>()))
                .Do(x => { throw new System.IO.IOException(); });

            _clientFactory.GetClient(_context);

            _client.Received(3).Configure(Arg.Any<ConfigSettings>());
        }
    }
}