namespace PrivateUtilities.EventBus
{
    public abstract class EventBase
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    }
}