using System;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public interface IBrokeredMessageWrapper
    {
        byte[] MessageBodyValue { get; }

        string LockToken { get; }   
    }
}