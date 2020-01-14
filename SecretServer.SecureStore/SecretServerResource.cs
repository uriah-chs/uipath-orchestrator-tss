using System.Globalization;
using System.Threading;

namespace SecretServer.SecureStore
{
    public static class SecretServerResource
    {
        public static string GetResource(string name)
        {
            var resource = Resource.ResourceManager.GetString(name, Thread.CurrentThread.CurrentUICulture);
            if (string.IsNullOrEmpty(resource))
            {
                resource = Resource.ResourceManager.GetString(name, CultureInfo.InvariantCulture);
            }

            return resource;
        }
    }
}
