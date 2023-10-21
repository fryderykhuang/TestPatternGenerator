namespace TestPatternGenerator;

public sealed partial class DrawingSurface
{
    private int _whiteBalanceColor1;
    private int _whiteBalanceColor2;
    private int _whiteBalancePatchHeight;
    private WhiteBalancePageSettings _whiteBalanceSettings = new();

    private void OnWhiteBalancePageSettingsChanged(WhiteBalancePageSettings? obj)
    {
        if (obj == null)
            return;

        _whiteBalanceSettings = obj;

        switch (_whiteBalanceSettings.Pattern)
        {
            case WhiteBalancePatterns.BlackPoint:
                _whiteBalanceColor1 = _whiteBalanceSettings.BlackPointColor1;
                _whiteBalanceColor2 = _whiteBalanceSettings.BlackPointColor2;
                break;
            case WhiteBalancePatterns.WhitePoint:
                _whiteBalanceColor1 = _whiteBalanceSettings.WhitePointColor1;
                _whiteBalanceColor2 = _whiteBalanceSettings.WhitePointColor2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Invalidate();
    }

    private void DrawWhiteBalancePattern(Graphics g)
    {
        var color = _whiteBalanceColor1;
        var colorStep = (_whiteBalanceColor2 - color) / _whiteBalanceSettings.PatchCount;
        var x = _screenLeftNarrow;

        var ratio = _whiteBalanceSettings.PatchRatio;
        if (ratio >= 1) ratio = 1 - 1e-4f;
        else if (ratio <= 0) ratio = 1e-4f;
        var scale = ratio / (1 - ratio);
        var interval = (_screenRightNarrow - _screenLeftNarrow) /
                       (scale * _whiteBalanceSettings.PatchCount +
                        (_whiteBalanceSettings.PatchCount - 1));
        var patchWidth = interval * scale;

        for (var i = 0;
             i < _whiteBalanceSettings.PatchCount;
             i++, color += colorStep, x += (int) Math.Round(interval + patchWidth))
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(color, color, color)), x, _screenTopNarrow,
                patchWidth, _whiteBalancePatchHeight);
            g.DrawString($"{Math.Round(color / 2.55f, 1):F1}%", Font,
                new SolidBrush(color > 127 ? Color.Black : Color.LightGray), x, _screenTopNarrow);
        }
    }
}