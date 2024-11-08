﻿using CommonServices.Domain.Enums;
using CommonServices.Domain.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Messaging
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ConcurrentQueue<MessageModel> _messageQueue = new ConcurrentQueue<MessageModel>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10); // Controls the maximum number of concurrent tasks
        private readonly IConnectionFactory _connectionFactory;
        private readonly string _queueName;

        public MessageProcessor(IConnectionFactory connectionFactory,
            string queueName)
        {
            _connectionFactory = connectionFactory;
            _queueName = queueName;
            DeclareAndBindQueue(_queueName);
        }

        public void StartProducer(CancellationToken token, Action<MessageModel> messageHandler)
        {
            IConnection connection = null;
            IModel channel = null;

            try
            {
                connection = _connectionFactory.CreateConnection();
                channel = connection.CreateModel();

                var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageContentJson = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<MessageModel>(messageContentJson);

                        message.CorrelationId = string.IsNullOrEmpty(message.CorrelationId)
                            ? ea.BasicProperties.CorrelationId
                            : message.CorrelationId;        
                        message.RoutingKey = ea.RoutingKey;
                        message.ReplyToQueue = string.IsNullOrEmpty(message.ReplyToQueue)
                            ? _queueName
                            : message.ReplyToQueue;

                        messageHandler(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing received message: {ex.Message}");
                    }
                };

                channel.BasicConsume(queue: _queueName,
                                     autoAck: true,
                                     consumer: consumer);

                while (!token.IsCancellationRequested)
                {
                    
                }
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }        
        }

        public void StartConsumers(CancellationToken token, Func<MessageModel, Task<MessageModel>> messageHandler)
        {
            while (!token.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    // Start a new task for each message processing
                    Task.Run(async () =>
                    {
                        await _semaphoreSlim.WaitAsync(); // Limit concurrent tasks
                        try
                        {
                            var response = await messageHandler(message);
                            if (response != null)
                            {
                                SendMessage(response);
                            }
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }, token);
                }
            }
        }

        public void SendMessage(MessageModel message)
        {
            IConnection connection = null;
            IModel channel = null;
            string queueName = message.ForwardToQueue;

            DeclareAndBindQueue(queueName);

            try
            {
                connection = _connectionFactory.CreateConnection();
                channel = connection.CreateModel();
                
                // Serialize message to JSON string
                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = channel.CreateBasicProperties();
                properties.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
                properties.ReplyTo = queueName;
                
                // Publish the message to the queue
                channel.BasicPublish(exchange: queueName,
                                     routingKey: message.RoutingKey,
                                     basicProperties: properties,
                                     body: body);
            
                Console.WriteLine($"Message with ID: {properties.CorrelationId} was sent to the queue named '{queueName}'.");               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }
        }

        public void EnqueueMessage(MessageModel message)
        {
            _messageQueue.Enqueue(message);
        }


        public void StopProcessing()
        {
            _cancellationTokenSource.Cancel();
        }

        private void DeclareAndBindQueue(string queueName)
        {
            using (var connection = _connectionFactory.CreateConnection())
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
