using MassTransit;
using OrderService.Domain.Contracts;
using Microsoft.Extensions.Logging;

namespace OrderService.Application.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;
        _logger.LogInformation("Отримано нове замовлення: {OrderId} для {Email}",
            message.OrderId, message.CustomerEmail);

        // Тут ми пізніше додамо збереження в CosmosDB
        await Task.CompletedTask;
    }
}
