namespace Domain.Products
{
    public class Product
    {
        public Product(ProductId id, string name, Money price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        private Product()
        {
        }

        public ProductId Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public Money Price { get; private set; }

        public bool IsSold { get; private set; }

        public void Update(string name, Money price)
        {
            Name = name;
            Price = price;
        }

        public void MarkAsSold()
        {
            IsSold = true;
        }
    }
}
