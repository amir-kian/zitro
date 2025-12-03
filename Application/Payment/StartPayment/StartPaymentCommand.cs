using MediatR;

namespace Application.Payment.StartPayment;

public record StartPaymentCommand(string UserId) : IRequest<StartPaymentResult>;

