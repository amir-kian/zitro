using System.Text.Json;
using Domain;
using Domain.Products;
using StackExchange.Redis;

namespace Application.Data;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private const string BasketKeyPrefix = "Basket:";
    private const string LockKeyPrefix = "Lock:";

    public RedisService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<Domain.Basket?> GetBasketAsync(string userId)
    {
        var key = $"{BasketKeyPrefix}{userId}";
        var value = await _database.StringGetAsync(key);
        
        if (value.IsNullOrEmpty)
            return null;

        var items = JsonSerializer.Deserialize<List<BasketItem>>(value!);
        if (items == null)
            return null;

        var basket = new Domain.Basket(userId);
        foreach (var item in items)
        {
            basket.AddItem(item.ProductId);
        }
        return basket;
    }

    public async Task SaveBasketAsync(string userId, Domain.Basket basket)
    {
        var key = $"{BasketKeyPrefix}{userId}";
        var value = JsonSerializer.Serialize(basket.Items);
        await _database.StringSetAsync(key, value);
    }

    public async Task DeleteBasketAsync(string userId)
    {
        var key = $"{BasketKeyPrefix}{userId}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> TryLockProductAsync(ProductId productId, string userId)
    {
        var key = $"{LockKeyPrefix}{productId.Value}";
        
         return await _database.StringSetAsync(key, userId, TimeSpan.FromMinutes(10), When.NotExists);
    }

    public async Task<bool> IsProductLockedAsync(ProductId productId)
    {
        var key = $"{LockKeyPrefix}{productId.Value}";
        return await _database.KeyExistsAsync(key);
    }

    public async Task<string?> GetProductLockOwnerAsync(ProductId productId)
    {
        var key = $"{LockKeyPrefix}{productId.Value}";
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task ReleaseProductLockAsync(ProductId productId)
    {
        var key = $"{LockKeyPrefix}{productId.Value}";
        await _database.KeyDeleteAsync(key);
    }
}

