
using Application;
using Application.Payment.ProcessPayment;
using Carter;
using Persistence;
using Web.API.Extensions;

namespace Web.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddPersistence(builder.Configuration);
            builder.Services.AddApplication();
            
            var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
                ?? "localhost:6379";
            builder.Services.AddRedis(redisConnectionString);
            
            var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ") 
                ?? "amqp://guest:guest@localhost:5672";
            builder.Services.AddRabbitMq(rabbitMqConnectionString);
            
            builder.Services.AddScoped<IPaymentProcessorService, PaymentProcessorService>();
            
            builder.Services.AddHostedService<Services.PaymentConsumerService>();
            
            builder.Services.AddCarter();

            var app = builder.Build();

            app.ApplyMigrations();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();


            app.MapCarter();

            app.Run();
        }
    }
}
