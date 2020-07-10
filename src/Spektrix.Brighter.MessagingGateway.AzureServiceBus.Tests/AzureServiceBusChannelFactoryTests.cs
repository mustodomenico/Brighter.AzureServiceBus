using System;
using NUnit.Framework;
using Paramore.Brighter;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.Tests
{
    [TestFixture]
    public class AzureServiceBusChannelFactoryTests
    {
        [Test]
        public void When_the_timeout_is_below_400_ms_it_should_throw_an_exception()
        {
            var factory = new AzureServiceBusChannelFactory(new AzureServiceBusConsumerFactory(new AzureServiceBusConfiguration("someString")));

            var exception = Assert.Throws<ArgumentException>(()=> factory.CreateChannel(new Connection(typeof(object), new ConnectionName("name"), new ChannelName("name"),
                new RoutingKey("name"),
                1, 1, 399)));

            Assert.That(exception.Message, Is.EqualTo("The minimum allowed timeout is 400 milliseconds"));
        }
    }
}