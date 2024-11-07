using RabbitMQ.Client;

namespace CommonServices.Infrastructure.Messaging
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection();
    }
}
