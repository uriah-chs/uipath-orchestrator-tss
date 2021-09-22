using System.Configuration;
using System.Threading.Tasks;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Clients;
using Thycotic.SecretServer.Sdk.Extensions.Integration.Models;
using Thycotic.SecretServer.Sdk.Infrastructure.Models;
using System;
using System.IO;
using System.Diagnostics;

namespace SecretServer.SecureStore
{
    public interface ISecretServerClientFactory
    {
        ISecretServerClient GetClient(SecretServerContext context);
    }

    public class SecretServerClientFactory : ISecretServerClientFactory
    {
        private ISecretServerClient _injectedSecretServerClient;

        public SecretServerClientFactory() : this(new SecretServerClient())
        {
        }

        public SecretServerClientFactory(ISecretServerClient secretServerClient)
        {
            _injectedSecretServerClient = secretServerClient;
        }

        public ISecretServerClient GetClient(SecretServerContext context)
        {
            EventLog.WriteEntry(".NET Runtime", $"Configuring SecretServerSdkConfigDirectory to {context.SdkConfigDirectory}", EventLogEntryType.Information, 1000);
            ConfigurationManager.AppSettings["SecretServerSdkConfigDirectory"] = context.SdkConfigDirectory;

            var redoTimes = 3;
            ISecretServerClient secretServerClient = null;
            while (redoTimes > 0 )
            {
                try
                {
                    secretServerClient = _injectedSecretServerClient ?? new SecretServerClient(); //required because the configure call creates state within the client that cannot be added again.
                    secretServerClient.Configure(new ConfigSettings
                    {
                        SecretServerUrl = context.SecretServerUrl,
                        RuleName = context.RuleName,
                        RuleKey = context.RuleKey,
                        CacheStrategy = CacheStrategy.Never,
                        ResetToken = context.ResetToken,
                    });
                    redoTimes = 0;
                }
                catch (Exception ex) when (ex is IOException || ex is ArgumentException)
                {
                    EventLog.WriteEntry(".NET Runtime", ex.ToString(), EventLogEntryType.Error, 1000);
                    Task.Delay(300).Wait();
                    redoTimes--;
                }
                catch
                {
                    throw;
                }
            }
            
            return secretServerClient;
        }
    }
}
