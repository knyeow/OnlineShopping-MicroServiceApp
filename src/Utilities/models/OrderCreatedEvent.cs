namespace PrivateUtilities.Models{

using PrivateUtilities.EventBus;
public class OrderCreatedEvent : EventBase
{
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public DateTime OrderDate { get; set; } = DateTime.Now;


}
public class OrderItem
{
    public int ProductId { get; set; }
    public int Amount { get; set; }
}
}