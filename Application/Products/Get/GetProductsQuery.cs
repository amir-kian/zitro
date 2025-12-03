using MediatR;

namespace Application.Products.Get
{
    public record GetProductsQuery() : IRequest<List<ProductWithStatusResponse>>;

    public record ProductWithStatusResponse(
        Guid Id,
        string Name,
        string Currency,
        decimal Amount,
        bool IsSold,
        bool IsLocked,
        string? LockedBy);
}

