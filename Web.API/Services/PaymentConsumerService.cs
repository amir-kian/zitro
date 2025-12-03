using System.Text;
using System.Text.Json;
using Application.Payment.ProcessPayment;
using Application.Payment.StartPayment;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Web.API.Services
{
    public class PaymentConsumerService : BackgroundService
    {
        private readonly RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IChannel? _channel;
        private readonly IServiceProvider _serviceProvider;
        private const string QueueName = "payment_queue";
        private const string ExchangeName = "payment_exchange";

        public PaymentConsumerService(RabbitMQ.Client.IConnection connection, IServiceProvider serviceProvider)
        {
            _connection = connection;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _channel = await _connection.CreateChannelAsync();
            
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
            
            await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            
            await _channel.QueueBindAsync(QueueName, ExchangeName, QueueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    var paymentMessage = JsonSerializer.Deserialize<PaymentMessage>(message);
                    if (paymentMessage != null)
                    {
                        await ProcessPaymentAsync(paymentMessage);
                    }
                    
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing payment message: {ex.Message}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessPaymentAsync(PaymentMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentProcessor = scope.ServiceProvider.GetRequiredService<IPaymentProcessorService>();
            
            var success = await paymentProcessor.ProcessPaymentAsync(message);
            
            Console.WriteLine($"Payment {message.PaymentId} processed. Success: {success}");
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}

