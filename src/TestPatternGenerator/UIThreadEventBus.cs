namespace TestPatternGenerator;

internal class UiThreadEventBus : IEventBus
{
    public event EventHandler<ApplicationEventArgs>? EventRaised;

    public void SendEvent(ApplicationEvents @event)
    {
        Application.OpenForms[0]?.BeginInvoke(() => EventRaised?.Invoke(this, new ApplicationEventArgs(@event)));
    }
}