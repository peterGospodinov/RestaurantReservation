using CommonServices.Domain.Enums;
using CommonServices.Domain.Queue;
using CommonServices.Infrastructure.Logging;
using CommonServices.Infrastructure.Messaging;
using FailService;
using RabbitMQ.Client;

class Program
{
    static void Main(string[] args)
    {
        const string AppName = "FailService";
        const string SqlConnectionString = "Host=localhost;Port=5432;Database=Reservations;Username=postgres;Password=YourStrong@Passw0rd;Trust Server Certificate=true;";
        const string Qname = QueueNames.Fail.Receive;

        var connectionFactory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = "localhost",
            UserName = "user",
            Password = "password"
        };

        CommonServices.Infrastructure.Messaging.IConnectionFactory rabbitMqConnectionFactory = new CommonServices.Infrastructure.Messaging.ConnectionFactory(connectionFactory);

        IMessageProcessor messageProcessor = new MessageProcessor(rabbitMqConnectionFactory, Qname);

        IFileLogger fileLogger = new FileLogger();

        var messageHandler = new FailCustomMessageHandler(fileLogger, 
                                                            AppName, 
                                                            SqlConnectionString, 
                                                            DatabaseType.PostgreSql);

        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Cancellation requested...");
            cancellationTokenSource.Cancel();
            e.Cancel = true;
        };


        // Start producer with a handler that enqueues messages
        Task producerTask = Task.Run(() => messageProcessor.StartProducer(CancellationToken.None, ((MessageProcessor)messageProcessor).EnqueueMessage));


        // Start consumers with the dynamic message handling logic
        Task consumerTask = Task.Run(() => messageProcessor.StartConsumers(CancellationToken.None, messageHandler.HandleMessageAsync));

        try
        {
            Console.WriteLine(AppName + "Service is running. Press Ctrl+C to stop.");
            Task.WaitAll(producerTask, consumerTask);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Service is stopping gracefully...");
        }
        finally
        {
            // Perform cleanup if necessary
            messageProcessor.StopProcessing();
            Console.WriteLine("Service has stopped.");
        }
    }
}