using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace TestPatternGenerator;

public sealed partial class DrawingSurface : Form
{
    private static readonly Matrix _matHFlip = new(-1, 0, 0, 1, 0, 0);
    private static readonly Matrix _matVFlip = new(1, 0, 0, -1, 0, 0);
    private static readonly Matrix _matHScaleUp5P;
    private static readonly Matrix _matVScaleUp5P;
    private static readonly Matrix _matHScaleDown5P;
    private static readonly Matrix _matVScaleDown5P;
    private static readonly Matrix _matRotate90CW = new(0, 1, -1, 0, 0, 0);
    private static readonly Matrix _matRotate90CCW = new(0, -1, 1, 0, 0, 0);

    private readonly StringBuilder _errorTextSb = new();
    private readonly IEventBus _eventBus;

    private readonly Font _hintFont;
    private Action<Graphics> _drawFunc = delegate { };

    private PatternCategories _patternCategory;
    private PatternViewState _patternViewState = new();

    private int _screenBottomNarrow;
    private int _screenHalfHeight;
    private int _screenHalfWidth;
    private int _screenHeight;
    private int _screenLeftNarrow;
    private int _screenRightNarrow;
    private int _screenTopNarrow;

    // private Screen _screen = null!;
    private int _screenWidth;

    private Font _textFont;
    private Matrix _transformMatrix = new();

    static DrawingSurface()
    {
        _matHScaleUp5P = new Matrix();
        _matHScaleUp5P.Scale(1.05f, 1f);
        _matVScaleUp5P = new Matrix();
        _matVScaleUp5P.Scale(1f, 1.05f);
        _matHScaleDown5P = new Matrix();
        _matHScaleDown5P.Scale(.95f, 1f);
        _matVScaleDown5P = new Matrix();
        _matVScaleDown5P.Scale(1f, .95f);
    }

    public DrawingSurface(IEventBus eventBus, IOptionsMonitor<ResolutionPageSettings> resolutionSettings,
        IOptionsMonitor<WhiteBalancePageSettings?> whiteBalanceSettings,
        IOptionsMonitor<ConvergencePageSettings> convergenceSettings,
        IOptionsMonitor<SpectrumPageSettings?> spectrumSettings,
        IOptionsMonitor<GammaPageSettings?> gammaSettings,
        IOptionsMonitor<PatternViewState?> patternViewStateSettings)
    {
        _convergenceRbgPen = new[] {_convergenceRedPen, _convergenceBluePen, _convergenceGreenPen};
        _eventBus = eventBus;
        eventBus.EventRaised += EventBusOnEventRaised;
        SystemEvents.DisplaySettingsChanged += SystemEventsOnDisplaySettingsChanged;
        OnScreenChanged();
        InitializeComponent();
        _hintFont = new Font(Font.FontFamily, Font.Size, FontStyle.Bold, Font.Unit,
            Font.GdiCharSet, Font.GdiVerticalFont);
        _textFont = Font;
        resolutionSettings.OnChange(OnResolutionPageSettingsChanged);
        OnResolutionPageSettingsChanged(resolutionSettings.CurrentValue);
        whiteBalanceSettings.OnChange(OnWhiteBalancePageSettingsChanged);
        OnWhiteBalancePageSettingsChanged(whiteBalanceSettings.CurrentValue);
        convergenceSettings.OnChange(OnConvergenceSettingsChanged);
        OnConvergenceSettingsChanged(convergenceSettings.CurrentValue);
        spectrumSettings.OnChange(OnSpectrumSettingsChanged);
        OnSpectrumSettingsChanged(spectrumSettings.CurrentValue);
        gammaSettings.OnChange(OnGammaSettingsChanged);
        OnGammaSettingsChanged(gammaSettings.CurrentValue);
        patternViewStateSettings.OnChange(OnPatternViewStateChanged);
        OnPatternViewStateChanged(patternViewStateSettings.CurrentValue);
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        Bounds = Screen.FromControl(this).Bounds;
    }

    private void EventBusOnEventRaised(object? sender, ApplicationEventArgs e)
    {
        switch (e.Event)
        {
            case ApplicationEvents.DecreaseGridSize:
                SwitchToOtherGridSize(-1);
                break;
            case ApplicationEvents.IncreaseGridSize:
                SwitchToOtherGridSize(1);
                break;
            case ApplicationEvents.DecreaseHue:
                _currentHue = (_currentHue % 360 + 360 + 1) % 360;
                Invalidate();
                break;
            case ApplicationEvents.IncreaseHue:
                _currentHue = (_currentHue % 360 + 360 - 1) % 360;
                Invalidate();
                break;
            case ApplicationEvents.DecreaseHueStep:
                var req = _hueBlockSize - 1;
                _hueBlockSize = req <= 0 ? 1 : req;
                Invalidate();
                break;
            case ApplicationEvents.IncreaseHueStep:
                req = _hueBlockSize + 1;
                _hueBlockSize = req > Math.Max(_screenWidth, _screenHeight) ? _hueBlockSize : req;
                Invalidate();
                break;
            case ApplicationEvents.DecreaseTextFontSize:
                req = (int) Math.Round(_textFont.Size - 1);
                if (req < 5) return;
                _textFont = new Font(_textFont.FontFamily, req, _textFont.Style, _textFont.Unit, _textFont.GdiCharSet,
                    _textFont.GdiVerticalFont);
                Invalidate();
                break;
            case ApplicationEvents.IncreaseTextFontSize:
                req = (int) Math.Round(_textFont.Size + 1);
                if (req > 120) return;
                _textFont = new Font(_textFont.FontFamily, req, _textFont.Style, _textFont.Unit, _textFont.GdiCharSet,
                    _textFont.GdiVerticalFont);
                Invalidate();
                break;
        }
    }

