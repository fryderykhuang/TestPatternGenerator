namespace TestPatternGenerator;

public class ApplicationEventArgs : EventArgs
{
    public ApplicationEventArgs(ApplicationEvents @event)
    {
        Event = @event;
    }

    public ApplicationEvents Event { get; }
}