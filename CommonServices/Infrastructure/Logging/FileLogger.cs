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
   
            _logQueue = new ConcurrentQueue<string>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LogAsync(IEnumerable<string> messages)
        {

            foreach (var message in messages)
            {
                _logQueue.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            }

            await ProcessLogQueueAsync();
        }

        private async Task ProcessLogQueueAsync()
        {          
            await _semaphore.WaitAsync();
            try
            {
                if (_isProcessing) return;
                _isProcessing = true;
             
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
