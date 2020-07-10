using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Moq;
using NUnit.Framework;
using Paramore.Brighter;
using Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers;
using Message = Paramore.Brighter.Message;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.Tests
{
    [TestFixture]
    public class AzureServiceBusConsumerTests
    {
        private Mock<IManagementClientWrapper> _nameSpaceManagerWrapper;
        private AzureServiceBusConsumer _azureServiceBusConsumer;
        private Mock<IMessageReceiverWrapper> _messageReceiver;
        private Mock<IAmAMessageProducer> _mockMessageProducer;

        [SetUp]
        public void SetUp()
        {
            _nameSpaceManagerWrapper = new Mock<IManagementClientWrapper>();
            _mockMessageProducer = new Mock<IAmAMessageProducer>();
            var mockMessageReceiver = new Mock<IMessageReceiverProvider>();

            _messageReceiver = new Mock<IMessageReceiverWrapper>();

            mockMessageReceiver.Setup(x => x.Get("topic", "subscription", ReceiveMode.PeekLock)).Returns(_messageReceiver.Object);

            _azureServiceBusConsumer = new AzureServiceBusConsumer("topic", "subscription", _mockMessageProducer.Object,
                _nameSpaceManagerWrapper.Object, mockMessageReceiver.Object);
        }

        [Test]
        public void When_a_subscription_exists_and_messages_are_in_the_queue_the_messages_are_returned()
        {
            _nameSpaceManagerWrapper.Setup(f => f.SubscriptionExists("topic", "subscription")).Returns(true);

            var messageLockTokenOne = Guid.NewGuid().ToString();
            var messageLockTokenTwo = Guid.NewGuid().ToString();


            var brokeredMessageList = new List<IBrokeredMessageWrapper>();
            var message1 = new Mock<IBrokeredMessageWrapper>();
            message1.Setup(m => m.LockToken).Returns(messageLockTokenOne);
            message1.Setup(m => m.MessageBodyValue).Returns(Encoding.UTF8.GetBytes("somebody"));
            var message2 = new Mock<IBrokeredMessageWrapper>();
            message2.Setup(m => m.LockToken).Returns(messageLockTokenTwo);
            message2.Setup(m => m.MessageBodyValue).Returns(Encoding.UTF8.GetBytes("somebody2"));
            brokeredMessageList.Add(message1.Object);
            brokeredMessageList.Add(message2.Object);

            _messageReceiver.Setup(x => x.Receive(10, TimeSpan.FromMilliseconds(400))).Returns(Task.FromResult<IEnumerable<IBrokeredMessageWrapper>>(brokeredMessageList));

            var result = _azureServiceBusConsumer.Receive(400);

            Assert.That(result[0].Body.Value, Is.EqualTo("somebody"));
            Assert.That(result[0].Header.Bag["LockToken"], Is.EqualTo(messageLockTokenOne));
            Assert.That(result[0].Header.Topic, Is.EqualTo("topic"));

            Assert.That(result[1].Body.Value, Is.EqualTo("somebody2"));
            Assert.That(result[1].Header.Bag["LockToken"], Is.EqualTo(messageLockTokenTwo));
            Assert.That(result[1].Header.Topic, Is.EqualTo("topic"));
        }

        [Test]
        public void When_a_subscription_does_not_exist_and_messages_are_in_the_queue_then_the_subscription_is_created_and_messages_are_returned()
        {
            _nameSpaceManagerWrapper.Setup(f => f.SubscriptionExists("topic", "subscription")).Returns(false);
            var brokeredMessageList = new List<IBrokeredMessageWrapper>();
            var message1 = new Mock<IBrokeredMessageWrapper>();
            message1.Setup(m => m.LockToken).Returns(Guid.NewGuid().ToString);
            message1.Setup(m => m.MessageBodyValue).Returns(Encoding.UTF8.GetBytes("somebody"));
            brokeredMessageList.Add(message1.Object);

            _messageReceiver.Setup(x => x.Receive(10, TimeSpan.FromMilliseconds(400))).Returns(Task.FromResult<IEnumerable<IBrokeredMessageWrapper>>(brokeredMessageList));

            var result = _azureServiceBusConsumer.Receive(400);

            _nameSpaceManagerWrapper.Verify(f => f.CreateSubscription("topic", "subscription"));
            Assert.That(result[0].Body.Value, Is.EqualTo("somebody"));
        }

        [Test]
        public void When_there_are_no_messages_then_it_returns_an_empty_array()
        {
            _nameSpaceManagerWrapper.Setup(f => f.SubscriptionExists("topic", "subscription")).Returns(true);
            var brokeredMessageList = new List<IBrokeredMessageWrapper>();

            _messageReceiver.Setup(x => x.Receive(10, TimeSpan.FromMilliseconds(400))).Returns(Task.FromResult<IEnumerable<IBrokeredMessageWrapper>>(brokeredMessageList));

            var result = _azureServiceBusConsumer.Receive(400);
            Assert.IsEmpty(result);
        }

        [Test]
        public void When_trying_to_create_a_subscription_which_was_already_created_by_another_thread_it_should_ignore_the_error()
        {
            _nameSpaceManagerWrapper.Setup(f => f.SubscriptionExists("topic", "subscription")).Returns(false);
            _nameSpaceManagerWrapper.Setup(f => f.CreateSubscription("topic", "subscription")).Throws(new MessagingEntityAlreadyExistsException("whatever"));

            var brokeredMessageList = new List<IBrokeredMessageWrapper>();
            var message1 = new Mock<IBrokeredMessageWrapper>();
            message1.Setup(m => m.LockToken).Returns(Guid.NewGuid().ToString());
            message1.Setup(m => m.MessageBodyValue).Returns(Encoding.UTF8.GetBytes("somebody"));
            brokeredMessageList.Add(message1.Object);

            _messageReceiver.Setup(x => x.Receive(10, TimeSpan.FromMilliseconds(400))).Returns(Task.FromResult<IEnumerable<IBrokeredMessageWrapper>>(brokeredMessageList));

            var result = _azureServiceBusConsumer.Receive(400);

            _nameSpaceManagerWrapper.Verify(f => f.CreateSubscription("topic", "subscription"));
            Assert.That(result[0].Body.Value, Is.EqualTo("somebody"));
        }
        
        [Test]
        public void When_acknowledge_is_called_the_complete_method_is_called_with_the_correct_lock_token()
        {
            var messageLockTokenOne = Guid.NewGuid().ToString();

            var messageHeader = new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_EVENT);

            var message = new Message(messageHeader, new MessageBody("body"));
            message.Header.Bag.Add("LockToken", messageLockTokenOne);

            _azureServiceBusConsumer.Acknowledge(message);

            _messageReceiver.Verify(x => x.Complete(messageLockTokenOne), Times.Once);
        }

        [Test]
        public void When_reject_is_called_the_abandon_method_is_called_with_the_correct_lock_token()
        {
            var messageLockTokenOne = Guid.NewGuid().ToString();

            var messageHeader = new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_EVENT);

            var message = new Message(messageHeader, new MessageBody("body"));
            message.Header.Bag.Add("LockToken", messageLockTokenOne);

            _azureServiceBusConsumer.Reject(message, false);

            _messageReceiver.Verify(x => x.Abandon(messageLockTokenOne), Times.Once);
        }

        [Test]
        public void When_reject_is_called_with_requeue_the_abandon_method_is_called_with_the_correct_lock_token_and_the_message_requeued()
        {
            var messageLockTokenOne = Guid.NewGuid().ToString();

            var messageHeader = new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_EVENT);

            var message = new Message(messageHeader, new MessageBody("body"));
            message.Header.Bag.Add("LockToken", messageLockTokenOne);

            _azureServiceBusConsumer.Reject(message, true);

            _messageReceiver.Verify(x => x.Abandon(messageLockTokenOne), Times.Once);
            _mockMessageProducer.Verify(x => x.Send(message), Times.Once);
        }

        [Test]
        public void When_dispose_is_called_the_close_method_is_called()
        {
            _azureServiceBusConsumer.Dispose();

            _messageReceiver.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void When_requeue_is_called_and_the_delay_is_zero_the_send_method_is_called()
        {
            var messageLockTokenOne = Guid.NewGuid();
            var messageHeader = new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_EVENT);
            var message = new Message(messageHeader, new MessageBody("body"));
            message.Header.Bag.Add("LockToken", messageLockTokenOne);

            _azureServiceBusConsumer.Requeue(message, 0);

            _mockMessageProducer.Verify(x => x.Send(message), Times.Once);
        }

        [Test]
        public void When_requeue_is_called_and_the_delay_is_more_than_zero_the_sendWithDelay_method_is_called()
        {
            var messageLockTokenOne = Guid.NewGuid();
            var messageHeader = new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_EVENT);
            var message = new Message(messageHeader, new MessageBody("body"));
            message.Header.Bag.Add("LockToken", messageLockTokenOne);

            _azureServiceBusConsumer.Requeue(message, 100);

            _mockMessageProducer.Verify(x => x.SendWithDelay(message, 100), Times.Once);
        }
    }
}
