using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region rabbitMq
// Optional: Run a background task to monitor RabbitMQ health or perform work.

var app = builder.Build();

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
using var channel= await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "message",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

for (var i = 0; i < 10; i++)
{
    var message = $"{DateTime.UtcNow}- {Guid.NewGuid()}";
    var body=Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "message",
        mandatory: true,
        basicProperties: new BasicProperties { Persistent = true },
        body: body);

    await Task.Delay(2000);
}

#endregion

// Add services to the container.
//builder.Services.AddRazorPages();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();


