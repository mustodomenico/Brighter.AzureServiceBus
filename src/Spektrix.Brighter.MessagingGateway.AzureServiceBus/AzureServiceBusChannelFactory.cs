using System;
using Paramore.Brighter;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusChannelFactory : IAmAChannelFactory
    {
        private readonly AzureServiceBusConsumerFactory _azureServiceBusConsumerFactory;
        private const int MaxQueueLength = 10;

        public AzureServiceBusChannelFactory(AzureServiceBusConsumerFactory azureServiceBusConsumerFactory)
        {
            _azureServiceBusConsumerFactory = azureServiceBusConsumerFactory;
        }

        public IAmAChannel CreateChannel(Connection connection)
        {
            if (connection.TimeoutInMiliseconds < 400)
            {
                throw new ArgumentException("The minimum allowed timeout is 400 milliseconds");
            }
            return new Channel(connection.ChannelName, _azureServiceBusConsumerFactory.Create(connection), MaxQueueLength);
        }
    }
}