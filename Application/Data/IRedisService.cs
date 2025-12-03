using Domain.Products;

namespace Application.Data;

public interface IRedisService
{
    Task<Domain.Basket?> GetBasketAsync(string userId);
    Task SaveBasketAsync(string userId, Domain.Basket basket);
    Task DeleteBasketAsync(string userId);
    Task<bool> TryLockProductAsync(ProductId productId, string userId);
    Task<bool> IsProductLockedAsync(ProductId productId);
    Task<string?> GetProductLockOwnerAsync(ProductId productId);
    Task ReleaseProductLockAsync(ProductId productId);
}

