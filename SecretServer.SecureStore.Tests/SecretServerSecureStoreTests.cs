using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using SecretServer.SecureStore;
using Thycotic.SecretServer.Sdk.Areas.Secrets.Models;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Clients;
using UiPath.Orchestrator.Extensibility.SecureStores;

namespace SecretServer.SecureStore.Tests
{
    [TestClass]
    public class SecretServerSecureStoreTests
    {
        private ISecretServerClient _client;
        private SecretServerContext _context;
        private ISecretServerClientFactory _clientFactory;
        private ISecureStore _secretSecureStore;
        private string _serializedContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = Substitute.For<ISecretServerClient>();
            _clientFactory = Substitute.For<ISecretServerClientFactory>();
            _context = new SecretServerContext
            {
                SecretServerUrl = "https://testsecretserver.fakedomain",
                RuleName = "uipath-rule",
                RuleKey = "abc-123",
                ResetToken = "reset",
                UsernameField = "username",
                PasswordField = "password",
            };

            _secretSecureStore = new SecretServerSecureStore(_clientFactory);
            _serializedContext = JsonConvert.SerializeObject(_context);
        }

        [TestMethod]
        public void TestGetValue()
        {
            var secretId = new Random().Next();
            var value = Guid.NewGuid().ToString();

            _clientFactory.GetClient(_context).ReturnsForAnyArgs(_client);
            _client.GetSecretField(secretId, "password").Returns(value);

            var password = _secretSecureStore.GetValueAsync(_serializedContext, secretId.ToString());
            Assert.AreEqual(value, password?.Result);
        }

        [TestMethod]
        public void TestGetCredential()
        {
            var secretId = new Random().Next();
            var username = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var value = new SecretModel
            {
                Items = new List<RestSecretItem>()
                {
                    new RestSecretItem {Slug = "username", ItemValue = username},
                    new RestSecretItem {Slug = "password", ItemValue = password}
                }
            };

            _clientFactory.GetClient(_context).ReturnsForAnyArgs(_client);
            _client.GetSecret(secretId).Returns(value);

            var secret = _secretSecureStore.GetCredentialsAsync(_serializedContext, secretId.ToString());
            Assert.AreEqual(username, secret?.Result.Username);
            Assert.AreEqual(password, secret?.Result.Password);
        }
    }
}