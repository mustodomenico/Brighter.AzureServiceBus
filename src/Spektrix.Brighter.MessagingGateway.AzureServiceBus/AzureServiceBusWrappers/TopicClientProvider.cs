using Microsoft.Azure.ServiceBus;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public class TopicClientProvider : ITopicClientProvider
    {
        private readonly string _connectionString;

        public TopicClientProvider(AzureServiceBusConfiguration configuration)
        {
            _connectionString = configuration.ConnectionString;
        }

        public ITopicClient Get(string topic)
        {
            var topicClient = new TopicClient(_connectionString, topic);
            return new TopicClientWrapper(topicClient);
        }
    }
}
