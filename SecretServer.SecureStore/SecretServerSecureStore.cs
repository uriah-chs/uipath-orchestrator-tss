using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Clients;
using UiPath.Orchestrator.Extensibility.Configuration;
using UiPath.Orchestrator.Extensibility.SecureStores;

namespace SecretServer.SecureStore
{
    public class SecretServerSecureStore : ISecureStore
    {
        private readonly ISecretServerClientFactory _clientFactory;

        public SecretServerSecureStore(ISecretServerClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public SecretServerSecureStore() : this(new SecretServerClientFactory(new SecretServerClient()))
        {
        }

        public void Initialize(Dictionary<string, string> hostSettings)
        {
        }

        public SecureStoreInfo GetStoreInfo()
        {
            return new SecureStoreInfo {Identifier = "SecretServer", IsReadOnly = true};
        }

        public Task ValidateContextAsync(string context)
        {
            var ctx = DeserializeContext(context);
            _clientFactory.GetClient(ctx);
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationEntry> GetConfiguration()
        {
            return new List<ConfigurationEntry>
            {
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "SecretServerUrl",
                    DisplayName = "Secret Server URL",
                    IsMandatory = true,
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "RuleName",
                    DisplayName = "Rule Name",
                    IsMandatory = true,
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "RuleKey",
                    DisplayName = "Rule Key",
                    IsMandatory = false,
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "ResetToken",
                    DisplayName = "Reset Key",
                    IsMandatory = false,
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "UsernameField",
                    DisplayName = "Username Field Slug",
                    DefaultValue = "username",
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "PasswordField",
                    DisplayName = "Password Field Slug",
                    DefaultValue = "password",
                },
                new ConfigurationValue(ConfigurationValueType.String)
                {
                    Key = "SdkConfigDirectory",
                    DisplayName = "SDK Config Storage Path",
                    IsMandatory = true,
                },
            };
        }

        public Task<string> GetValueAsync(string context, string key)
        {
            var ctx = DeserializeContext(context);
            var client = _clientFactory.GetClient(ctx);

            var result = int.TryParse(key, out var outKey);
            if (!result)
            {
                throw new SecureStoreException("invalid external name, secret id must be a number");
            }

            var val = client.GetSecretField(outKey, ctx.PasswordField);
            return Task.FromResult(val.ToString());
        }

        public Task<Credential> GetCredentialsAsync(string context, string key)
        {
            var ctx = DeserializeContext(context);
            var client = _clientFactory.GetClient(ctx);

            var result = int.TryParse(key, out var outKey);

            if (!result)
            {
                throw new SecureStoreException(SecureStoreException.Type.InvalidConfiguration, SecretServerResource.GetResource("InvalidSecretId"));
            }

            var secretResponse = client.GetSecret(outKey);
            var items = secretResponse.Items.ToDictionary(x => $"{x.Slug}", x => x.ItemValue);

            var cred = new Credential
            {
                Username = items[GetFieldSlug(ctx.UsernameField)],
                Password = items[GetFieldSlug(ctx.PasswordField)],
            };

            return Task.FromResult(cred);
        }

        public Task<string> CreateValueAsync(string context, string key, string value)
        {
            throw new SecureStoreException(SecureStoreException.Type.UnsupportedOperation,
                SecretServerResource.GetResource("ReadOnly"));
        }

        public Task<string> CreateCredentialsAsync(string context, string key, Credential value)
        {
            throw new SecureStoreException(SecureStoreException.Type.UnsupportedOperation,
                SecretServerResource.GetResource("ReadOnly"));
        }

        public Task<string> UpdateValueAsync(string context, string key, string oldAugumentedKey, string value)
        {
            throw new SecureStoreException(SecureStoreException.Type.UnsupportedOperation,
                SecretServerResource.GetResource("ReadOnly"));
        }

        public Task<string> UpdateCredentialsAsync(string context, string key, string oldAugumentedKey,
            Credential value)
        {
            throw new SecureStoreException(SecureStoreException.Type.UnsupportedOperation,
                SecretServerResource.GetResource("ReadOnly"));
        }

        public Task RemoveValueAsync(string context, string key)
        {
            throw new SecureStoreException(SecureStoreException.Type.UnsupportedOperation,
                SecretServerResource.GetResource("ReadOnly"));
        }

        private SecretServerContext DeserializeContext(string context)
        {
            var config = JsonConvert.DeserializeObject<SecretServerContext>(context);
            return config;
        }

        private string GetFieldSlug(string fieldName)
        {
            return fieldName.Trim().Replace(" ", "-");
        }
    }
}