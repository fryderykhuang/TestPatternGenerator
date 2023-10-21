using System.ComponentModel;
using System.Drawing.Drawing2D;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TestPatternGenerator;

public sealed partial class MainForm : Form
{
    private readonly IEventBus _eventBus;
    private readonly DrawingSurface _drawingSurfaceForm;
    private readonly SettingsManager _settingsManager;
    private readonly IServiceProvider _sp;
    private ConvergencePageSettings _convergenceSettings = new();
    private GammaPageSettings _gammaSettings = new();
    private PatternViewState _patternViewState = new();
    private ResolutionPageSettings _resolutionSettings = new();
    private SpectrumPageSettings _spectrumSettings = new();
    private WhiteBalancePageSettings _whiteBalancePageSettings = new();
    private static readonly int PatternCategoryCount = Enum.GetValues<PatternCategories>().Select(x => (int)x).Max() + 1;


    public MainForm(IEventBus eventBus, IServiceProvider sp,
        IOptionsMonitor<PatternViewState?> patternViewStateSettings,
        IOptionsMonitor<WhiteBalancePageSettings?> whiteBalancePageSettings,
        IOptionsMonitor<ResolutionPageSettings> resolutionSettings,
        IOptionsMonitor<ConvergencePageSettings?> convergenceSettings,
        IOptionsMonitor<SpectrumPageSettings?> spectrumSettings,
        IOptionsMonitor<GammaPageSettings?> gammaSettings,
        SettingsManager settingsManager)
    {
        _eventBus = eventBus;
        _sp = sp;
        _drawingSurfaceForm = sp.GetRequiredService<DrawingSurface>();
        _drawingSurfaceForm.FormClosed += DrawingSurfaceFormOnFormClosed;
        _settingsManager = settingsManager;
        eventBus.EventRaised += EventBusOnEventRaised;
        // using (settingsManager.IgnoreChangesOn<PatternViewState>())
        // using (settingsManager.IgnoreChangesOn<ResolutionPageSettings>())
        // using (settingsManager.IgnoreChangesOn<WhiteBalancePageSettings>())
        // using (settingsManager.IgnoreChangesOn<ConvergencePageSettings>())
        // using (settingsManager.IgnoreChangesOn<SpectrumPageSettings>())
        // using (settingsManager.IgnoreChangesOn<GammaPageSettings>())
        // {
        InitializeComponent();
        comboBox1.DataSource = Enum.GetValues<ResolutionPatterns>()
            .Select(x => new Tuple<ResolutionPatterns, string>(x, x.ToString())).ToList();
        comboBox1.ValueMember = "Item1";
        comboBox1.DisplayMember = "Item2";

        comboBox2.DataSource = Enum.GetValues<WhiteBalancePatterns>()
            .Select(x => new Tuple<WhiteBalancePatterns, string>(x, x.ToString())).ToList();
        comboBox2.ValueMember = "Item1";
        comboBox2.DisplayMember = "Item2";

        comboBox3.DataSource = Enum.GetValues<SpectrumPatterns>()
            .Select(x => new Tuple<SpectrumPatterns, string>(x, x.ToString())).ToList();
        comboBox3.ValueMember = "Item1";
        comboBox3.DisplayMember = "Item2";

        comboBox4.DataSource = new Tuple<int, string>[] { new(1, "1"), new(2, "2"), new(3, "3") };
        comboBox4.ValueMember = "Item1";
        comboBox4.DisplayMember = "Item2";
        // }

        resolutionSettings.OnChange(OnResolutionSettingsChanged);
        OnResolutionSettingsChanged(resolutionSettings.CurrentValue);
        whiteBalancePageSettings.OnChange(OnWhiteBalanceSettingsChanged);
        OnWhiteBalanceSettingsChanged(whiteBalancePageSettings.CurrentValue);
        patternViewStateSettings.OnChange(OnPatternViewStateChanged);
        OnPatternViewStateChanged(patternViewStateSettings.CurrentValue);
        convergenceSettings.OnChange(OnConvergenceSettingsChanged);
        OnConvergenceSettingsChanged(convergenceSettings.CurrentValue);
        spectrumSettings.OnChange(OnSpectrumSettingsChanged);
        OnSpectrumSettingsChanged(spectrumSettings.CurrentValue);
        gammaSettings.OnChange(OnGammaSettingsChanged);
        OnGammaSettingsChanged(gammaSettings.CurrentValue);

        var screenSz = Screen.FromControl(this).Bounds.Size;
        Location = new Point(screenSz.Width - Width, screenSz.Height - Width);
    }

