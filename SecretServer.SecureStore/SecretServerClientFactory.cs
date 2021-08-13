using System.Configuration;
using System.Threading.Tasks;
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
        private ISecretServerClient _secretServerClient;

        public SecretServerClientFactory(ISecretServerClient secretServerClient)
        {
            _secretServerClient = secretServerClient;
        }

        public ISecretServerClient GetClient(SecretServerContext context)
        {
            ConfigurationManager.AppSettings["SecretServerSdkConfigDirectory"] = context.SdkConfigDirectory;

            var redoTimes = 3;
            while (redoTimes > 0 )
            {
                try
                {
                    _secretServerClient.Configure(new ConfigSettings
                    {
                        SecretServerUrl = context.SecretServerUrl,
                        RuleName = context.RuleName,
                        RuleKey = context.RuleKey,
                        CacheStrategy = CacheStrategy.Never,
                        ResetToken = context.ResetToken,
                    });
                    redoTimes = 0;
                }
                catch (System.IO.IOException)
                {
                    Task.Delay(200).Wait();
                    redoTimes--;
                }
                catch
                {
                    throw;
                }
            }
            
            return _secretServerClient;
        }
    }
}
