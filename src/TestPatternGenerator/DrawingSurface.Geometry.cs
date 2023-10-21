namespace TestPatternGenerator;

public partial class DrawingSurface
{
    private readonly Brush _geometryBgBrush = new SolidBrush(Color.Black);

    // private readonly Pen _geometryBgPen = new(Color.FromArgb(0xff, 0xd0, 0xd0, 0xd0));
    private readonly Pen _geometryPen = new(Color.White);
    private int _geometryCircleDiameter;
    private int _geometryCircleX;
    private int _geometryCircleY;
    private int _geometryCornerCircleDiameter;

    private int _geometryCornerCircleMargin;
    private int _geometryCornerCircleOrigin;
    private int _geometryGridSizeFallback;

    private int _geometrySmallCircleDiameter;
    private int _geometrySmallCircleX;
    private int _geometrySmallCircleY;

    private void DrawGeometryPattern(Graphics g)
    {
        // for (var x = _geometryGridSize; x <= _screenWidth - _geometryGridSize; x += _geometryGridSize)
        // {
        //     e.Graphics.DrawLine(_geometryBgPen, x, 0, x, _screenHeight);
        //     e.Graphics.DrawLine(_geometryBgPen, 0, x, _screenWidth, x);
        // }

        g.DrawRectangle(_geometryPen, 0, 0, _screenWidth - 1, _screenHeight - 1);
        g.DrawLine(_geometryPen, _geometryCornerCircleOrigin, 0, _geometryCornerCircleOrigin, _screenHeight);
        g.DrawLine(_geometryPen, 0, _geometryCornerCircleOrigin, _screenWidth, _geometryCornerCircleOrigin);
        g.DrawLine(_geometryPen, _screenWidth - _geometryCornerCircleOrigin, 0,
            _screenWidth - _geometryCornerCircleOrigin, _screenHeight);
        g.DrawLine(_geometryPen, 0, _screenHeight - _geometryCornerCircleOrigin, _screenWidth,
            _screenHeight - _geometryCornerCircleOrigin);

        g.FillEllipse(_geometryBgBrush, _geometryCircleX, _geometryCircleY, _geometryCircleDiameter,
            _geometryCircleDiameter);
        g.DrawLine(_geometryPen, _screenHalfWidth, 0, _screenHalfWidth, _screenHeight);
        g.DrawLine(_geometryPen, 0, _screenHalfHeight, _screenWidth, _screenHalfHeight);
        g.DrawEllipse(_geometryPen, _geometryCircleX, _geometryCircleY, _geometryCircleDiameter,
            _geometryCircleDiameter);
        g.DrawEllipse(_geometryPen, _geometrySmallCircleX, _geometrySmallCircleY, _geometrySmallCircleDiameter,
            _geometrySmallCircleDiameter);

        g.DrawEllipse(_geometryPen, _geometryCornerCircleMargin, _geometryCornerCircleMargin,
            _geometryCornerCircleDiameter, _geometryCornerCircleDiameter);
        g.DrawEllipse(_geometryPen, _screenWidth - _geometryCornerCircleMargin - _geometryCornerCircleDiameter,
            _geometryCornerCircleMargin, _geometryCornerCircleDiameter, _geometryCornerCircleDiameter);
        g.DrawEllipse(_geometryPen, _geometryCornerCircleMargin,
            _screenHeight - _geometryCornerCircleMargin - _geometryCornerCircleDiameter, _geometryCornerCircleDiameter,
            _geometryCornerCircleDiameter);
        g.DrawEllipse(_geometryPen, _screenWidth - _geometryCornerCircleMargin - _geometryCornerCircleDiameter,
            _screenHeight - _geometryCornerCircleMargin - _geometryCornerCircleDiameter, _geometryCornerCircleDiameter,
            _geometryCornerCircleDiameter);
    }
}