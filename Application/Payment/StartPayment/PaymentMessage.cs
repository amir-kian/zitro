namespace Application.Payment.StartPayment;

public class PaymentMessage
{
    public Guid PaymentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<Guid> ProductIds { get; set; } = new();
}

