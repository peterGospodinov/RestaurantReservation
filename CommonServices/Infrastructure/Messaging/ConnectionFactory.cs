using Microsoft.Win32;
using RabbitMQ.Client;

namespace CommonServices.Infrastructure.Messaging
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly RabbitMQ.Client.ConnectionFactory _factory;

        public ConnectionFactory(RabbitMQ.Client.ConnectionFactory factory)
        {
            _factory = factory;
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }
    }
}
