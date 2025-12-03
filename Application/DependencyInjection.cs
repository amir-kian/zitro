using Application.Data;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using MediatR.NotificationPublishers;
using StackExchange.Redis;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblyContaining<ApplicationAssemblyReference>();

                config.NotificationPublisher = new TaskWhenAllPublisher();
            });

            return services;
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(connectionString));
            
            services.AddScoped<IRedisService, RedisService>();

            return services;
        }

        public static IServiceCollection AddRabbitMq(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<RabbitMQ.Client.IConnection>(sp =>
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    Uri = new Uri(connectionString)
                };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            services.AddScoped<IRabbitMqService, RabbitMqService>();

            return services;
        }
    }

}
