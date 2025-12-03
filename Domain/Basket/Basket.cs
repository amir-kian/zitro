using Domain.Products;

namespace Domain
{
    public class Basket(string userId)
    {
        public string UserId { get; private set; } = userId;
        public List<BasketItem> Items { get; private set; } = new List<BasketItem>();

        public void AddItem(ProductId productId)
        {
            var existingItem = Items.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem == null)
            {
                Items.Add(new BasketItem(productId));
            }
        }

    }
}

