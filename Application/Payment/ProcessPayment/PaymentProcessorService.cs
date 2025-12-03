using Application.Data;
using Application.Payment.StartPayment;
using Domain.Products;

namespace Application.Payment.ProcessPayment;

public interface IPaymentProcessorService
{
    Task<bool> ProcessPaymentAsync(PaymentMessage message);
}

public class PaymentProcessorService(
    IRedisService redisService,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IPaymentProcessorService
{
    private readonly IRedisService _redisService = redisService;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly Random _random = new();
    private readonly bool _paymentSuccess = true;//To do : depends on business

    public async Task<bool> ProcessPaymentAsync(PaymentMessage message)
    {


        if (_paymentSuccess)
        {
            foreach (var productIdValue in message.ProductIds)
            {
                var productId = new ProductId(productIdValue);
                
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    product.MarkAsSold();
                    _productRepository.Update(product);
                }

                await _redisService.ReleaseProductLockAsync(productId);
            }

            await _redisService.DeleteBasketAsync(message.UserId);

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return true;
        }
        else
        {
            return false;
        }
    }
}

