using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CalendarHelper.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CalendarHelper
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                var config = (string) Configuration["Redis"];
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

            app.Run(async (context) =>
            {
                try
                {
                    var logger = loggerFactory.CreateLogger("CalendarHelper.Startup");
               
                   logger.LogInformation("Log Info");
            

                    var urlsArray = UrlsArray();
                  
                    //AddHeaders(context);
                    var calendarCreator = new CalendarCreator(logger, memoryCache);
                    await context.Response.WriteAsync(await calendarCreator.Merge(urlsArray));
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger("CalendarHelper.Startup");
                    logger.LogError(new EventId(-1, "Startup configure error"), ex, "Mark's Error Message", null);
                }

            });
        
        }

        private static void AddHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;
           
            headers.Add("X-Content-Type-Options", "nosniff");
            headers.Add("X-Frame-Options", "SAMEORIGIN");
            headers.Add("X-XSS-Protection", "1; mode=block");
            headers.Add("Pragma", "no-cache");
            headers.Add("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
            //context.Response.ContentType = "text/calendar; charset=UTF-8";
            //headers.Add("Content-Disposition", "attachment; filename=StPete139.ics");
        }

        private string[] UrlsArray()
        {
            var urls = (string) Configuration["URLs"];
            string[] urlsArray;
            if (urls.Contains(";"))
            {
                urlsArray = urls.Split(';');
            }
            else
            {
                urlsArray = new[] {urls};
            }
            return urlsArray;
        }
    }
}
