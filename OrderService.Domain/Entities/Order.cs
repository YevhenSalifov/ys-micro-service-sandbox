namespace OrderService.Domain.Entities;

// КРИТИЧНО: клас має бути public!
public class Order
{
    // CosmosDB вимагає поле "id" (саме маленькими літерами в JSON)
    // для десеріалізації в .NET ми використовуємо атрибут
    [Newtonsoft.Json.JsonProperty("id")] 
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Це поле ми будемо використовувати як Partition Key
    public string PartitionKey => Id; 
}