using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public interface IMessageReceiverWrapper
    {
        Task<IEnumerable<IBrokeredMessageWrapper>> Receive(int batchSize, TimeSpan serverWaitTime);
        void Complete(string lockToken);
        void Abandon(string lockToken);
        void Close();
    }
}