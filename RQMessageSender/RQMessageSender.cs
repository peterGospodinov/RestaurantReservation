using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using CommonServices.Domain.Queue;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace RQMessageSender
{
    class Program
    {        
        static void Main(string[] args)
        {
            List<MessageModel> jsonMessageContents = new List<MessageModel>();
            string SendingQueue = QueueNames.Validation.Receive;
    
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",   
                Password = "password"    
            };
        
            var hardcodedMessages = new List<string>
            {
                JsonConvert.SerializeObject(new { ClientName = "", ClientTelephone = "0878878871", NumberOfReservedTable = 1, DateOfReservation = "2021-11-17 20:20:20" }),
                JsonConvert.SerializeObject(new { ClientName = "Jane Johnson", ClientTelephone = "0878878872", NumberOfReservedTable = 2, DateOfReservation = "2021-11-18 19:00:00" }),
                JsonConvert.SerializeObject(new { ClientName = "Alex Williams", ClientTelephone = "0878878873", NumberOfReservedTable = 3, DateOfReservation = "2021-11-19 18:30:00" }),
                JsonConvert.SerializeObject(new { ClientName = "", ClientTelephone = "0878878874", NumberOfReservedTable = 1, DateOfReservation = "2021-11-20 21:00:00" }),
                JsonConvert.SerializeObject(new { ClientName = "", ClientTelephone = "0878878875", NumberOfReservedTable = 2, DateOfReservation = "2021-11-21 20:00:00" }),
                JsonConvert.SerializeObject(new { ClientName = "Katie Testerski", ClientTelephone = "0878878876", NumberOfReservedTable = 4, DateOfReservation = "2021-11-22 19:30:00" }),
                JsonConvert.SerializeObject(new { ClientName = "Tester Smith", ClientTelephone = "0878878877", NumberOfReservedTable = 1, DateOfReservation = "2021-11-23 18:00:00" }),
                JsonConvert.SerializeObject(new { ClientName = "John Johnson", ClientTelephone = "0878878878", NumberOfReservedTable = 3, DateOfReservation = "2021-11-24 20:45:00" }),
                JsonConvert.SerializeObject(new { ClientName = "Jane Williams", ClientTelephone = "0878878879", NumberOfReservedTable = 2, DateOfReservation = "2021-11-25 19:15:00" }),
                JsonConvert.SerializeObject(new { ClientName = "Alex Brown", ClientTelephone = "0878878880", NumberOfReservedTable = 4, DateOfReservation = "2021-11-26 21:30:00" })               
            };       
     
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string queueName = SendingQueue;

                EnsureQueueExistsIfNotExists(queueName);

                foreach (var contentJson in hardcodedMessages)
                {                   
                    //EnsureQueueExistsIfNotExists(SendingQueue);
                    string messageId = Guid.NewGuid().ToString();

                    var messageModel = new MessageModel
                    {
                        Content = contentJson,
                        CorrelationId = messageId,
                        ReplyToQueue = queueName
                    };

                    var messageJson = JsonConvert.SerializeObject(messageModel);
                    var body = Encoding.UTF8.GetBytes(messageJson);

                    try
                    {
                        // Ensure that you declare the exchange first
                        var exchangeName = queueName;
   
                        // Now you can publish to the exchange safely
                        channel.BasicPublish(exchange: exchangeName, routingKey: RabbitMqRoutingKeys.Validate.ToString(), basicProperties: null, body: body);
                    }
                    catch (RabbitMQ.Client.Exceptions.AlreadyClosedException ex)
                    {
                        Console.WriteLine($"RabbitMQ Channel is already closed: {ex.Message}");
                        // Handle reconnection logic here if necessary
                    }
                    catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
                    {
                        Console.WriteLine($"Operation was interrupted: {ex.Message}");
                        // Handle other possible exceptions related to RabbitMQ operations
                    }


                    Console.WriteLine($"Message with Id: {messageId} Sent: {messageJson}");
                    Thread.Sleep(2000);

                }
            }

        }

        private static void EnsureQueueExistsIfNotExists(string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",
                Password = "password"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Direct, durable: true); 

                channel.QueueDeclare(queue: queueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                channel.QueueBind(queue: queueName, exchange: queueName, routingKey: RabbitMqRoutingKeys.Validate.ToString());
                channel.QueueBind(queue: queueName, exchange: queueName, routingKey: RabbitMqRoutingKeys.Success.ToString());
                channel.QueueBind(queue: queueName, exchange: queueName, routingKey: RabbitMqRoutingKeys.Fail.ToString());           
            }
        }
    }
}