    private void DrawingSurfaceFormOnFormClosed(object? sender, FormClosedEventArgs e)
    {
        Close();
    }

    private void OnGammaSettingsChanged(GammaPageSettings? obj)
    {
        if (obj == null)
            return;

        _gammaSettings = obj;
        using (_settingsManager.IgnoreChangesOn<GammaPageSettings>())
        {
            checkBox5.Checked = obj.PortraitMode;
            comboBox4.SelectedValue = obj.StripeThickness;
        }
    }

    private void OnSpectrumSettingsChanged(SpectrumPageSettings? obj)
    {
        if (obj == null)
            return;

        _spectrumSettings = obj;
        using (_settingsManager.IgnoreChangesOn<SpectrumPageSettings>())
        {
            comboBox3.SelectedValue = obj.Pattern;
        }
    }

    private void OnConvergenceSettingsChanged(ConvergencePageSettings? obj)
    {
        if (obj == null)
            return;

        _convergenceSettings = obj;
        using (_settingsManager.IgnoreChangesOn<ConvergencePageSettings>())
        {
            tabControl3.SelectedIndex = (int)obj.Pattern;
            numericUpDown9.Value = obj.StrokeThickness;
            checkBox2.Checked = obj.RedEnabled;
            checkBox3.Checked = obj.GreenEnabled;
            checkBox4.Checked = obj.BlueEnabled;
        }
    }

    private void EventBusOnEventRaised(object? sender, ApplicationEventArgs e)
    {
        switch (e.Event)
        {
            case ApplicationEvents.ReopenDrawingSurface:
                // _patternSurfaceForm.Close();
                // _patternSurfaceForm = _sp.GetRequiredService<PatternSurface>();
                // _patternSurfaceForm.Show();
                Application.Restart();
                break;
            case ApplicationEvents.ToggleMainFormVisibility:
                if (Visible)
                    Hide();
                else
                    Show();
                break;
            case ApplicationEvents.GoToPreviousPatternCategory:
                _patternViewState.ActivePatternCategory =
                    (PatternCategories)((((int)_patternViewState.ActivePatternCategory - 1) % PatternCategoryCount + PatternCategoryCount) % PatternCategoryCount);
                _settingsManager.SetSettings(_patternViewState);
                break;
            case ApplicationEvents.GoToNextPatternCategory:
                _patternViewState.ActivePatternCategory =
                    (PatternCategories)((((int)_patternViewState.ActivePatternCategory + 1) % PatternCategoryCount + PatternCategoryCount) % PatternCategoryCount);
                _settingsManager.SetSettings(_patternViewState);
                break;
        }
    }

    private void OnWhiteBalanceSettingsChanged(WhiteBalancePageSettings? obj)
    {
        if (obj == null)
            return;

        _whiteBalancePageSettings = obj;
        using (_settingsManager.IgnoreChangesOn<WhiteBalancePageSettings>())
        {
            numericUpDown1.Value = (decimal)_whiteBalancePageSettings.PatchRatio;
            numericUpDown2.Value = _whiteBalancePageSettings.PatchCount;
            numericUpDown5.Value = _whiteBalancePageSettings.WhitePointColor2;
            numericUpDown6.Value = _whiteBalancePageSettings.WhitePointColor1;
            numericUpDown8.Value = _whiteBalancePageSettings.BlackPointColor2;
            numericUpDown7.Value = _whiteBalancePageSettings.BlackPointColor1;
            comboBox2.SelectedValue = _whiteBalancePageSettings.Pattern;
        }
    }

