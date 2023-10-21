namespace TestPatternGenerator;

public class ConvergencePageSettings
{
    public int ColorOffset { get; set; }
    public int StrokeThickness { get; set; } = 1;
    public ConvergencePatterns Pattern { get; set; }
    public bool RedEnabled { get; set; } = true;
    public bool GreenEnabled { get; set; } = true;
    public bool BlueEnabled { get; set; } = true;
}