namespace PrivateUtilities.EventBus
{
    public interface IEventBus
    {
        void Publish(EventBase @event);
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : EventBase;
    }
}