using Microsoft.Azure.ServiceBus.Management;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public class ManagementClientWrapper : IManagementClientWrapper
    {
        private readonly ManagementClient _managementClient;

        public ManagementClientWrapper(AzureServiceBusConfiguration configuration)
        {
            _managementClient = new ManagementClient(configuration.ConnectionString);
        }

        public bool TopicExists(string topic)
        {
            return _managementClient.TopicExistsAsync(topic).Result;
        }

        public void CreateTopic(string topic)
        {
            _managementClient.CreateTopicAsync(topic).Wait();
        }

        public bool SubscriptionExists(string topicName, string subscriptionName)
        {
            return _managementClient.SubscriptionExistsAsync(topicName, subscriptionName).Result;
        }

        public void CreateSubscription(string topicName, string subscriptionName)
        {
            var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName)
            {
                MaxDeliveryCount = 2000
            };
            _managementClient.CreateSubscriptionAsync(subscriptionDescription).Wait();
        }
    }
}
