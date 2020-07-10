namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public interface ITopicClientProvider
    {
        ITopicClient Get(string topic);
    }
}