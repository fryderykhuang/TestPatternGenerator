namespace TestPatternGenerator;

public class PatternSettings
{
    public ResolutionPageSettings ResolutionPageSettings { get; set; } = new();
    public WhiteBalancePageSettings WhiteBalancePageSettings { get; set; } = new();
    public ConvergencePageSettings ConvergencePageSettings { get; set; } = new();
    public SpectrumPageSettings SpectrumPageSettings { get; set; } = new();
    public GammaPageSettings GammaPageSettings { get; set; } = new();
}