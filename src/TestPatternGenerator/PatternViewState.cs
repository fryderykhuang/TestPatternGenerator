using System.Drawing.Drawing2D;

namespace TestPatternGenerator;

public class PatternViewState
{
    public PatternCategories ActivePatternCategory { get; set; }
    public IList<PredefinedTransforms>? PredefinedTransforms { get; set; }
    public Matrix? TransformMatrix { get; set; } = new();
    public bool UseCustomTransformMatrix { get; set; }
}