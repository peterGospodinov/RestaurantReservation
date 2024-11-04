using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RQMessageReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a connection factory with hostname, username, and password
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",   
                Password = "password"   
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

            // declare a server-named queue
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                              exchange: "logs",
                              routingKey: string.Empty);

            Console.WriteLine(" [*] Waiting for logs.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] {message}");
            };
            channel.BasicConsume(queue: queueName,
                     autoAck: true,
                     consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}