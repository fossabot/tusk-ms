﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Tusk.Story.Filters;
using Tusk.Story.Health;
using Tusk.Story.Persistance;

namespace Tusk.Story
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
            // Add MediatR
            services.AddMediatR(Assembly.GetExecutingAssembly());

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Tusk API", Version = "v1" });
            });

            // Add Health Checks
            services.AddHealthChecks()
                .AddCheck<ApiHealthCheck>("api");

            // Add DbContext using SQL Server Provider
            services.AddDbContext<TuskDbContext>(options =>
                options.UseInMemoryDatabase(new Guid().ToString()));

            services.AddMvc(options => options.Filters.Add(typeof(CustomExceptionFilter)))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(fv =>
                {
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                    //fv.RegisterValidatorsFromAssemblyContaining<CreateStoryValidator>();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tusk API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseMvc();
        }

        private static Task WriteHealthCheckResponse(
            HttpContext httpContext,
            HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(
                    result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("exception", pair.Value.Exception.Message),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value)))
                        )
                    )))
                ))
            );
            return httpContext.Response.WriteAsync(
                json.ToString(Formatting.Indented)
            );
        }
    }
}
