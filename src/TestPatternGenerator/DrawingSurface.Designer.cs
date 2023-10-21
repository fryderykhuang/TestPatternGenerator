

namespace TestPatternGenerator;

sealed partial class DrawingSurface
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        errorLabel = new Label();
        SuspendLayout();
        // 
        // errorLabel
        // 
        errorLabel.AutoSize = true;
        errorLabel.BackColor = Color.Transparent;
        errorLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        errorLabel.ForeColor = Color.Red;
        errorLabel.Location = new Point(19, 19);
        errorLabel.Margin = new Padding(10);
        errorLabel.Name = "errorLabel";
        errorLabel.Size = new Size(0, 28);
        errorLabel.TabIndex = 0;
        // 
        // PatternSurface
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(800, 450);
        Controls.Add(errorLabel);
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.None;
        Name = "DrawingSurface";
        Text = "Form1";
        MouseDown += DrawingSurface_MouseDown;
        MouseMove += DrawingSurface_MouseMove;
        MouseUp += DrawingSurface_MouseUp;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label errorLabel;
}