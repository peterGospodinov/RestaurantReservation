using CommonServices.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServices.Infrastructure.Messaging
{
    public interface IMessageProcessor
    {
        void StartProducer(CancellationToken token, Action<MessageModel> messageHandler);
        void StartConsumers(CancellationToken token, Func<MessageModel, Task<MessageModel>> messageHandler);
        void SendMessage(MessageModel message);
        void StopProcessing();
    }
}
