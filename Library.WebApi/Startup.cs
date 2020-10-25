using System;
using CommonData.Configuration;
using GreenPipes;
using Library.WebApi.Consumer;
using Library.WebApi.Models;
using Library.WebApi.Services;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Library.WebApi
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
            services.Configure<LibraryDatabaseSettings>(
                Configuration.GetSection(nameof(LibraryDatabaseSettings)));

            services.AddSingleton<ILibraryDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<LibraryDatabaseSettings>>().Value);

            services.AddSingleton<LibraryService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            var rabbitConfiguration = Configuration.GetSection("RabbitMQ").Get<RabbitMqConfiguration>();
            services.AddMassTransit(x =>
            {

                x.AddConsumer<EditBookDbConsumer>();
                x.AddConsumer<ListBooksConsumer>();
                x.AddConsumer<UserLoginConsumer>();
                x.AddConsumer<UserRegisterConsumer>();
                x.AddConsumer<UserUpdateConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.ConfigureJsonSerializer(setting => { setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; return setting; });
                    var host = cfg.Host(new Uri(rabbitConfiguration.ServerAddress), hostConfigurator => 
                    { 
                        hostConfigurator.Username(rabbitConfiguration.Username);
                        hostConfigurator.Password(rabbitConfiguration.Password);
                    });

                    cfg.ReceiveEndpoint(host, "book-update", ep =>
                    {
                        ep.PrefetchCount = 1;
                        ep.UseMessageRetry(r => r.Interval(5, 500));

                        ep.Consumer<EditBookDbConsumer>(provider);
                    });

                    cfg.ReceiveEndpoint(host, "book-show", ep =>
                    {
                        ep.PrefetchCount = 1;
                        ep.UseMessageRetry(r => r.Interval(5, 500));

                        ep.Consumer<ListBooksConsumer>(provider);
                    });

                    cfg.ReceiveEndpoint(host, "user-login", ep =>
                    {
                        ep.PrefetchCount = 1;
                        ep.UseMessageRetry(r => r.Interval(5, 500));

                        ep.Consumer<UserLoginConsumer>(provider);
                    });

                    cfg.ReceiveEndpoint(host, "user-register", ep =>
                    {
                        ep.PrefetchCount = 1;
                        ep.UseMessageRetry(r => r.Interval(5, 500));

                        ep.Consumer<UserRegisterConsumer>(provider);
                    });

                    cfg.ReceiveEndpoint(host, "user-update", ep =>
                    {
                        ep.PrefetchCount = 1;
                        ep.UseMessageRetry(r => r.Interval(5, 500));

                        ep.Consumer<UserUpdateConsumer>(provider);
                    });
                }));
            });

            
            services.AddSingleton<IPublishEndpoint>(provider => provider.GetRequiredService<IBusControl>());
            services.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());

            services.AddSingleton<IHostedService, BusService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Backend in now running.");

            });
        }
    }
}