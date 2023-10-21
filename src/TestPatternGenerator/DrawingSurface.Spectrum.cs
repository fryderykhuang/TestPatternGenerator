using System.Drawing.Drawing2D;

namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private double _currentHue;
    private int _hueBlockSize = 1;
    private SpectrumPageSettings _spectrumSettings = new();

    private void OnSpectrumSettingsChanged(SpectrumPageSettings? obj)
    {
        if (obj == null)
            return;

        _spectrumSettings = obj;
        UpdateDrawingFunction();

        Invalidate();
    }

    private void DrawHorizontalSpectrumPattern(Graphics g)
    {
        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        var brush = new SolidBrush(Color.Black);
        var steps = (int) Math.Ceiling(_screenWidth / (double) _hueBlockSize);
        var hStep = 360d / steps;
        var h = _currentHue;
        for (var x = 0; x < _screenWidth; x += _hueBlockSize, h += hStep)
        {
            brush.Color = ColorFromHsv(h % 360, 1, 1);
            g.FillRectangle(brush, x, 0, _hueBlockSize, _screenHeight);
        }
    }

    private void DrawPieSpectrumPattern(Graphics g)
    {
        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        var brush = new SolidBrush(Color.Black);
        var pen = new Pen(Color.Black);
        var size = Math.Min(_screenWidth, _screenHeight);
        float x, y;
        double stepAngle;
        if (_screenWidth > _screenHeight)
        {
            x = (int) Math.Round(_screenHalfWidth - size / 2f);
            y = 0;
            stepAngle = _hueBlockSize * 10 / (double) _screenHalfHeight;
        }
        else
        {
            y = (int) Math.Round(_screenHalfHeight - size / 2f);
            x = 0;
            stepAngle = _hueBlockSize * 10 / (double) _screenHalfWidth;
        }

        var steps = (int) Math.Ceiling(360 / stepAngle);
        var hStep = 360d / steps;
        var h = _currentHue;

        for (var theta = 0d; theta < 360d; theta += stepAngle, h += hStep)
        {
            brush.Color = ColorFromHsv(h % 360, 1, 1);
            pen.Color = brush.Color;
            g.DrawPie(pen, x, y, size, size, (float) theta, (float) stepAngle);
            g.FillPie(brush, x, y, size, size, (float) theta, (float) stepAngle);
        }
    }

    private static void ColorToHsv(Color color, out double hue, out double saturation, out double value)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        hue = color.GetHue();
        saturation = max == 0 ? 0 : 1d - 1d * min / max;
        value = max / 255d;
    }

    private static Color ColorFromHsv(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60d)) % 6;
        var f = hue / 60d - Math.Floor(hue / 60d);

        value *= 255d;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - f * saturation));
        var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(255, v, t, p),
            1 => Color.FromArgb(255, q, v, p),
            2 => Color.FromArgb(255, p, v, t),
            3 => Color.FromArgb(255, p, q, v),
            4 => Color.FromArgb(255, t, p, v),
            _ => Color.FromArgb(255, v, p, q)
        };
    }
}