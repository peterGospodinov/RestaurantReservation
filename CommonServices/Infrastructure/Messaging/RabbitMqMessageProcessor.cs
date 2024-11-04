using CommonServices.Domain.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
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

        public RabbitMqMessageProcessor(IRabbitMqConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void StartProducer(CancellationToken token, Action<MessageModel> messageHandler)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var messageContent = Encoding.UTF8.GetString(body);
                        var message = new MessageModel
                        {
                            Content = messageContent,
                            CorrelationId = ea.BasicProperties.CorrelationId,
                            ReplyTo = ea.BasicProperties.ReplyTo
                        };

                        // Delegate message handling to the provided handler
                        messageHandler(message);
                    };

                    channel.BasicConsume(queue: "your-queue",
                                         autoAck: false,
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

        public void StopProcessing()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
