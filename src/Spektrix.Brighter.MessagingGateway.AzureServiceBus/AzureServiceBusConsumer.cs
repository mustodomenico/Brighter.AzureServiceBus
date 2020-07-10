using System;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Paramore.Brighter;
using Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers;
using Message = Paramore.Brighter.Message;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusConsumer : IAmAMessageConsumer
    {
        private readonly string _topicName;
        private readonly IAmAMessageProducer _messageProducer;
        private readonly IManagementClientWrapper _managementClientWrapper;
        private readonly int _batchSize;
        private readonly IMessageReceiverWrapper _messageReceiver;
        private readonly string _subscriptionName;
        private const string LockToken = "LockToken";

        public AzureServiceBusConsumer(string topicName, string subscriptionName, IAmAMessageProducer messageProducer, IManagementClientWrapper managementClientWrapper, IMessageReceiverProvider messageReceiverProvider, int batchSize = 10)
        {
            _subscriptionName = subscriptionName;
            _topicName = topicName;
            _messageProducer = messageProducer;
            _managementClientWrapper = managementClientWrapper;
            _batchSize = batchSize;
            _messageReceiver = messageReceiverProvider.Get(_topicName, subscriptionName, ReceiveMode.PeekLock);
        }

        public Message[] Receive(int timeoutInMilliseconds)
        {
            EnsureSubscription();
            
            var messages = _messageReceiver.Receive(_batchSize, TimeSpan.FromMilliseconds(timeoutInMilliseconds)).Result;
            var messagesToReturn = new List<Message>();

            foreach (var azureServiceBusMessage in messages)
            {
                var messageBody = System.Text.Encoding.Default.GetString(azureServiceBusMessage.MessageBodyValue);

                var message = new Message(new MessageHeader(Guid.NewGuid(), _topicName, MessageType.MT_EVENT), new MessageBody(messageBody));

                message.Header.Bag.Add(LockToken, azureServiceBusMessage.LockToken);
                messagesToReturn.Add(message);
            }

            return messagesToReturn.ToArray();
        }

        private void EnsureSubscription()
        {
            if (_managementClientWrapper.SubscriptionExists(_topicName, _subscriptionName))
                return;
            try
            {
                _managementClientWrapper.CreateSubscription(_topicName, _subscriptionName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {

            }
        }

        public void Requeue(Message message, int delayMilliseconds)
        {
            if (delayMilliseconds > 0)
            {
                _messageProducer.SendWithDelay(message, delayMilliseconds);
            }
            else
            {
                _messageProducer.Send(message);
            }
        }

        public void Acknowledge(Message message)
        {
            _messageReceiver.Complete(message.Header.Bag[LockToken].ToString());
        }

        public void Reject(Message message, bool requeue)
        {
            _messageReceiver.Abandon(message.Header.Bag[LockToken].ToString());

            if (requeue)
            {
                Requeue(message, 0);
            }
        }

        public void Purge()
        {
            //Not Implemented
        }

        public void Dispose()
        {
            _messageReceiver.Close();
        }
    }
}
