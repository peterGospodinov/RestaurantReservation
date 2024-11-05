using CommonServices.Domain.Models;
using System.Text.Json;

namespace ValidationService
{
    public class Validator
    {
        public MessageContentModel ValidateContent(MessageModel input)
        {
            var messageContent = new MessageContentModel();
            if (string.IsNullOrEmpty(input.Content))
            {
                messageContent.ValidationResult = 0;
                messageContent.ResultText = "Content is empty or null.";
                return messageContent;
            }

            try
            {               
                messageContent = JsonSerializer.Deserialize<MessageContentModel>(input.Content);
                
                if (string.IsNullOrEmpty(messageContent.ClientName))
                {
                    messageContent.ValidationResult = 0;
                    messageContent.ResultText = "ClientName is required.";
                    return messageContent;
                }

                if (string.IsNullOrEmpty(messageContent.ClientTelephone))
                {
                    messageContent.ValidationResult = 0;
                    messageContent.ResultText = "Client Telephone is required.";
                    return messageContent;
                }

                if (string.IsNullOrEmpty(messageContent.NumberOfReservedTable))
                {
                    messageContent.ValidationResult = 0;
                    messageContent.ResultText = "Number Of ReservedTable is required.";
                    return messageContent;
                }

                if (string.IsNullOrEmpty(messageContent.DateOfReservation) || !DateTime.TryParse(messageContent.DateOfReservation, out _))
                {
                    messageContent.ValidationResult = 0;
                    messageContent.ResultText = "DateOfReservation is required and should be a valid date.";
                    return messageContent;
                }

                // If all validations pass
                messageContent.ValidationResult = 9;
                messageContent.ResultText = "Validation successful.";
            }
            catch (JsonException ex)
            {
                messageContent.ValidationResult = 0;
                messageContent.ResultText = $"Invalid JSON format: {ex.Message}";
            }
            catch (Exception ex)
            {
                messageContent.ValidationResult = 0;
                messageContent.ResultText = $"Unexpected error: {ex.Message}";
            }

            return messageContent;

        }
    }
}
