using System;
using NUnit.Framework;
using Paramore.Brighter;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.Tests
{
    [TestFixture]
    public class ThreadSafeInMemoryMessageStoreTests
    {
        [Test]
        public void When_adding_a_new_message_it_should_add_it()
        {
            var store = new ConcurrentInMemoryMessageStore();
            var message = new Message();
            store.Add(message);
            var storedMessage = store.Get(message.Id);
            Assert.That(storedMessage, Is.EqualTo(message));
        }

        [Test]
        public void When_adding_a_new_message_that_already_exist_it_should_not_override_it()
        {
            var store = new ConcurrentInMemoryMessageStore();
            var message = new Message();
            store.Add(message);
            store.Add(message);
            var storedMessage = store.Get(message.Id);
            Assert.That(storedMessage, Is.EqualTo(message));
        }

        [Test]
        public void When_getting_a_message_that_does_not_exist_it_should_return_null()
        {
            var store = new ConcurrentInMemoryMessageStore();
            var storedMessage = store.Get(Guid.NewGuid());
            Assert.That(storedMessage, Is.Null);
        }

        [Test]
        public void When_getting_a_message_it_should_then_remove_it()
        {
            var store = new ConcurrentInMemoryMessageStore();
            var message = new Message();
            store.Add(message);
            var storedMessage = store.Get(message.Id);
            Assert.That(storedMessage, Is.Not.Null);
            storedMessage = store.Get(message.Id);
            Assert.That(storedMessage, Is.Null);
        }
    }
}