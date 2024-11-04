using CommonServices.Domain.Models;
using CommonServices.Infrastructure.Messaging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Windows.Storage.FileProperties;

namespace TestProject
{
    public class RabbitMqMessageProcessorTests
    {
        private readonly Mock<IRabbitMqConnectionFactory> _mockConnectionFactory;
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IModel> _mockChannel;
        private readonly IBasicProperties _basicProperties;

        public RabbitMqMessageProcessorTests()
        {
            _mockConnectionFactory = new Mock<IRabbitMqConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();

            _mockConnectionFactory.Setup(factory => factory.CreateConnection())
                                  .Returns(_mockConnection.Object);

            _mockConnection.Setup(conn => conn.CreateModel())
                           .Returns(_mockChannel.Object);

            var realFactory = new ConnectionFactory
            {
                HostName = "localhost",    
                UserName = "user",    
                Password = "password"  
            };
            using (var realConnection = realFactory.CreateConnection())
            using (var realChannel = realConnection.CreateModel())
            {
                _basicProperties = realChannel.CreateBasicProperties();
            }

            _mockChannel.Setup(channel => channel.CreateBasicProperties())
                    .Returns(_basicProperties);

        }

        [Fact]
        public void StartProducer_Should_Call_MessageHandler_When_Message_Received()
        {
            // Arrange
            var processor = new RabbitMqMessageProcessor(_mockConnectionFactory.Object);

            bool messageHandlerCalled = false;

            void MessageHandler(MessageModel message)
            {
                messageHandlerCalled = true;
            }

            var consumer = new EventingBasicConsumer(_mockChannel.Object);

            _basicProperties.CorrelationId = "123";
            _basicProperties.ReplyTo = "reply-queue";

            consumer.Received += (model, ea) =>
            {
                MessageHandler(new MessageModel
                {
                    Content = Encoding.UTF8.GetString(ea.Body.ToArray()),
                    CorrelationId = ea.BasicProperties?.CorrelationId,
                    ReplyTo = ea.BasicProperties?.ReplyTo
                });
            };

            consumer.HandleBasicDeliver(
                consumerTag: "",
                deliveryTag: 1,
                redelivered: false,
                exchange: "",
                routingKey: "",
                properties: _basicProperties,
                body: Encoding.UTF8.GetBytes("Test Message"));

            // Act
            var cts = new CancellationTokenSource(500); // Timeout after 500ms
            Task.Run(() => processor.StartProducer(cts.Token, MessageHandler));

            // Assert
            Assert.True(messageHandlerCalled);
        }
    }
}
