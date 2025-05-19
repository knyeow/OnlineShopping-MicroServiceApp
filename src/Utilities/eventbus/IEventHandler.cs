
namespace PrivateUtilities.EventBus
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T @event);
    }
}
