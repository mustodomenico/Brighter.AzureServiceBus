using System;
using Microsoft.Azure.ServiceBus;

namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus.AzureServiceBusWrappers
{
    public interface ITopicClient
    {
        void Send(Message message);
        void ScheduleMessage(Message message, DateTimeOffset scheduleEnqueueTime);
        void Close();
    }
}