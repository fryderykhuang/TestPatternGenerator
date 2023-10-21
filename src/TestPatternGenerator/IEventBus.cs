namespace TestPatternGenerator;

public interface IEventBus
{
    void SendEvent(ApplicationEvents @event);
    event EventHandler<ApplicationEventArgs> EventRaised;
}