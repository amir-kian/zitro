using MediatR;

namespace Application.Products.Create;

public record CreateProductCommand(
    string Name,
    string Currency,
    decimal Amount) : IRequest;
