using PrivateUtilities.EventBus;
using PrivateUtilities.Models;
using System.Text.Json;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly AppDbContext _context;

    public OrderCreatedEventHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        Console.WriteLine($"[Event Received] OrderCreatedEvent: {JsonSerializer.Serialize(@event)}");

        if (@event.Items == null || !@event.Items.Any())
        {
            Console.WriteLine("[Warning] Event has no items.");
            return;
        }

        foreach (var item in @event.Items)
        {
            Console.WriteLine($"[Processing Item] ProductId: {item.ProductId}, Amount: {item.Amount}");

            var product = await _context.Products.FindAsync(item.ProductId);

            if (product == null)
            {
                Console.WriteLine($"[Error] Product not found: {item.ProductId}");
                continue;
            }

            if (product.Stock < item.Amount)
            {
                Console.WriteLine($"[Warning] Not enough stock for ProductId: {item.ProductId}. Current: {product.Stock}, Requested: {item.Amount}");
                continue;
            }

            product.Stock -= item.Amount;
            Console.WriteLine($"[Stock Updated] ProductId: {item.ProductId}, New Stock: {product.Stock}");
        }

        await _context.SaveChangesAsync();
        Console.WriteLine("[Success] Stock changes saved to database.");
    }
}
