using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public class BrokeredMessageWrapper : IBrokeredMessageWrapper
    {
        private readonly Message _brokeredMessage;

        public BrokeredMessageWrapper(Message brokeredMessage)
        {
            _brokeredMessage = brokeredMessage;
        }

        public byte[] MessageBodyValue => _brokeredMessage.Body;

        public string LockToken => _brokeredMessage.SystemProperties.LockToken;
    }
}