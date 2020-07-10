using Paramore.Brighter;
using Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusConsumerFactory : IAmAMessageConsumerFactory
    {
        private readonly AzureServiceBusConfiguration _configuration;

        public AzureServiceBusConsumerFactory(AzureServiceBusConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IAmAMessageConsumer Create(Connection connection)
        {
            var nameSpaceManagerWrapper = new ManagementClientWrapper(_configuration);

            return new AzureServiceBusConsumer(connection.RoutingKey, connection.ChannelName, 
                new AzureServiceBusMessageProducer(nameSpaceManagerWrapper, 
                new TopicClientProvider(_configuration)),nameSpaceManagerWrapper, 
                new MessageReceiverProvider(_configuration));
        }
    }
}
