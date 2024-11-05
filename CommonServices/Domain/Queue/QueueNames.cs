namespace CommonServices.Domain.Queue
{
    public static class QueueNames
    {
        public static class Validation
        {
            public const string Send = "Validation.Send.Queue";
            public const string Receive = "Validation.Receive.Queue";
        }

        public static class Success
        {
            public const string Send = "Success.Send.Queue";
            public const string Receive = "Success.Receive.Queue";
        }

        public static class Fail
        {
            public const string Send = "Fail.Send.Queue";
            public const string Receive = "Fail.Receive.Queue";
        }
    }
}
