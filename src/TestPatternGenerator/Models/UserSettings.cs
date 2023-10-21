namespace TestPatternGenerator;

public class UserSettings
{
    public PatternSettings Patterns { get; set; } = new();
    public PatternViewState PatternViewState { get; set; } = new();
}