using Application.Data;
using Application.Services;
using MediatR;

namespace Application.Payment.StartPayment;

public class StartPaymentCommandHandler(
    IRedisService redisService,
    IRabbitMqService rabbitMqService) : IRequestHandler<StartPaymentCommand, StartPaymentResult>
{
    private readonly IRedisService _redisService = redisService;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;

    public async Task<StartPaymentResult> Handle(StartPaymentCommand request, CancellationToken cancellationToken)
    {
        var basket = await _redisService.GetBasketAsync(request.UserId);
        if (basket == null || !basket.Items.Any())
        {
            return new StartPaymentResult(false, "Basket is empty", null);
        }

        var paymentId = Guid.NewGuid();

        var paymentMessage = new PaymentMessage
        {
            PaymentId = paymentId,
            UserId = request.UserId,
            ProductIds = basket.Items.Select(x => x.ProductId.Value).ToList()
        };

        await _rabbitMqService.PublishPaymentMessageAsync(paymentMessage);

        return new StartPaymentResult(true, null, paymentId);
    }
}

