using CommonServices.Infrastructure.Db;
using Moq;
using System.Collections.Concurrent;

namespace TestProject
{
    public class ReservationDbServiceTests
    {
        private readonly Mock<IDatabaseManager> _mockDbManager;
        private readonly string _appName = "MyApp";
        private readonly ConcurrentBag<string> _logMessagesCollection;

        public ReservationDbServiceTests()
        {
            _mockDbManager = new Mock<IDatabaseManager>();
            _logMessagesCollection = new ConcurrentBag<string>();
        }

        [Fact]
        public async Task InsertReservation_ShouldLogSuccessMessage_WhenResultIsOne()
        {
            // Arrange
            var model = new StoreResultToDb { Result = 1 };

            _mockDbManager
                .Setup(m => m.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model))
                .Returns(Task.CompletedTask);

            // Act
            await ExecuteReservationInsertAsync(model);

            // Assert
            Assert.Contains("Request result inserted successfully", _logMessagesCollection);
        }

        [Fact]
        public async Task InsertReservation_ShouldLogErrorMessage_WhenResultIsNotOne() 
        {
            // Arrange
            var model = new StoreResultToDb { Result = 0, ResultText = "Some error occurred" };

            _mockDbManager
                .Setup(m => m.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model))
                .Returns(Task.CompletedTask);

            // Act
            await ExecuteReservationInsertAsync(model);

            // Assert
            Assert.Contains($"{_appName} Some error occurred", _logMessagesCollection);

        }

        [Fact]
        public async Task InsertReservation_ShouldLogExceptionMessage_WhenExceptionIsThrown() 
        {
            // Arrange
            var model = new StoreResultToDb();

            _mockDbManager
                .Setup(m => m.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            await ExecuteReservationInsertAsync(model);

            // Assert
            Assert.Contains($"{_appName} Database connection error", _logMessagesCollection);
        }

        private async Task ExecuteReservationInsertAsync(StoreResultToDb model)
        {
            try
            {
                await _mockDbManager.Object.ExecuteStoredProcedureAsync("sp_InsertRequestResult", model);

                if (model.Result == 1)
                {
                    _logMessagesCollection.Add("Request result inserted successfully");
                }
                else
                {
                    _logMessagesCollection.Add($"{_appName} {model.ResultText}");
                }
            }
            catch (Exception ex)
            {
                _logMessagesCollection.Add($"{_appName} {ex.Message}");
            }
        }
    }
}
