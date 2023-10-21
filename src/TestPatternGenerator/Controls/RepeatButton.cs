using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace TestPatternGenerator.Controls;

/// <summary>
///     Repeat button WinForms control. Behaves exactly like standard WinForms button with additional
///     repeater functionality: when button is pressed and hold, after <c>InitialDelay</c> button starts
///     emitting <c>MouseUp</c> event with <c></c>
///     Source: https://www.codeproject.com/Articles/629644/Auto-repeat-Button-in-10-Minutes
/// </summary>
public class RepeatButton : Button
{
    /// <summary>
    ///     Constructor.
    /// </summary>
    public RepeatButton()
    {
        InitializeComponent();
        InitialDelay = 400;
        RepeatInterval = 50;
    }

    private void InitializeComponent()
    {
        _components = new Container();
        _timerRepeater = new Timer(_components);
        SuspendLayout();
        _timerRepeater.Tick += timerRepeater_Tick;
        ResumeLayout(false);
    }


    /// <summary>
    ///     Initiates timer, that issues <c>MouseUp</c> event every <c>RepeatIteral</c> milliseconds. For the first time
    ///     event is fires after <c>InitialDelay</c> milliseconds.
    /// </summary>
    /// <param name="mevent"></param>
    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        //Save arguments
        _mouseDownArgs = mevent;
        _timerRepeater.Enabled = false;
        timerRepeater_Tick(null, EventArgs.Empty);
    }


    /// <summary>
    ///     Repeat loop happens in thin event handler handler using the following logic:
    ///     If handler is called for the first time, it fires <c>MouseDown</c> event and waits <c>InitialDelay</c>
    ///     milliseconds till next iteration. Every next iteration is called with delay of <c>RepeatDelay</c>
    ///     milliseconds.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void timerRepeater_Tick(object sender, EventArgs e)
    {
        base.OnMouseDown(_mouseDownArgs);
        base.OnMouseUp(_mouseDownArgs);
        if (_timerRepeater.Enabled)
            _timerRepeater.Interval = RepeatInterval;
        else
            _timerRepeater.Interval = InitialDelay;

        _timerRepeater.Enabled = true;
    }


    /// <summary>
    ///     Disables timer and repetitions.
    /// </summary>
    /// <param name="mevent"></param>
    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _timerRepeater.Enabled = false;
    }

    /// <summary>
    ///     Disposes local resources (timer).
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_components != null) _components.Dispose();

                _timerRepeater.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }

    #region Private members

    private Timer _timerRepeater; //timer to measure repeat intervals wait.
    private IContainer _components; //Components collection of this control (timer)
    private bool _disposed; //flag used to prevent multiple disposing in Dispose method
    private MouseEventArgs _mouseDownArgs; //muse down arguments; used by timer when repeating events.

    #endregion

    #region Public properties

    /// <summary>
    ///     Initial delay. Time in milliseconds between button press and first repeat action.
    /// </summary>
    [DefaultValue(400)]
    [Category("Enhanced")]
    [Description("Initial delay. Time in milliseconds between button press and first repeat action.")]
    public int InitialDelay { set; get; }

    /// <summary>
    ///     Repeat Interval. Repeat between each repeat action while button is hold pressed.
    /// </summary>
    [DefaultValue(50)]
    [Category("Behavior")]
    [Description("Repeat Interval. Repeat between each repeat action while button is hold pressed.")]
    public int RepeatInterval { set; get; }

    #endregion
}