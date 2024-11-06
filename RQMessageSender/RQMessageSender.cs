﻿using CommonServices.Domain.Models;
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
            string SendingQueue = QueueNames.Success.Receive;
    
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "user",   
                Password = "password"    
            };

            var hardcodedMessages = new List<string>
            {
                JsonConvert.SerializeObject(new { ClientName = "John Smith", ClientTelephone = "0878878871", NumberOfReservedTable = 1, DateOfReservation = "2021-11-17 20:20:20" })
                //JsonConvert.SerializeObject(new { ClientName = "Jane Johnson", ClientTelephone = "0878878872", NumberOfReservedTable = 2, DateOfReservation = "2021-11-18 19:00:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Alex Williams", ClientTelephone = "0878878873", NumberOfReservedTable = 3, DateOfReservation = "2021-11-19 18:30:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "", ClientTelephone = "0878878874", NumberOfReservedTable = 1, DateOfReservation = "2021-11-20 21:00:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "", ClientTelephone = "0878878875", NumberOfReservedTable = 2, DateOfReservation = "2021-11-21 20:00:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Katie Testerski", ClientTelephone = "0878878876", NumberOfReservedTable = 4, DateOfReservation = "2021-11-22 19:30:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Tester Smith", ClientTelephone = "0878878877", NumberOfReservedTable = 1, DateOfReservation = "2021-11-23 18:00:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "John Johnson", ClientTelephone = "0878878878", NumberOfReservedTable = 3, DateOfReservation = "2021-11-24 20:45:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Jane Williams", ClientTelephone = "0878878879", NumberOfReservedTable = 2, DateOfReservation = "2021-11-25 19:15:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Alex Brown", ClientTelephone = "0878878880", NumberOfReservedTable = 4, DateOfReservation = "2021-11-26 21:30:00" }),
                //JsonConvert.SerializeObject(new { ClientName = "Alex Brown", ClientTelephone = "0878878880", NumberOfReservedTable = 4, DateOfReservation = "2021-11-26 21:30:00" })
            };       
     
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string queueName = SendingQueue;           
                channel.ExchangeDeclare(exchange: "shared_exchange", type: ExchangeType.Direct);
                
                
                
                channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "Sucess");
                //channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "Sucess");
                //channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "Fail");

                foreach (var contentJson in hardcodedMessages)
                {
                    string messageId = Guid.NewGuid().ToString();

                    var messageModel = new MessageModel
                    {
                        Content = contentJson,
                        CorrelationId = messageId,
                        ReplyToQueue = queueName
                    };

                    var messageJson = JsonConvert.SerializeObject(messageModel);
                    var body = Encoding.UTF8.GetBytes(messageJson);

                    channel.BasicPublish(exchange: "logs",
                                         routingKey: "Sucess",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine($"Message with Id: {messageId} Sent: {messageJson}");
                  
                }
            }

        }

    }
}