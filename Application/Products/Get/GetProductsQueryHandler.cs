using Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Products.Get
{
    internal sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductWithStatusResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IRedisService _redisService;

        public GetProductsQueryHandler(
            IApplicationDbContext context,
            IRedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        public async Task<List<ProductWithStatusResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _context
                .Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.IsSold
                })
                .ToListAsync(cancellationToken);

            var result = new List<ProductWithStatusResponse>();

            foreach (var product in products)
            {
                var isLocked = await _redisService.IsProductLockedAsync(product.Id);
                string? lockedBy = null;
                if (isLocked)
                {
                    lockedBy = await _redisService.GetProductLockOwnerAsync(product.Id);
                }

                result.Add(new ProductWithStatusResponse(
                    product.Id.Value,
                    product.Name,
                    product.Price.Currency,
                    product.Price.Amount,
                    product.IsSold,
                    isLocked,
                    lockedBy));
            }

            return result;
        }
    }
}

