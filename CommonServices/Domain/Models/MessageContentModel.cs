namespace CommonServices.Domain.Models
{
    public class MessageContentModel
    {
        public string ClientName { get; set; }
        public string ClientTelephone { get; set; }
        public int NumberOfReservedTable { get; set; }
        public string DateOfReservation { get; set; }
        public int ValidationResult { get; set; }
        public string ResultText { get; set; }
    }
}
