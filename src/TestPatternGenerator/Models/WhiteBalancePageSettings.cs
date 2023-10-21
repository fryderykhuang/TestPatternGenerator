namespace TestPatternGenerator;

public class WhiteBalancePageSettings
{
    public WhiteBalancePatterns Pattern { get; set; } = WhiteBalancePatterns.BlackPoint;
    public int BlackPointColor1 { get; set; } = 3;
    public int BlackPointColor2 { get; set; } = 64;
    public int WhitePointColor1 { get; set; } = 128;
    public int WhitePointColor2 { get; set; } = 255;
    public int PatchCount { get; set; } = 5;
    public float PatchRatio { get; set; } = 0.85f;
}