using Application.Data;
using Domain.Products;
using MediatR;

namespace Application.Basket.AddToBasket;

public class AddToBasketCommandHandler(
    IRedisService redisService,
    IProductRepository productRepository) : IRequestHandler<AddToBasketCommand, AddToBasketResult>
{
    private readonly IRedisService _redisService = redisService;
    private readonly IProductRepository _productRepository = productRepository;

    public async Task<AddToBasketResult> Handle(AddToBasketCommand request, CancellationToken cancellationToken)
    {
        var productId = new ProductId(request.ProductId);
        
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return new AddToBasketResult(false, "Product not found");
        }

        if (product.IsSold)
        {
            return new AddToBasketResult(false, "Product is already sold");
        }

        var isLocked = await _redisService.IsProductLockedAsync(productId);
        if (isLocked)
        {
            var lockOwner = await _redisService.GetProductLockOwnerAsync(productId);
            if (lockOwner != request.UserId)
            {
                return new AddToBasketResult(false, "Product is locked by another user");
            }
        }

        var lockAcquired = await _redisService.TryLockProductAsync(productId, request.UserId);
        if (!lockAcquired && !isLocked)
        {
            return new AddToBasketResult(false, "Failed to acquire product lock");
        }

        var basket = await _redisService.GetBasketAsync(request.UserId) ?? new Domain.Basket(request.UserId);
        
        basket.AddItem(productId);
        
        await _redisService.SaveBasketAsync(request.UserId, basket);

        return new AddToBasketResult(true, null);
    }
}

