using Domain.Products;

namespace Domain;

public class BasketItem(ProductId productId)
{
    public ProductId ProductId { get; private set; } = productId;
}

