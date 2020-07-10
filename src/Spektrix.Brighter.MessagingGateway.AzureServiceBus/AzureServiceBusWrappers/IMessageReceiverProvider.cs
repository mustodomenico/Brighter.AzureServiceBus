using Microsoft.Azure.ServiceBus;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public interface IMessageReceiverProvider
    {
        IMessageReceiverWrapper Get(string topicName, string subscriptionName, ReceiveMode receiveMode);
    }
}
