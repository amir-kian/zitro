using Application.Payment.StartPayment;

namespace Application.Services;

public interface IRabbitMqService
{
    Task PublishPaymentMessageAsync(PaymentMessage message);
}

