using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Consumers;
using OrderService.Domain.Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.UnitTests;

public class OrderCreatedConsumerTests
{
    [Fact]
    public async Task OrderCreatedConsumer_ShouldConsumeMessage()
    {
        // 1. Arrange: Налаштовуємо тестове середовище MassTransit
        var loggerMock = new Mock<ILogger<OrderCreatedConsumer>>();
        
        // Використовуємо ServiceProvider для імітації DI
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();
            })
            .AddSingleton(loggerMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // 2. Act: Публікуємо повідомлення в шину
        var orderId = Guid.NewGuid();
        await harness.Bus.Publish(new OrderCreated 
        { 
            OrderId = orderId, 
            CustomerEmail = "test@example.com",
            Amount = 150.50m
        });

        // 3. Assert: Перевіряємо результати
        // Чи було повідомлення спожите взагалі?
        (await harness.Consumed.Any<OrderCreated>()).Should().BeTrue();
        
        // Чи конкретний Consumer його обробив?
        (await harness.GetConsumerHarness<OrderCreatedConsumer>().Consumed.Any<OrderCreated>()).Should().BeTrue();

        // Перевіряємо, що логер був викликаний (наша бізнес-логіка)
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(orderId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}