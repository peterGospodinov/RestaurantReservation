using Microsoft.Win32;
using RabbitMQ.Client;

namespace CommonServices.Infrastructure.Messaging
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConnectionFactory(ConnectionFactory factory, string userName, string password)
        {
            _factory = factory;
            _factory.UserName = userName;
            _factory.Password = password;
        }

        public IConnection CreateConnection()
        {
            return _factory.CreateConnection();
        }
    }
}
