using System;
using System.Text;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Moq;
using NUnit.Framework;
using Paramore.Brighter;
using Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.Tests
{
    [TestFixture]
    public class AzureServiceBusMessageProducerTests
    {
        private Mock<IManagementClientWrapper> _nameSpaceManagerWrapper;
        private Mock<ITopicClientProvider> _topicClientProvider;
        private Mock<ITopicClient> _topicClient;
        private AzureServiceBusMessageProducer _producer;

        [SetUp]
        public void SetUp()
        {
            _nameSpaceManagerWrapper = new Mock<IManagementClientWrapper>();
            _topicClientProvider = new Mock<ITopicClientProvider>();
            _topicClient = new Mock<ITopicClient>();

            _producer = new AzureServiceBusMessageProducer(_nameSpaceManagerWrapper.Object, _topicClientProvider.Object);
        }
        
        [Test]
        public void When_the_topic_exists_and_sending_a_message_with_no_delay_it_should_send_the_message_to_the_correct_topicclient()
        {
            Microsoft.Azure.ServiceBus.Message sentMessage = null;
            var messageBody = Encoding.UTF8.GetBytes("somebody");
            
            _nameSpaceManagerWrapper.Setup(t => t.TopicExists("topic")).Returns(true);
            _topicClientProvider.Setup(f => f.Get("topic")).Returns(_topicClient.Object);
            _topicClient.Setup(f => f.Send(It.IsAny<Microsoft.Azure.ServiceBus.Message>())).Callback((Microsoft.Azure.ServiceBus.Message g) => sentMessage = g);
            
            _producer.Send(new Message(new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_NONE), new MessageBody(messageBody, "JSON")));

            Assert.That(sentMessage.Body, Is.EqualTo(messageBody));
            _topicClient.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void When_the_topic_does_not_exist_it_should_be_created_and_the_message_is_sent_to_the_correct_topicclient()
        {
            Microsoft.Azure.ServiceBus.Message sentMessage = null;
            var messageBody = Encoding.UTF8.GetBytes("somebody");

            _nameSpaceManagerWrapper.Setup(t => t.TopicExists("topic")).Returns(false);
            _topicClientProvider.Setup(f => f.Get("topic")).Returns(_topicClient.Object);
            _topicClient.Setup(f => f.Send(It.IsAny<Microsoft.Azure.ServiceBus.Message>())).Callback((Microsoft.Azure.ServiceBus.Message g) => sentMessage = g);
            
            _producer.Send(new Message(new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_NONE), new MessageBody(messageBody, "JSON")));

            _nameSpaceManagerWrapper.Verify(x => x.CreateTopic("topic"),Times.Once);
            Assert.That(sentMessage.Body, Is.EqualTo(messageBody));
        }

        [Test]
        public void When_a_message_is_send_and_an_exception_occurs_close_is_still_called()
        {
            _nameSpaceManagerWrapper.Setup(t => t.TopicExists("topic")).Returns(true);
            _topicClientProvider.Setup(f => f.Get("topic")).Returns(_topicClient.Object);

            _topicClient.Setup(x => x.Send(It.IsAny<Microsoft.Azure.ServiceBus.Message>())).Throws(new Exception("Failed"));

            try
            {
                _producer.Send(new Message(new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_NONE), new MessageBody("Message", "JSON")));
            }
            catch (Exception)
            {
                // ignored
            }

            _topicClient.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void When_the_topic_exists_and_sending_a_message_with_a_delay_it_should_send_the_message_to_the_correct_topicclient()
        {
            Microsoft.Azure.ServiceBus.Message sentMessage = null;
            var messageBody = Encoding.UTF8.GetBytes("somebody");
            
            _nameSpaceManagerWrapper.Setup(t => t.TopicExists("topic")).Returns(true);
            _topicClientProvider.Setup(f => f.Get("topic")).Returns(_topicClient.Object);
            _topicClient.Setup(f => f.ScheduleMessage(It.IsAny<Microsoft.Azure.ServiceBus.Message>(), It.IsAny<DateTimeOffset>())).Callback((Microsoft.Azure.ServiceBus.Message g, DateTimeOffset d) => sentMessage = g);
            
            _producer.SendWithDelay(new Message(new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_NONE), new MessageBody(messageBody, "JSON")), 1);

            Assert.That(sentMessage.Body, Is.EqualTo(messageBody));
            _topicClient.Verify(x => x.Close(), Times.Once);
        }

        [Test]
        public void When_the_topic_does_not_exist_and_sending_a_message_with_a_delay_it_should_send_the_message_to_the_correct_topicclient()
        {
            Microsoft.Azure.ServiceBus.Message sentMessage = null;
            var messageBody = Encoding.UTF8.GetBytes("somebody");
            
            _nameSpaceManagerWrapper.Setup(t => t.TopicExists("topic")).Returns(false);
            _topicClientProvider.Setup(f => f.Get("topic")).Returns(_topicClient.Object);
            _topicClient.Setup(f => f.ScheduleMessage(It.IsAny<Microsoft.Azure.ServiceBus.Message>(), It.IsAny<DateTimeOffset>())).Callback((Microsoft.Azure.ServiceBus.Message g, DateTimeOffset d) => sentMessage = g);
            
            _producer.SendWithDelay(new Message(new MessageHeader(Guid.NewGuid(), "topic", MessageType.MT_NONE), new MessageBody(messageBody, "JSON")), 1);

            _nameSpaceManagerWrapper.Verify(x => x.CreateTopic("topic"),Times.Once);
            Assert.That(sentMessage.Body, Is.EqualTo(messageBody));
            _topicClient.Verify(x => x.Close(), Times.Once);
        }

    }
}
