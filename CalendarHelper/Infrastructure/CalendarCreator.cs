using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace CalendarHelper.Infrastructure
{
    public class CalendarCreator
    {
        public string CreateDemo()
        {
            var now = DateTime.Now;
            var later = now.AddHours(1);

            //Repeat daily for 5 days
            var rrule = new RecurrencePattern(FrequencyType.Weekly, 1) { Count = 5 };

            var e = new CalendarEvent
            {
                Summary =  "Summary",
                Location = "123 Fake St",
                Status = "Status",
                Start = new CalDateTime(now),
                End = new CalDateTime(later),
                RecurrenceRules = new List<RecurrencePattern> { rrule },
                
            };
            
            var calendar = new Calendar();
            calendar.Events.Add(e);
            return new CalendarSerializer().SerializeToString(calendar);
        }
    }
}
