using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMq.Consumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region rabbitMq
            // Optional: Run a background task to monitor RabbitMQ health or perform work.


            // RabbitMQ configuration
            var rabbitHost = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "localhost";
            var rabbitUser = Environment.GetEnvironmentVariable("RABBIT_USER") ?? "guest";
            var rabbitPass = Environment.GetEnvironmentVariable("RABBIT_PASSWORD") ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = rabbitHost,
                UserName = rabbitUser,
                Password = rabbitPass,
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "message",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Wating for messages");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Recieved: {message}");

                await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);

            };
            //end creating consumer
            //consuming 
            await channel.BasicConsumeAsync("message",autoAck:false, consumer: consumer);
            Console.ReadLine();
            #endregion


            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
