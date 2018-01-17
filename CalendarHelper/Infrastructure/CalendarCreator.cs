using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.TimeZones;

namespace CalendarHelper.Infrastructure
{
    public class CalendarCreator
    {
        public async Task<string> Merge(string[] urlsArray)
        {
            var retVal = new Calendar();
            var downloadTasks = urlsArray.Where(f=> !String.IsNullOrEmpty(f)).Select(GetUrlAsCalendar).ToArray();
            var calendars = await Task.WhenAll(downloadTasks);
         
            
            foreach (var calendar in calendars)
            {
                foreach (var calendarEvent in calendar.Events)
                {
                   
                    
                  
                        var toAdd = CopyCalendarEvent(calendarEvent);
                        retVal.Events.Add(toAdd);
                 
                }
                //       retVal.Events.AddRange(calendar.Events);
            }
            foreach (var tzid in Tzids.Distinct())
            {
                if (!String.IsNullOrEmpty(tzid))
                {
                    _logger.LogInformation("TzId = \"" + tzid + "\"");
                    retVal.AddTimeZone(tzid, new DateTime(2000, 1, 1), true);
                }
            }
           // File.AppendAllText(@"C:\Users\mark.greenway\Desktop\trash\playing\tzids.txt", String.Join("\n",Tzids.Distinct().ToArray()));
            return new CalendarSerializer().SerializeToString(retVal);

        }
       

        private static readonly List<string> Tzids = new List<string>();
        private readonly ILogger _logger;
        public CalendarCreator(ILogger logger)
        {
            _logger = logger;
        }

        private static CalendarEvent CopyCalendarEvent(CalendarEvent calendarEvent)
        {
            Tzids.Add(calendarEvent.DtStart.TzId);
            IDateTime staDateTime = new CalDateTime(calendarEvent.DtStart.Value, calendarEvent.DtStart.TzId);
            var toAdd = new CalendarEvent
            {
               
                DtStart = calendarEvent.DtStart,
                DtEnd = calendarEvent.DtEnd,
                Duration = calendarEvent.Duration,
               
                IsAllDay = calendarEvent.IsAllDay,
                GeographicLocation = calendarEvent.GeographicLocation,
                Location = calendarEvent.Location,
                Resources = calendarEvent.Resources,
                Status = calendarEvent.Status,
                Transparency = calendarEvent.Transparency,
                Attachments = calendarEvent.Attachments,
                Categories = calendarEvent.Categories,
                Class = calendarEvent.Class,
                Contacts = calendarEvent.Contacts,
                Created = calendarEvent.Created,
                Description = calendarEvent.Description,

                LastModified = calendarEvent.LastModified,
                Priority = calendarEvent.Priority,
            
                RelatedComponents = calendarEvent.RelatedComponents,
                Sequence = calendarEvent.Sequence,
               
                Summary = calendarEvent.Summary,
                Attendees = calendarEvent.Attendees,
                Comments = calendarEvent.Comments,
                DtStamp = calendarEvent.DtStamp,
                Organizer = calendarEvent.Organizer,
                RequestStatuses = calendarEvent.RequestStatuses,
                Url = calendarEvent.Url,
                Uid = calendarEvent.Uid,
                Parent = calendarEvent.Parent,
                Name = calendarEvent.Name,
                Line = calendarEvent.Line,
                Column = calendarEvent.Column,
                Group = calendarEvent.Group
            };
            Reoccurrence(toAdd, calendarEvent);
            return toAdd;
        }


        private static void Reoccurrence(CalendarEvent toAdd, CalendarEvent calendarEvent)
        {
            toAdd.RecurrenceDates = PeriodListList(calendarEvent.RecurrenceDates.AsEnumerable());
            toAdd.ExceptionDates = PeriodListList(calendarEvent.ExceptionDates);

            toAdd.RecurrenceRules = RecurrencePatterns(calendarEvent.RecurrenceRules);
            toAdd.ExceptionRules = RecurrencePatterns(calendarEvent.ExceptionRules);

        }

        private static List<RecurrencePattern> RecurrencePatterns(IList<RecurrencePattern> blah)
        {
            var dest = new List<RecurrencePattern>();
            foreach (var calendarEventRecurrenceRule in blah)
            {
                dest.Add(calendarEventRecurrenceRule);
            }
            return dest;
            
        }

        private static List<PeriodList> PeriodListList(IEnumerable<PeriodList> blah)
        {
            var periods = new List<PeriodList>();
            foreach (var date in blah)
            {
                var periodList = new PeriodList();
                foreach (var period in date)
                {
                    periodList.Add(period);
                }
                periods.Add(periodList);
            }
            return periods;
        }

        private async Task<Calendar> GetUrlAsCalendar(string url)
        {
            var iCalendarString = await GetUrlAsString(url);
            return Calendar.Load(iCalendarString);
        }

        private async Task<string> GetUrlAsString(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/calendar"));
            client.DefaultRequestHeaders.Add("User-Agent", "StPete139 Calendar tools");
            var stringTask = client.GetStringAsync(url);

            return await stringTask;
        }
    }
}
