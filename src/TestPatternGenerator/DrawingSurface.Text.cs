namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private static readonly string[] TextPatterns =
        {"CRT monitor is amazing", "CRT显示器牛逼", "CRTモニターってすごいですね", "CRT 모니터는 정말 대단해요"};

    private void DrawTextPattern(Graphics g)
    {
        var sizes = TextPatterns.Select(x => g.MeasureString(x, _textFont)).ToList();
        var w = (int) Math.Ceiling(sizes.Select(x => x.Width).Max());
        var h = (int) Math.Ceiling(sizes.Select(x => x.Height).Max());


        var i = 0;
        for (int y = 0, k = 0; y < _screenHeight; y += h, k++)
        {
            var f = k % 4 < 2;
            for (var x = 0; x < _screenWidth; x += w)
            {
                var str = TextPatterns[(i++ + k) % TextPatterns.Length];
                // var sz = g.MeasureString(str, _textFont);
                // w = (int) Math.Ceiling(sz.Width);
                // h = (int) Math.Ceiling(sz.Height);
                g.FillRectangle(!f ? Brushes.White : Brushes.Black, x, y, w, h);
                g.DrawString(str, _textFont, f ? Brushes.White : Brushes.Black, x, y);
                f = !f;
            }
        }
    }
}