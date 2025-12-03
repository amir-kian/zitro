using MediatR;

namespace Application.Basket.AddToBasket;

public record AddToBasketCommand(
    string UserId,
    Guid ProductId) : IRequest<AddToBasketResult>;