    private void OnPatternViewStateChanged(PatternViewState? obj)
    {
        if (obj == null)
            return;

        _patternViewState = obj;
        tabControl1.SelectedIndex = (int)_patternViewState.ActivePatternCategory;
        using (_settingsManager.IgnoreChangesOn<PatternViewState>())
        {
            checkBox1.Checked = obj.UseCustomTransformMatrix;
            if (obj.TransformMatrix != null)
            {
                matrix11.Value = (decimal)obj.TransformMatrix.Elements[0];
                matrix12.Value = (decimal)obj.TransformMatrix.Elements[1];
                matrix21.Value = (decimal)obj.TransformMatrix.Elements[2];
                matrix22.Value = (decimal)obj.TransformMatrix.Elements[3];
                matrix31.Value = (decimal)obj.TransformMatrix.Elements[4];
                matrix32.Value = (decimal)obj.TransformMatrix.Elements[5];
            }

            obj.PredefinedTransforms ??= new List<PredefinedTransforms>();
            if (obj.PredefinedTransforms is not IBindingList)
                obj.PredefinedTransforms = new BindingList<PredefinedTransforms>(obj.PredefinedTransforms);
            listBox1.DataSource = obj.PredefinedTransforms;
        }
    }

    private void OnResolutionSettingsChanged(ResolutionPageSettings? obj)
    {
        if (obj == null)
            return;

        _resolutionSettings = obj;
        using (_settingsManager.IgnoreChangesOn<ResolutionPageSettings>())
        {
            comboBox1.SelectedValue = _resolutionSettings.Pattern;
            resolution_colorButton1.BackColor = _resolutionSettings.Color1;
            resolution_colorButton2.BackColor = _resolutionSettings.Color2;
            resolution_numericUpDown1.Value = _resolutionSettings.Color1Size;
            resolution_numericUpDown2.Value = _resolutionSettings.Color2Size;
            numericUpDown3.Value = _resolutionSettings.PatchSize;
            numericUpDown4.Value = _resolutionSettings.Color2IncreasingInterval;
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _drawingSurfaceForm.Show();
    }


    private void button1_Click_1(object sender, EventArgs e)
    {
        var dlg = new ColorDialog();
        dlg.AllowFullOpen = true;
        dlg.Color = _resolutionSettings.Color1;

        if (dlg.ShowDialog() == DialogResult.OK)
            _resolutionSettings.Color1 = dlg.Color;

        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void resolution_colorButton2_Click(object sender, EventArgs e)
    {
        var dlg = new ColorDialog();
        dlg.AllowFullOpen = true;
        dlg.Color = _resolutionSettings.Color2;

        if (dlg.ShowDialog() == DialogResult.OK)
            _resolutionSettings.Color2 = dlg.Color;

        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void ResolutionNumericUpDown1ValueChanged(object sender, EventArgs e)
    {
        _resolutionSettings.Color1Size = (int)resolution_numericUpDown1.Value;
        if (checkBox6.Checked)
        {
            resolution_numericUpDown2.Value = (int)resolution_numericUpDown1.Value;
        }

        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void ResolutionNumericUpDown2ValueChanged(object sender, EventArgs e)
    {
        _resolutionSettings.Color2Size = (int)resolution_numericUpDown2.Value;
        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        _resolutionSettings.Pattern =
            (ResolutionPatterns)(comboBox1.SelectedValue ?? ResolutionPatterns.HorizontalStripe);
        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void numericUpDown3_ValueChanged(object sender, EventArgs e)
    {
        _resolutionSettings.PatchSize = (int)numericUpDown3.Value;
        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void numericUpDown4_ValueChanged(object sender, EventArgs e)
    {
        _resolutionSettings.Color2IncreasingInterval = (int)numericUpDown4.Value;
        _settingsManager.SetSettings(_resolutionSettings);
    }

    private void matrix11_ValueChanged(object sender, EventArgs e)
    {
        if (_settingsManager.IsWriteIgnoredOn<PatternViewState>())
            return;
        _patternViewState.TransformMatrix = new Matrix((float)matrix11.Value, (float)matrix12.Value,
            (float)matrix21.Value, (float)matrix22.Value, (float)matrix31.Value, (float)matrix32.Value);
        _settingsManager.SetSettings(_patternViewState);
    }

    private void MainForm_DoubleClick(object sender, EventArgs e)
    {
        if (groupBox2.Visible)
            groupBox2.Hide();
        else
            groupBox2.Show();
    }

    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.Pattern =
            (WhiteBalancePatterns)(comboBox2.SelectedValue ?? WhiteBalancePatterns.BlackPoint);
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void numericUpDown7_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.BlackPointColor1 = (int)numericUpDown7.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void numericUpDown8_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.BlackPointColor2 = (int)numericUpDown8.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void numericUpDown6_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.WhitePointColor1 = (int)numericUpDown6.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void numericUpDown5_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.WhitePointColor2 = (int)numericUpDown5.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        _patternViewState.ActivePatternCategory = (PatternCategories)tabControl1.SelectedIndex;
        _settingsManager.SetSettings(_patternViewState);
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.PatchCount = (int)numericUpDown2.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
        _whiteBalancePageSettings.PatchRatio = (float)numericUpDown1.Value;
        _settingsManager.SetSettings(_whiteBalancePageSettings);
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        _patternViewState.UseCustomTransformMatrix = checkBox1.Checked;
        _settingsManager.SetSettings(_patternViewState);
    }

    private void label15_MouseClick(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.HorizontalFlip);
    }

    private void TransformOperation(MouseEventArgs e, PredefinedTransforms transform)
    {
        if (e.Button == MouseButtons.Left)
            _patternViewState.PredefinedTransforms!.Add(transform);
        else if (e.Button == MouseButtons.Right)
            _patternViewState.PredefinedTransforms!.RemoveLast(transform);

        _settingsManager.SetSettings(_patternViewState);
    }

    private void label16_MouseClick(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.VerticalFlip);
    }

    private void label17_MouseClick(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.Rotate90CW);
    }

    private void label18_MouseClick(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.Rotate90CCW);
    }

    private void button1_Click_2(object sender, EventArgs e)
    {
        _patternViewState.PredefinedTransforms?.Clear();
        _settingsManager.SetSettings(_patternViewState);
    }

    private void noDblClickLabel1_Click(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.HorizontalScaleUp5Percent);
    }

