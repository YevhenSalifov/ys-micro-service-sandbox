namespace OrderService.Domain.Contracts;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerEmail { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreatedAt { get; init; }
}
