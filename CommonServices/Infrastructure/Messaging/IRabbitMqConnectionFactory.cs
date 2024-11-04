using RabbitMQ.Client;

namespace CommonServices.Infrastructure.Messaging
{
    public interface IRabbitMqConnectionFactory
    {
        IConnection CreateConnection();
    }
}
