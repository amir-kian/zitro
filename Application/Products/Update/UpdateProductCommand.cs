using Domain.Products;
using MediatR;

namespace Application.Products.Update;

public record UpdateProductCommand(
    ProductId ProductId,
    string Name,
    string Currency,
    decimal Amount) : IRequest;

