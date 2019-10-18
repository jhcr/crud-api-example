using System;
using System.Reflection;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Models;
using GuidDataCRUD.Infrastructure.Database;
using GuidDataCRUD.Infrastructure.Cache;
using GuidDataCRUD.Web.Middlewares;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using GuidDataCRUD.Web.Filters;
using CorrelationId;
using NLog.Extensions.Logging;
using NLog.Web;

namespace GuidDataCRUD
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            })
            .AddLogging()
            .AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "GuidData API", Version = "v1" });

                // Swagger reads XML comments from below files.
                foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
            })
            .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddMvcOptions(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            })
            .AddJsonOptions(o =>
            {
                o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                o.SerializerSettings.DateParseHandling = DateParseHandling.None;
                o.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                o.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ";
            });

            services.AddCorrelationId();

            services.Configure<AppSettings>(Configuration);
            
            services.AddSingleton<IGuidDataService, GuidDataService>();
            services.AddSingleton<IGuidDataRepository>(x=>new RedisGuidDataDecorator(new SqlGuidDataRepository(x.GetService<IOptions<AppSettings>>()), x.GetService<IDistributedCache>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add Correlation Id in request/response header and sync across API calls
            app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = "X-Correlation-ID",
                UseGuidForCorrelationId = false,
                UpdateTraceIdentifier = true
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseMvc();

            app.UseGlobalErrorHandler(loggerFactory.CreateLogger("GlobalErrorHandler"));

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GuidData API");
            });

            
        }
    }
}
