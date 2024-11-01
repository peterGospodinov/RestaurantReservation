using CommonServices.Infrastructure.Logging;

namespace TestProject
{
    public class FileLoggerTests : IDisposable
    {
        private readonly string _testLogFilePath;
        private readonly IFileLogger _fileLogger;

        public FileLoggerTests()
        {
            _testLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "test_application.log");
            _fileLogger = new FileLogger(_testLogFilePath);
        }

        public void Dispose()
        {
            // Clean up test log file after each test
            if (File.Exists(_testLogFilePath))
            {
                File.Delete(_testLogFilePath);
            }
        }

        [Fact]
        public async Task LogFile_IsCreated_WhenLogIsCalled()
        {
            // Act
            await _fileLogger.LogAsync(new List<string> { "Test log entry" });

            // Assert
            Assert.True(File.Exists(_testLogFilePath), "Log file was not created.");
        }

        [Fact]
        public async Task LogEntry_IsWritten_ToLogFile()
        {
            var message = "Test log entry";
            await _fileLogger.LogAsync(new List<string> { message });

            var logContents = await File.ReadAllLinesAsync(_testLogFilePath);
            Assert.Contains(logContents, line => line.Contains(message));
        }

        [Fact]
        public async Task LogEntries_AreWritten_InFIFOOrder()
        {
            var messages = new List<string> { "First entry", "Second entry", "Third entry" };
            await _fileLogger.LogAsync(messages);

            var logContents = await File.ReadAllLinesAsync(_testLogFilePath);
            for (int i = 0; i < messages.Count; i++)
            {
                Assert.Contains(messages[i], logContents[i]);
            }
        }

        [Fact]
        public async Task MultipleLogCalls_HandleMessages_Sequentially()
        {
            var firstBatch = new List<string> { "First batch - Entry 1", "First batch - Entry 2" };
            var secondBatch = new List<string> { "Second batch - Entry 1", "Second batch - Entry 2" };

            await _fileLogger.LogAsync(firstBatch);
            await _fileLogger.LogAsync(secondBatch);

            var logContents = await File.ReadAllLinesAsync(_testLogFilePath);
            Assert.Contains("First batch - Entry 1", logContents[0]);
            Assert.Contains("First batch - Entry 2", logContents[1]);
            Assert.Contains("Second batch - Entry 1", logContents[2]);
            Assert.Contains("Second batch - Entry 2", logContents[3]);
        }
    }
}