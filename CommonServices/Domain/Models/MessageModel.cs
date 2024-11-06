namespace CommonServices.Domain.Models
{
    public class MessageModel
    {
       
        public string CorrelationId { get; set; }
        public string RoutingKey { get; set; }
        public string ReplyToQueue { get; set; }
        public string ForwardToQueue { get; set; }
        public string Content { get; set; }
        public int ValidationResult { get; set; }
    }
}
