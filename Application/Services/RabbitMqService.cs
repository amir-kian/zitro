using System.Text;
using System.Text.Json;
using Application.Payment.StartPayment;
using RabbitMQ.Client;

namespace Application.Services
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IChannel? _channel;
        private readonly SemaphoreSlim _channelLock = new SemaphoreSlim(1, 1);
        private const string QueueName = "payment_queue";
        private const string ExchangeName = "payment_exchange";

        public RabbitMqService(RabbitMQ.Client.IConnection connection)
        {
            _connection = connection;
        }

        private async Task<RabbitMQ.Client.IChannel> GetChannelAsync()
        {
            if (_channel != null)
                return _channel;

            await _channelLock.WaitAsync();
            try
            {
                if (_channel != null)
                    return _channel;

                _channel = await _connection.CreateChannelAsync();
                
                await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
                
                await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                
                await _channel.QueueBindAsync(QueueName, ExchangeName, QueueName);

                return _channel;
            }
            finally
            {
                _channelLock.Release();
            }
        }

        public async Task PublishPaymentMessageAsync(PaymentMessage message)
        {
            var channel = await GetChannelAsync();
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: QueueName,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _channelLock?.Dispose();
        }
    }
}

