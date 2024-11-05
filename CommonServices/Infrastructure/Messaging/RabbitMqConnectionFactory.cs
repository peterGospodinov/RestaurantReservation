using Microsoft.Win32;
using RabbitMQ.Client;

namespace CommonServices.Infrastructure.Messaging
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConnectionFactory(ConnectionFactory factory)
        {
            _factory = factory;
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }
    }
}
