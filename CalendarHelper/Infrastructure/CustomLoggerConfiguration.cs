using System;
using Microsoft.Extensions.Logging;

namespace CalendarHelper.Infrastructure
{
    public class CustomLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public int EventId { get; set; } = 0;
        public ConsoleColor Color { get; set; } = ConsoleColor.Yellow;
    }

}
