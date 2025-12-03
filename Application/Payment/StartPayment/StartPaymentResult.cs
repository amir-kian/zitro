namespace Application.Payment.StartPayment;

public record StartPaymentResult(bool Success, string? ErrorMessage, Guid? PaymentId);

