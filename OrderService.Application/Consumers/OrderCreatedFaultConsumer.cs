using MassTransit;
using OrderService.Domain.Contracts;

namespace OrderService.Application.Consumers;

public class OrderCreatedFaultConsumer : IConsumer<Fault<OrderCreated>>
{
    public async Task Consume(ConsumeContext<Fault<OrderCreated>> context)
    {
        var originalMessage = context.Message.Message;
        var exceptions = context.Message.Exceptions;

        // Тут логіка: наприклад, записати в лог "Critical Error" 
        // або сповістити адміністратора про проблему з клієнтом {originalMessage.CustomerEmail}
        Console.WriteLine($"❌ КРИТИЧНА ПОМИЛКА: Замовлення {originalMessage.OrderId} не оброблено.");
    }
}