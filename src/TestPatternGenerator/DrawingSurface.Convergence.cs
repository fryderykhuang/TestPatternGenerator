namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private readonly Pen _convergenceBluePen = new(Color.Blue);
    private readonly Pen _convergenceGreenPen = new(Color.Green);
    private readonly Pen _convergenceGridPen = new(Color.White);
    private readonly Pen[] _convergenceRbgPen;
    private readonly Pen _convergenceRedPen = new(Color.Red);
    private ConvergencePageSettings _convergenceSettings = new();
    private int _gridSizeCurrentIndex;
    private int _gridSizeDefaultIndex;
    private SortedSet<int> _gridSizes = null!;

    private int GetGridSize()
    {
        if (_gridSizes.Count == 0)
            return _geometryGridSizeFallback;
        return _gridSizeCurrentIndex >= 0 && _gridSizeCurrentIndex < _gridSizes.Count
            ? _gridSizes.ElementAt(_gridSizeCurrentIndex)
            : _gridSizes.ElementAt(_gridSizeDefaultIndex);
    }

    private void SwitchToOtherGridSize(int offset)
    {
        var req = _gridSizeCurrentIndex + offset;
        if (req < 0)
            req = 0;
        else if (req >= _gridSizes.Count)
            req = _gridSizes.Count - 1;
        _gridSizeCurrentIndex = req;

        Invalidate();
    }

    private void OnConvergenceSettingsChanged(ConvergencePageSettings? obj)
    {
        if (obj == null)
            return;

        _convergenceSettings = obj;

        UpdateDrawingFunction();

        foreach (var pen in _convergenceRbgPen) pen.Width = obj.StrokeThickness;

        _convergenceGridPen.Width = obj.StrokeThickness;

        var color = 0xFF000000;
        if (obj.RedEnabled)
            color |= 0x00FF0000;
        if (obj.GreenEnabled)
            color |= 0x0000FF00;
        if (obj.BlueEnabled)
            color |= 0x000000FF;

        _convergenceGridPen.Color = Color.FromArgb((int) color);

        Invalidate();
    }

    private void DrawCrosshairConvergencePattern(Graphics g)
    {
        var gs = GetGridSize();
        var gsh = (int) Math.Round(gs / 2.0);

        for (int y = 0, k = 0, cr = _convergenceSettings.ColorOffset; y < _screenHeight; y += gs, k++)
        {
            var lastCr = cr;
            cr += k % 3 == 0 ? 0 : 1;
            for (int x = -gsh, c = lastCr, cc = 0;
                 x < _screenWidth + gsh;
                 cc++)
            {
                c += (cc + k) % 3 == 0 ? 0 : 1;
                g.DrawLine(_convergenceRbgPen[(c % 3 + 3) % 3], x, y, x += gs, y);
            }
        }

        for (int x = 0, k = 0, cr = _convergenceSettings.ColorOffset; x < _screenWidth; x += gs, k++)
        {
            var lastCr = cr;
            cr += k % 3 == 0 ? 0 : 1;
            for (int y = -gsh, c = lastCr, cc = 0;
                 y < _screenHeight + gsh;
                 cc++)
            {
                c += (cc + k) % 3 == 0 ? 0 : 1;
                g.DrawLine(_convergenceRbgPen[(c % 3 + 3) % 3], x, y, x, y += gs);
            }
        }
    }

    private void DrawGridConvergencePattern(Graphics g)
    {
        var gs = GetGridSize();
        g.DrawRectangle(_convergenceGridPen, 0, 0, _screenWidth - 1, _screenHeight - 1);
        for (var x = gs; x <= _screenWidth - gs; x += gs)
        {
            g.DrawLine(_convergenceGridPen, x, 0, x, _screenHeight - 1);
            g.DrawLine(_convergenceGridPen, 0, x, _screenWidth - 1, x);
        }
    }
}