namespace CommonServices.Domain.Queue
{
    public static class QueueNames
    {
        public static class Validation
        {
            public const string Receive = "Validation.Receive.Queue";
        }

        public static class Success
        {
            public const string Receive = "Success_RabbitMq";
        }

        public static class Fail
        {
            public const string Receive = "Fail_RabbitMq";
        }
    }
}
