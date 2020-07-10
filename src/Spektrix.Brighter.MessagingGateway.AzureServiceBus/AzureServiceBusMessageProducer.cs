using System;
using Paramore.Brighter;
using Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusMessageProducer : IAmAMessageProducer
    {
        private readonly IManagementClientWrapper _managementClientWrapper;
        private readonly ITopicClientProvider _topicClientProvider;
       
        public AzureServiceBusMessageProducer(IManagementClientWrapper managementClientWrapper, ITopicClientProvider topicClientProvider)
        {
            _managementClientWrapper = managementClientWrapper;
            _topicClientProvider = topicClientProvider;
        }

        public void Send(Message message)
        {
            SendWithDelay(message);
        }
        
        public void SendWithDelay(Message message, int delayMilliseconds = 0)
        {
            EnsureTopicExists(message.Header.Topic);

            var topicClient = _topicClientProvider.Get(message.Header.Topic);
            try
            {
                if (delayMilliseconds == 0)
                {
                    topicClient.Send(new Microsoft.Azure.ServiceBus.Message(message.Body.Bytes));
                }
                else
                {
                    var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow.AddMilliseconds(delayMilliseconds));
                    topicClient.ScheduleMessage(new Microsoft.Azure.ServiceBus.Message(message.Body.Bytes), dateTimeOffset);
                }
            }
            finally
            {
                topicClient.Close();
            }
        }

        private void EnsureTopicExists(string topic)
        {
            if (!_managementClientWrapper.TopicExists(topic))
            {
                _managementClientWrapper.CreateTopic(topic);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
