using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.TimeZones;

namespace CalendarHelper.Infrastructure
{
    public class CalendarCreator
    {
       
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private readonly DateTime StartDate = DateTime.Today.AddDays(-7);
        private readonly DateTime EndDate = DateTime.Today.AddDays(90);
        public async Task<string> Merge(string[] urlsArray)
        {

            var aKey = _env.ApplicationName + "." + _env.EnvironmentName;

            var raw = await _cache.GetAsync(aKey);
            if (raw != null)
            {
                return Encoding.UTF8.GetString(raw);
            }


            var downloadTasks = urlsArray.Where(f => !String.IsNullOrEmpty(f)).Select(GetUrlAsCalendar).ToArray();
            var calendars = await Task.WhenAll(downloadTasks);
            var toSave = await ProcessCalendarsAsync(calendars, aKey);
            return toSave;
        }


        private async Task<string> ProcessCalendarsAsync(Calendar[] calendars, string aKey)
        {
            Calendar retVal = new Calendar();
            foreach (var calendar in calendars)
            {
                foreach (var calendarEvent in calendar.Events)
                {
                    if (calendarEvent.GetOccurrences(StartDate, EndDate).Any())
                    {
                        _logger.LogInformation(JsonConvert.SerializeObject(calendarEvent.Summary, _settings));
                        var toAdd = CopyCalendarEvent(calendarEvent);
                        retVal.Events.Add(toAdd);
                    }
                    else
                    {
                        _logger.LogInformation("skipping");
                    }
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
            //_logger.LogWarning("Time To Serialize");
            var retStr = new CalendarSerializer().SerializeToString(retVal);
            var opt = new DistributedCacheEntryOptions();
            opt.SetAbsoluteExpiration(TimeSpan.FromHours(3));
            await _cache.SetAsync(aKey, Encoding.UTF8.GetBytes(retStr), opt);
            return retStr;
        }


        private static readonly List<string> Tzids = new List<string>();
        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private IHostingEnvironment _env;

        public CalendarCreator(ILogger logger, IDistributedCache cache, IHostingEnvironment env)
        {
            _logger = logger;
            _cache = cache;
            _env = env;
        }

        private static CalendarEvent CopyCalendarEvent(CalendarEvent calendarEvent)
        {
            Tzids.Add(calendarEvent.DtStart.TzId);

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
            _logger.LogInformation("GetUrlAsCalendar - " + url);
            string cacheEntry;
            var aKey = Base64.Base64Encode(url);

            var value = await _cache.GetAsync(aKey);
            if (value != null)
            {
                //_logger.LogWarning("Cache Hit");
                //_logger.LogWarning(aKey);
                cacheEntry = Encoding.UTF8.GetString(value);
            }
            else
            {
                //_logger.LogWarning("Cache Miss");
                // _logger.LogWarning(aKey);
                var opt = new DistributedCacheEntryOptions();
                opt.SetAbsoluteExpiration(TimeSpan.FromHours(3));
                cacheEntry = await GetUrlAsString(url);
         //       await _cache.SetAsync(aKey, Encoding.UTF8.GetBytes(cacheEntry), opt);
            }
            return Calendar.Load(cacheEntry);
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
