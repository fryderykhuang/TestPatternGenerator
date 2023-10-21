using System.Drawing.Drawing2D;

namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private const string GammaHintText =
        "1. Press and hold left mouse button on gray area below to focus\n2. Drag to find a position with minimal brightness difference between left and right.\n3. Read the gamma value on the right.";

    private const string GammaHintText2 =
        "*If you can clearly see the alternating black and white strips on the left side, you should either:\n 1. Look further from you monitor.\n 2. Switch to a higher resolution. (CRT only, use native resolution otherwise.)";

    private const int GammaOverlayMargin = 80;

    private bool _drawGammaOverlay;
    private Point _gammaOverlayMouseLocation;
    private Rectangle _gammaRegion = Rectangle.Empty;
    private GammaPageSettings _gammaSettings = new();

    private void OnGammaSettingsChanged(GammaPageSettings? obj)
    {
        if (obj == null)
            return;

        _gammaSettings = obj;
        Invalidate();
    }

    private void DrawGammaPattern(Graphics g)
    {
        int marginBgX, marginBgY;
        if (_screenWidth > _screenHeight)
        {
            marginBgX = (_screenWidth - _screenHeight) / 2;
            marginBgY = 0;
        }
        else
        {
            marginBgX = 0;
            marginBgY = (_screenHeight - _screenWidth) / 2;
        }

        var wbg = _screenWidth - 2 * marginBgX;
        var hbg = _screenHeight - 2 * marginBgY;

        var hintText1Size = g.MeasureString(GammaHintText, _hintFont);
        var gammaStripMarginY = (int) Math.Round(hintText1Size.Height + 2);

        var gammaValueTextSize = g.MeasureString("2.2", Font);
        var gammaValueHalfTextHeight = (int) Math.Round(gammaValueTextSize.Height / 2d);
        var marginX = (int) Math.Round(gammaValueTextSize.Width + 4d);

        var x1 = marginBgX + marginX;
        var yBegin = marginBgY + gammaStripMarginY;
        var w = (wbg - 2 * marginX) / 2;
        var h = hbg - 2 * gammaStripMarginY;
        var x2 = x1 + w;
        var x2End = x2 + w;
        var yEnd = _screenHeight - yBegin;


        g.FillRectangle(Brushes.White, marginBgX, marginBgY, wbg, hbg);

        if (_gammaSettings.PortraitMode)
        {
            _gammaRegion = new Rectangle(x1, yBegin, w * 2, h);

            var trans = new Matrix(1, 0, 0, 1, _screenHalfWidth, _screenHalfHeight);
            trans.Multiply(_matRotate90CW);
            trans.Translate(-_screenHalfWidth, -_screenHalfHeight);
            g.Transform = trans;
        }
        else
        {
            _gammaRegion = new Rectangle(x1, yBegin, w * 2, h);
        }

        for (var y = yBegin; y < yEnd;)
        {
            g.FillRectangle(Brushes.Black, x1, y, w, _gammaSettings.StripeThickness);
            y += _gammaSettings.StripeThickness;
            g.FillRectangle(Brushes.White, x1, y, w, _gammaSettings.StripeThickness);
            y += _gammaSettings.StripeThickness;
        }

        var pen = new Pen(Color.Black);
        var kMin = 0.5d;
        var kMax = 4d;
        var kStep = (kMax - kMin) / (yEnd - yBegin);
        var gammaValueInterval = (int) gammaValueTextSize.Height + 2;
        var k = kMax;
        for (int y = yBegin, i = 0; y < yEnd; y++, k = kMax - ++i * kStep)
        {
            var c = (int) Math.Round(Math.Pow(0.5d, 1 / k) * 255);
            pen.Color = Color.FromArgb(c, c, c);
            g.DrawLine(pen, x2, y, x2End, y);
            if (i % gammaValueInterval == 0)
                g.DrawString($"{Math.Round(k, 1):F1}", Font, Brushes.Black, x2End + 1, y - gammaValueHalfTextHeight);
        }

        var textWidth = hintText1Size.Width;
        g.DrawString(GammaHintText,
            _hintFont, Brushes.Black, _screenHalfWidth - textWidth / 2, 0f);
        var hintText2Size = g.MeasureString(GammaHintText2, _hintFont);
        var text2Width = hintText2Size.Width;
        g.DrawString(GammaHintText2,
            _hintFont, Brushes.Black, _screenHalfWidth - text2Width / 2, yEnd);

        if (_drawGammaOverlay)
        {
            if (_gammaSettings.PortraitMode)
            {
            }
            else
            {
                var mouseY = _gammaOverlayMouseLocation.Y;
                if (mouseY - GammaOverlayMargin < yBegin)
                    mouseY = yBegin + GammaOverlayMargin;
                else if (mouseY + GammaOverlayMargin > yEnd)
                    mouseY = yEnd - GammaOverlayMargin;

                g.FillRectangle(Brushes.Black, 0, 0, _screenWidth, mouseY - GammaOverlayMargin);
                g.FillRectangle(Brushes.Black, 0, mouseY + GammaOverlayMargin, _screenWidth,
                    _screenHeight - (mouseY + GammaOverlayMargin));
                g.FillRectangle(Brushes.Black, 0, 0, x1, _screenHeight);
                g.FillRectangle(Brushes.Black, x2End + gammaValueTextSize.Width, 0,
                    _screenWidth - (x2End + gammaValueTextSize.Width),
                    _screenHeight);
            }
        }
    }
}