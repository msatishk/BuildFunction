using Shared.Utils;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using ToIVUMultipleFromOracle.Interfaces;
using ToIVUMultipleFromOracle.Models;

namespace ToIVUMultipleFromOracle.Services
{
    public class WCFClient<T> : IWCFClient<T> where T : class, ICommunicationObject
    {
        private T client;
        private bool disposed;

        public WCFClient(IVUPayloadSettings iVUPayloadSettings)
        {
            if (string.IsNullOrWhiteSpace(iVUPayloadSettings.IVUEndpoint))
            {
                throw new ArgumentException("Endpoint cannot be empty.");
            }
            client = CreateChannel(iVUPayloadSettings);
        }

        ~WCFClient() { Dispose(false); }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                var closedSuccessfully = false;

                try
                {
                    if (client.State != CommunicationState.Faulted)
                    {
                        client.Close();
                        closedSuccessfully = true;
                    }
                }
                finally
                {
                    if (!closedSuccessfully)
                    {
                        client.Abort();
                    }
                }

                client = null;
                disposed = true;
            }
        }

        private static T CreateChannel(IVUPayloadSettings iVUPayloadSettings)
        {
            var binding = new BasicHttpsBinding();
            binding.MaxBufferPoolSize = int.MaxValue;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            var factory = new ChannelFactory<T>(binding, new EndpointAddress(iVUPayloadSettings.IVUEndpoint));
            factory.Credentials.UserName.UserName = iVUPayloadSettings.Username;
            factory.Credentials.UserName.Password = iVUPayloadSettings.Password;    
            return factory.CreateChannel();
        }

        public void Execute(Action<T> function) { function(client); }

        public TResult Execute<TResult>(Func<T, TResult> function) { return function(client); }
    }
}
