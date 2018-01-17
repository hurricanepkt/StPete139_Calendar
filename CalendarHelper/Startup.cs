using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CalendarHelper.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
        
            app.Run(async (context) =>
            {
                
                var logger = loggerFactory.CreateLogger("CalendarHelper.Startup");
                logger.LogInformation("Log Info");
                try
                {

                    var urls = (string)Configuration["URLs"];
                    var urlsArray = urls.Split(';');

                    //context.Response.ContentType = "text/calendar";
                    //context.Response.Headers.Add("Content-Disposition", "attachment; filename=StPete139.ics");
                    var calendarCreator = new CalendarCreator(logger);
                    await context.Response.WriteAsync(await calendarCreator.Merge(urlsArray));
                }
                catch (Exception ex)
                {
                    logger.LogError(new EventId(-1, "Startup configure error"), ex, "Mark's Error Message", null);
                }

            });
        }
    }
}
