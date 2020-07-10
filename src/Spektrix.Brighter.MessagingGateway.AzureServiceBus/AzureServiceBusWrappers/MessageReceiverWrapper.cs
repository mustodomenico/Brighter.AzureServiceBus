using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Core;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public class MessageReceiverWrapper : IMessageReceiverWrapper
    {
        private readonly IMessageReceiver _messageReceiver;

        public MessageReceiverWrapper(IMessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        public async Task<IEnumerable<IBrokeredMessageWrapper>> Receive(int batchSize, TimeSpan serverWaitTime)
        {
            var messages = await _messageReceiver.ReceiveAsync(batchSize, serverWaitTime);

            if (messages == null)
            {
                return new List<IBrokeredMessageWrapper>();
            }
            return messages.Select(x => new BrokeredMessageWrapper(x));
        }

        public void Complete(string lockToken)
        {
            _messageReceiver.CompleteAsync(lockToken).Wait();
        }

        public void Abandon(string lockToken)
        {
            _messageReceiver.AbandonAsync(lockToken).Wait();
        }

        public void Close()
        {
            _messageReceiver.CloseAsync().Wait();
        }
    }
}