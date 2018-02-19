using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using CalendarHelper.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Slack;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CalendarHelper
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Console.WriteLine("Before");
            try
            {
                var builder = new ConfigurationBuilder();
                if (File.Exists(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                {

                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
                }
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                var config = (string)Configuration["Redis"];
                services.AddDistributedRedisCache(options =>
                {

                    options.Configuration = config;
                    options.InstanceName = "";
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Configure Services " + ex.Message);

            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDistributedCache memoryCache)
        {
           
            loggerFactory
                .AddConsole()  
                .AddDebug(LogLevel.Trace);
            
          
     
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }



            app.UseStaticFiles();
            app.UseResponseBuffering();

            ConfigureSlack(loggerFactory, Configuration["SlackLocation"], env.ApplicationName, env.EnvironmentName);

            app.Run(async (context) =>
            {
                try
                {
                    var logger = loggerFactory.CreateLogger("CalendarHelper.Startup");
                    Stopwatch sw = Stopwatch.StartNew();


                    var urlsArray = UrlsArray();

                    AddHeaders(context);
                    var calendarCreator = new CalendarCreator(logger, memoryCache, env);
                    var merge = await calendarCreator.Merge(urlsArray);
                    await context.Response.WriteAsync(merge);
                    sw.Stop();
                    //logger.LogWarning("Time To Execute : " + sw.Elapsed.ToString("g") + " at " + DateTime.UtcNow.ToString("u"));
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger("CalendarHelper.Startup");
                    logger.LogError(new EventId(-1, "Startup configure error"), ex, "Mark's Error Message", null);
                    await context.Response.WriteAsync("Exception ex\n" + ex.Message);
                }

            });

        }

        private static void ConfigureSlack(ILoggerFactory loggerFactory, string slackLocation, string appName, string envName)
        {
            var slackConfig = new SlackConfiguration()
            {
                MinLevel = LogLevel.Information,
                WebhookUrl = new Uri(slackLocation)
            };
            Func<string, LogLevel, Exception, bool> filter = (category, logLevel, exc) => logLevel >= LogLevel.Warning;
            loggerFactory.AddProvider(new SlackLoggerProvider(filter, slackConfig, new HttpClient(), appName, envName));
        }

        private static void AddHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            headers.Add("X-Content-Type-Options", "nosniff");
            headers.Add("X-Frame-Options", "SAMEORIGIN");
            headers.Add("X-XSS-Protection", "1; mode=block");
            headers.Add("Pragma", "no-cache");
            headers.Add("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
            headers.Add("Expires", "Mon, 01 Jan 1990 00:00:00 GMT");
            headers.Add("Accept-Ranges", "none");
            headers.Add("Vary", "Accept-Encoding");
            headers.Add("Transfer-Encoding", "chunked");
            context.Response.ContentType = "text/calendar; charset=UTF-8";
            //headers.Add("Content-Disposition", "inline; filename=StPete139.ics");
        }

        private string[] UrlsArray()
        {
            var urls = (string)Configuration["URLs"];
            string[] urlsArray;
            if (urls.Contains(";"))
            {
                urlsArray = urls.Split(';');
            }
            else
            {
                urlsArray = new[] { urls };
            }
            return urlsArray;
        }
    }
}
