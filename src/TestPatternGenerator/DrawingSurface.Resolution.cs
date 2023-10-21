namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private readonly Pen _resolutionPen1 = new(Color.Black);
    private readonly Pen _resolutionPen2 = new(Color.White);
    private Brush _resolutionBrush1 = new SolidBrush(Color.Black);
    private Brush _resolutionBrush2 = new SolidBrush(Color.White);
    private ResolutionPageSettings _resolutionSettings = new();

    private void OnResolutionPageSettingsChanged(ResolutionPageSettings? options)
    {
        if (options == null)
            return;
        _resolutionSettings = options;
        _resolutionPen1.Color = _resolutionSettings.Color1;
        _resolutionPen2.Color = _resolutionSettings.Color2;
        _resolutionBrush1 = new SolidBrush(_resolutionSettings.Color1);
        _resolutionBrush2 = new SolidBrush(_resolutionSettings.Color2);
        UpdateDrawingFunction();
        Invalidate();
    }

    private void DrawHorizontalResolutionPattern(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s + c2s <= 0)
            return;

        for (var i = 0; i < _screenWidth;)
        {
            for (var j = 0; j < c1s; ++j, i++)
                g.DrawLine(_resolutionPen1, i, 0, i, _screenHeight);

            for (var j = 0; j < c2s; ++j, i++)
                g.DrawLine(_resolutionPen2, i, 0, i, _screenHeight);
        }
    }

    private void DrawVerticalResolutionPattern(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s + c2s <= 0)
            return;

        for (var i = 0; i < _screenHeight;)
        {
            for (var j = 0; j < c1s; ++j, i++)
                g.DrawLine(_resolutionPen1, 0, i, _screenWidth, i);
            for (var j = 0; j < c2s; ++j, i++)
                g.DrawLine(_resolutionPen2, 0, i, _screenWidth, i);
        }
    }

    private void DrawHorizontalGrowingSizeResolutionPattern(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s + c2s <= 0)
            return;

        if (_resolutionSettings.PatchSize <= 0)
            return;

        for (var i = 0; i < _screenWidth;)
        {
            var maxI = i + _resolutionSettings.PatchSize;
            do
            {
                for (var j = 0; j < c1s && i < maxI; ++j, i++)
                    g.DrawLine(_resolutionPen1, i, 0, i, _screenHeight);
                for (var j = 0; j < c2s && i < maxI; ++j, i++)
                    g.DrawLine(_resolutionPen2, i, 0, i, _screenHeight);
            } while (i < maxI);

            c1s += _resolutionSettings.Color1Size;
            c2s += _resolutionSettings.Color2Size;
        }
    }

    private void DrawVerticalGrowingSizeResolutionPattern(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s + c2s <= 0)
            return;

        if (_resolutionSettings.PatchSize <= 0)
            return;

        for (var i = 0; i < _screenHeight;)
        {
            var maxI = i + _resolutionSettings.PatchSize;
            do
            {
                for (var j = 0; j < c1s && i < maxI; ++j, i++)
                    g.DrawLine(_resolutionPen1, 0, i, _screenWidth, i);
                for (var j = 0; j < c2s && i < maxI; ++j, i++)
                    g.DrawLine(_resolutionPen2, 0, i, _screenWidth, i);
            } while (i < maxI);

            c1s += _resolutionSettings.Color1Size;
            c2s += _resolutionSettings.Color2Size;
        }
    }

    private void DrawDotResolutionPattern1(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s <= 0 && c2s <= 0)
            return;
        if (c1s <= 0)
        {
            g.FillRectangle(_resolutionBrush2, 0, 0, _screenWidth, _screenHeight);
            return;
        }

        if (c2s <= 0)
        {
            g.FillRectangle(_resolutionBrush1, 0, 0, _screenWidth, _screenHeight);
            return;
        }

        var rowHeight = Math.Max(c1s, c2s);

        for (int y = 0, k = 0; y < _screenHeight; y += rowHeight, ++k)
            if (k % 2 == 0)
                for (var x = 0; x < _screenWidth;)
                {
                    g.FillRectangle(_resolutionBrush1, x, y, c1s, c1s);
                    x += c1s;
                    g.FillRectangle(_resolutionBrush2, x, y, c2s, c2s);
                    x += c2s;
                }
            else
                for (var x = 0; x < _screenWidth;)
                {
                    g.FillRectangle(_resolutionBrush2, x, y, c2s, c2s);
                    x += c2s;
                    g.FillRectangle(_resolutionBrush1, x, y, c1s, c1s);
                    x += c1s;
                }
    }

    private void DrawDotResolutionPattern2(Graphics g)
    {
        var c1s = _resolutionSettings.Color1Size;
        var c2s = _resolutionSettings.Color2Size;
        if (c1s <= 0 && c2s <= 0)
            return;
        if (c1s <= 0)
        {
            g.FillRectangle(_resolutionBrush2, 0, 0, _screenWidth, _screenHeight);
            return;
        }

        if (c2s <= 0)
        {
            g.FillRectangle(_resolutionBrush1, 0, 0, _screenWidth, _screenHeight);
            return;
        }

        var sizes = new[] {c1s, c2s};
        var brushes = new[] {_resolutionBrush1, _resolutionBrush2};


        int y = 0, x = 0;
        bool dY = true, dX = true;
        var state = DiagonalZigzagState.HitTop;
        for (var i = 0;; ++i)
        {
            var size = sizes[i % 2];
            var brush = brushes[i % 2];
            g.FillRectangle(brush, x, y, size, size);

            switch (state)
            {
                case DiagonalZigzagState.HitBottom:
                    x += size;
                    if (x >= _screenWidth)
                        return;
                    state = DiagonalZigzagState.GoRt;
                    break;
                case DiagonalZigzagState.HitTop:
                    x += size;
                    state = DiagonalZigzagState.GoLb;
                    break;
                case DiagonalZigzagState.HitRight:
                    y += size;
                    if (y >= _screenHeight)
                        return;
                    state = DiagonalZigzagState.GoLb;
                    break;
                case DiagonalZigzagState.HitLeft:
                    y += size;
                    state = DiagonalZigzagState.GoRt;
                    break;
                case DiagonalZigzagState.GoLb:
                    y += size;
                    x -= size;
                    if (x <= 0)
                        state = DiagonalZigzagState.HitLeft;
                    else if (y >= _screenHeight)
                        state = DiagonalZigzagState.HitBottom;
                    break;
                case DiagonalZigzagState.GoRt:
                    y -= size;
                    x += size;
                    if (y <= 0)
                        state = DiagonalZigzagState.HitTop;
                    else if (x >= _screenWidth)
                        state = DiagonalZigzagState.HitRight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private enum DiagonalZigzagState
    {
        HitBottom,
        HitTop,
        HitRight,
        HitLeft,
        GoLb,
        GoRt
    }
}