using CommonServices.Domain.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Messaging
{
    public class RabbitMqMessageProcessor : IMessageProcessor
    {
        private readonly ConcurrentQueue<MessageModel> _messageQueue = new ConcurrentQueue<MessageModel>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10); // Controls the maximum number of concurrent tasks
        private readonly IRabbitMqConnectionFactory _connectionFactory;
        private readonly string _queueName;
        private readonly string _replyToQueue;

        public RabbitMqMessageProcessor(IRabbitMqConnectionFactory connectionFactory,
            string queueName, 
            string replyToQueue)
        {
            _connectionFactory = connectionFactory;
            _queueName = queueName;
            _replyToQueue = replyToQueue;
            //EnsureQueueExists(_queueName);
        }

        public void StartProducer(CancellationToken token, Action<MessageModel> messageHandler)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {                      
                        try
                        {
                            // Deserialize message content
                            var body = ea.Body.ToArray();
                            var messageContentJson = Encoding.UTF8.GetString(body);
                            var message = JsonConvert.DeserializeObject<MessageModel>(messageContentJson);
                          
                            message.CorrelationId = string.IsNullOrEmpty(message.CorrelationId)
                                ? ea.BasicProperties.CorrelationId
                                : message.CorrelationId;
                            message.ReplyTo = string.IsNullOrEmpty(message.ReplyTo)
                                ? _replyToQueue
                                : message.ReplyTo;

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
                }
            }

            while (!token.IsCancellationRequested)
            {
                // Keep running until cancelled
            }
        }

        public void StartConsumers(CancellationToken token, Func<MessageModel, Task> messageHandler)
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
                            await messageHandler(message);
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }, token);
                }
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

        private void EnsureQueueExists(string queueName)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {              
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            }
        }
    }
}
