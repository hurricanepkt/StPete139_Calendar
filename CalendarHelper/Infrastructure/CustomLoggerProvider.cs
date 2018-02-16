using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CalendarHelper.Infrastructure
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly CustomLoggerConfiguration _customLoggerConfiguration;
        private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new ConcurrentDictionary<string, CustomLogger>();
        public CustomLoggerProvider(CustomLoggerConfiguration customLoggerConfiguration)
        {
            try
            {
                _customLoggerConfiguration = customLoggerConfiguration;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _loggers.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            try
            {
                return _loggers.GetOrAdd(categoryName, name => new CustomLogger(name, _customLoggerConfiguration));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