    private void noDblClickLabel2_Click(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.VerticalScaleUp5Percent);
    }

    private void noDblClickLabel3_Click(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.HorizontalScaleDown5Percent);
    }

    private void noDblClickLabel4_Click(object sender, MouseEventArgs e)
    {
        TransformOperation(e, PredefinedTransforms.VerticalScaleDown5Percent);
    }

    private void button2_Click(object sender, EventArgs e)
    {
        _convergenceSettings.ColorOffset++;
        _convergenceSettings.ColorOffset %= 3;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        _convergenceSettings.ColorOffset--;
        _convergenceSettings.ColorOffset %= 3;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void numericUpDown9_ValueChanged(object sender, EventArgs e)
    {
        _convergenceSettings.StrokeThickness = (int)numericUpDown9.Value;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
    {
        _convergenceSettings.Pattern = (ConvergencePatterns)tabControl3.SelectedIndex;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        _convergenceSettings.RedEnabled = checkBox2.Checked;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        _convergenceSettings.GreenEnabled = checkBox3.Checked;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
    {
        _convergenceSettings.BlueEnabled = checkBox4.Checked;
        _settingsManager.SetSettings(_convergenceSettings);
    }

    private void button5_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.IncreaseGridSize);
    }

    private void button4_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.DecreaseGridSize);
    }

    private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
    {
        _spectrumSettings.Pattern = (SpectrumPatterns)(comboBox3.SelectedValue ?? SpectrumPatterns.Horizontal);
        _settingsManager.SetSettings(_spectrumSettings);
    }

    private void button7_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.DecreaseHue);
    }

    private void button6_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.IncreaseHue);
    }

    private void button9_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.DecreaseHueStep);
    }

    private void button8_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.IncreaseHueStep);
    }

    private void repeatButton1_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.IncreaseTextFontSize);
    }

    private void repeatButton2_Click(object sender, EventArgs e)
    {
        _eventBus.SendEvent(ApplicationEvents.DecreaseTextFontSize);
    }

    private void checkBox5_CheckedChanged(object sender, EventArgs e)
    {
        _gammaSettings.PortraitMode = checkBox5.Checked;
        _settingsManager.SetSettings(_gammaSettings);
    }

    private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
    {
        _gammaSettings.StripeThickness = (int)(comboBox4.SelectedValue ?? 1);
        _settingsManager.SetSettings(_gammaSettings);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) Close();

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void checkBox6_CheckedChanged(object sender, EventArgs e)
    {
        if (checkBox6.Checked)
        {
            resolution_numericUpDown2.Enabled = false;
            resolution_numericUpDown2.Value = resolution_numericUpDown1.Value;
        }
        else
        {
            resolution_numericUpDown2.Enabled = true;
        }
    }
}