using System.Configuration;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Clients;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Models;
using Thycotic.SecretServer.Sdk.Infrastructure.Models;

namespace SecretServer.SecureStore
{
    public interface ISecretServerClientFactory
    {
        ISecretServerClient GetClient(SecretServerContext context);
    }

    public class SecretServerClientFactory : ISecretServerClientFactory
    {
        public ISecretServerClient GetClient(SecretServerContext context)
        {
            ConfigurationManager.AppSettings["SecretServerSdkConfigDirectory"] = context.SdkConfigDirectory;
            var client = new SecretServerClient(); 
            client.Configure(new ConfigSettings
            {
                SecretServerUrl = context.SecretServerUrl,
                RuleName = context.RuleName, 
                RuleKey = context.RuleKey,
                CacheStrategy = CacheStrategy.Never,
                ResetToken = context.ResetToken,
            });
            return client;
        }
    }
}
