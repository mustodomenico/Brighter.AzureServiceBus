namespace Spektrix.Brighter.MessagingGateway.AzureServiceBus
{
    public class AzureServiceBusConfiguration
    {
        public AzureServiceBusConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }
    }
}