    private void OnPatternViewStateChanged(PatternViewState? obj)
    {
        if (obj == null)
            return;

        _patternViewState = obj;

        _patternCategory = obj.ActivePatternCategory;

        var halfWidth = (float) (_screenWidth / 2.0);
        var halfHeight = (float) (_screenHeight / 2.0);
        var m = new Matrix(1, 0, 0, 1, halfWidth, halfHeight);

        if (obj.UseCustomTransformMatrix)
        {
            if (obj.TransformMatrix != null)
                m.Multiply(obj.TransformMatrix);
        }
        else
        {
            if (obj.PredefinedTransforms != null)
                foreach (var trans in obj.PredefinedTransforms)
                    switch (trans)
                    {
                        case PredefinedTransforms.HorizontalFlip:
                            m.Multiply(_matHFlip);
                            break;
                        case PredefinedTransforms.VerticalFlip:
                            m.Multiply(_matVFlip);
                            break;
                        case PredefinedTransforms.Rotate90CW:
                            m.Multiply(_matRotate90CW);
                            break;
                        case PredefinedTransforms.Rotate90CCW:
                            m.Multiply(_matRotate90CCW);
                            break;
                        case PredefinedTransforms.HorizontalScaleUp5Percent:
                            m.Multiply(_matHScaleUp5P);
                            break;
                        case PredefinedTransforms.VerticalScaleUp5Percent:
                            m.Multiply(_matVScaleUp5P);
                            break;
                        case PredefinedTransforms.HorizontalScaleDown5Percent:
                            m.Multiply(_matHScaleDown5P);
                            break;
                        case PredefinedTransforms.VerticalScaleDown5Percent:
                            m.Multiply(_matVScaleDown5P);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
        }

        m.Translate(-halfWidth, -halfHeight);
        _transformMatrix = m;

        UpdateDrawingFunction();
        Invalidate();
    }

    private void UpdateDrawingFunction()
    {
        switch (_patternCategory)
        {
            case PatternCategories.Resolution:
                switch (_resolutionSettings.Pattern)
                {
                    case ResolutionPatterns.HorizontalStripe:
                        _drawFunc = DrawHorizontalResolutionPattern;
                        break;
                    case ResolutionPatterns.VerticalStripe:
                        _drawFunc = DrawVerticalResolutionPattern;
                        break;
                    case ResolutionPatterns.HorizontalStripeSizeGrowing:
                        _drawFunc = DrawHorizontalGrowingSizeResolutionPattern;
                        break;
                    case ResolutionPatterns.VerticalStripeSizeGrowing:
                        _drawFunc = DrawVerticalGrowingSizeResolutionPattern;
                        break;
                    case ResolutionPatterns.Dot1:
                        _drawFunc = DrawDotResolutionPattern1;
                        break;
                    case ResolutionPatterns.Dot2:
                        _drawFunc = DrawDotResolutionPattern2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case PatternCategories.WhiteBalance:
                _drawFunc = DrawWhiteBalancePattern;
                break;
            case PatternCategories.Geometry:
                _drawFunc = DrawGeometryPattern;
                break;
            case PatternCategories.Convergence:
                switch (_convergenceSettings.Pattern)
                {
                    case ConvergencePatterns.Crosshair:
                        _drawFunc = DrawCrosshairConvergencePattern;
                        break;
                    case ConvergencePatterns.Grid:
                        _drawFunc = DrawGridConvergencePattern;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case PatternCategories.Spectrum:
                switch (_spectrumSettings.Pattern)
                {
                    case SpectrumPatterns.Horizontal:
                        _drawFunc = DrawHorizontalSpectrumPattern;
                        break;
                    case SpectrumPatterns.Pie:
                        _drawFunc = DrawPieSpectrumPattern;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            case PatternCategories.Text:
                _drawFunc = DrawTextPattern;
                break;
            case PatternCategories.Gamma:
                _drawFunc = DrawGammaPattern;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


    private void OnScreenChanged()
    {
        var hdc = GetDC(Handle);
        var DESKTOPVERTRES = 117;
        var DESKTOPHORZRES = 118;
        _screenWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
        _screenHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);
        ReleaseDC(Handle, hdc);

        _screenTopNarrow = (int) Math.Round(_screenHeight / 12d);
        _screenBottomNarrow = (int) Math.Round(_screenTopNarrow * 11d);
        _whiteBalancePatchHeight = (int) Math.Round(_screenHeight / 12d * 10d);
        _screenLeftNarrow = (int) Math.Round(_screenWidth / 12d);
        _screenRightNarrow = (int) Math.Round(_screenLeftNarrow * 11d);

        _screenHalfWidth = (int) Math.Round(_screenWidth / 2.0d);
        _screenHalfHeight = (int) Math.Round(_screenHeight / 2.0d);

        var geometryCircleDiameterByWidth = (int) Math.Round(_screenWidth / 5d * 4d);
        var geometryCircleDiameterByHeight = (int) Math.Round(_screenHeight / 7d * 6d);
        _geometryCircleDiameter = Math.Min(geometryCircleDiameterByWidth, geometryCircleDiameterByHeight);
        var geometryCircleRadius = (int) Math.Round(_geometryCircleDiameter / 2d);
        _geometrySmallCircleDiameter = (int) Math.Round(_geometryCircleDiameter / 2d);
        var geometrySmallCircleRadius = (int) Math.Round(_geometrySmallCircleDiameter / 2d);
        _geometryCircleX = _screenHalfWidth - geometryCircleRadius;
        _geometryCircleY = _screenHalfHeight - geometryCircleRadius;
        _geometrySmallCircleX = _screenHalfWidth - geometrySmallCircleRadius;
        _geometrySmallCircleY = _screenHalfHeight - geometrySmallCircleRadius;
        var geometryCornerCircleDiameterByWidth = (int) Math.Round(_screenWidth / 7d);
        var geometryCornerCircleDiameterByHeight = (int) Math.Round(_screenHeight / 4d);
        _geometryCornerCircleDiameter =
            Math.Min(geometryCornerCircleDiameterByWidth, geometryCornerCircleDiameterByHeight);
        _geometryCornerCircleMargin = (int) Math.Round(_geometryCornerCircleDiameter / 5d);
        _geometryCornerCircleOrigin =
            (int) Math.Round(_geometryCornerCircleDiameter / 2d) + _geometryCornerCircleMargin;
        var screenWidthHeightCommonDenominator = GetGreatestCommonFactor(_screenWidth, _screenHeight);
        var geometryGridMin = (int) Math.Round(_geometryCornerCircleDiameter / 9d);
        var geometryGridMax = (int) Math.Round(_geometryCornerCircleDiameter * 2d);
        var lst = new SortedSet<int>();
        for (var n = screenWidthHeightCommonDenominator;
             n >= geometryGridMin;
             n = (int) Math.Round(n / 2d))
            if (n <= geometryGridMax)
                lst.Add(n);

        _gridSizes = lst;

        for (var n = screenWidthHeightCommonDenominator;
             n <= geometryGridMax;
             n = (int) Math.Round(n * 2d))
            if (n >= geometryGridMin)
                lst.Add(n);

        _gridSizeDefaultIndex = (int) Math.Round(lst.Count / 2d);
        _gridSizeCurrentIndex = _gridSizeDefaultIndex;

        if (lst.Count == 0)
            _geometryGridSizeFallback = _geometryCornerCircleDiameter / 2;
        OnPatternViewStateChanged(_patternViewState);

        Invalidate();
    }

    private void SystemEventsOnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.ReopenDrawingSurface);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        ClearErrorText();
        var g = e.Graphics;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.None;

        try
        {
            g.Transform = _transformMatrix;
        }
        catch (Exception ex)
        {
            AppendErrorText($"Transform matrix is invalid. ({ex.Message})");
        }

        _drawFunc.Invoke(g);
    }

    private void AppendErrorText(string message)
    {
        _errorTextSb.AppendLine(message);
        errorLabel.Text = _errorTextSb.ToString();
    }

    private void ClearErrorText()
    {
        _errorTextSb.Clear();
        errorLabel.Text = string.Empty;
    }

    private static int GetGreatestCommonFactor(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    private void DrawingSurface_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;


        if (_patternCategory == PatternCategories.Gamma && _gammaRegion.Contains(e.Location))
        {
            _gammaOverlayMouseLocation = e.Location;
            _drawGammaOverlay = true;
            Invalidate();
        }
        else
        {
            _drawGammaOverlay = false;
            _eventBus.SendEvent(ApplicationEvents.ToggleMainFormVisibility);
        }
    }

    private void DrawingSurface_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        _drawGammaOverlay = false;
        Invalidate();
    }

    private void DrawingSurface_MouseMove(object sender, MouseEventArgs e)
    {
        if (_drawGammaOverlay && _patternCategory == PatternCategories.Gamma)
        {
            _gammaOverlayMouseLocation = e.Location;
            Invalidate();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        _eventBus.SendEvent(e.Delta > 0
            ? ApplicationEvents.GoToPreviousPatternCategory
            : ApplicationEvents.GoToNextPatternCategory);
    }
}