namespace TestPatternGenerator;

public class ResolutionPageSettings
{
    public ResolutionPatterns Pattern { get; set; } = ResolutionPatterns.HorizontalStripe;
    public int Color1Size { get; set; } = 1;
    public int Color2Size { get; set; } = 1;

    public int PatchSize { get; set; } = 150;
    public int Color2IncreasingInterval { get; set; } = 10;
    public Color Color1 { get; set; } = Color.White;
    public Color Color2 { get; set; } = Color.Black;
}