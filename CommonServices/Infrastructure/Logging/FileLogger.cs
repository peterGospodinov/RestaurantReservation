using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace CommonServices.Infrastructure.Logging
{
    public class FileLogger : IFileLogger
    {
        private readonly string _logFilePath;
        private readonly ConcurrentQueue<string> _logQueue;
        private readonly SemaphoreSlim _semaphore;
        private bool _isProcessing;

        public FileLogger() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "application.log"))
        {
        }

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath) ?? string.Empty);

            // Initialize the log queue and semaphore using semaphoreSlim
            _logQueue = new ConcurrentQueue<string>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LogAsync(IEnumerable<string> messages)
        {

            foreach (var message in messages)
            {
                _logQueue.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }

            // Start processing the queue if not already in process
            await ProcessLogQueueAsync();
        }

        private async Task ProcessLogQueueAsync()
        {
            // Ensure only one process is handling the queue at a time
            await _semaphore.WaitAsync();
            try
            {
                if (_isProcessing) return;
                _isProcessing = true;

                // Process the queue until all messages are written
                while (_logQueue.TryDequeue(out var logEntry))
                {
                    await WriteLogAsync(logEntry);
                }
            }
            finally
            {
                _isProcessing = false;
                _semaphore.Release();
            }
        }

        private async Task WriteLogAsync(string logEntry)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    await writer.WriteLineAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
