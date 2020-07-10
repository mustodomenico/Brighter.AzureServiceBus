using System;
using Microsoft.Azure.ServiceBus;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public class TopicClientWrapper : ITopicClient
    {
        private readonly TopicClient _topicClient;

        public TopicClientWrapper(TopicClient topicClient)
        {
            _topicClient = topicClient;
        }

        public void Send(Message message)
        {
            _topicClient.SendAsync(message).Wait();
        }

        public void ScheduleMessage(Message message, DateTimeOffset scheduleEnqueueTime)
        {
            _topicClient.ScheduleMessageAsync(message, scheduleEnqueueTime).Wait();
        }

        public void Close()
        {
            _topicClient.CloseAsync().Wait();
        }
    }
}
