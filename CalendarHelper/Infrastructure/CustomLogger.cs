using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace CalendarHelper.Infrastructure
{
    public class CustomLogger : ILogger
    {

        private readonly CustomLoggerConfiguration _config;
        private readonly string _name;


        public CustomLogger(string name, CustomLoggerConfiguration customLoggerConfiguration)
        {
            try
            {
                _name = name;
                _config = customLoggerConfiguration;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            try
            {
                return logLevel == _config.LogLevel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                if (_config.EventId != 0 && _config.EventId != eventId.Id)
                {
                    return;
                }
                var output = new string('!', 80) + "\n" +  $"{logLevel} - {eventId.Id}  - {_name} - {formatter(state, exception)}";
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}