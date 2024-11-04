using CommonServices.Domain.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace RQMessageSender
{
    class Program
    {       
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",   
                Password = "password"    
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
            var replyQueueName = channel.QueueDeclare().QueueName;

            var messageModel = new MessageModel
            {
                Content = "{\"ClientName\":\"Tester Testerski\",\"ClientTelephone\":\"0878878878\",\"NumberOfReservedTable\":1,\"DateOfReservation\":\"2021-11-17 20:20:20\"}",
                CorrelationId = Guid.NewGuid().ToString(), // Generate a unique correlation ID
                ReplyTo = replyQueueName
            };

            var messageJson = JsonConvert.SerializeObject(messageModel); 

            var body = Encoding.UTF8.GetBytes(messageJson);

            channel.BasicPublish(exchange: "logs",
                                 routingKey: string.Empty,
                                 basicProperties: null,
                                 body: body);

        }

    }
}