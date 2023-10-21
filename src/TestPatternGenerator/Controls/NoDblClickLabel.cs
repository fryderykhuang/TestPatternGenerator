namespace TestPatternGenerator.Controls;

public class NoDblClickLabel : Label
{
    protected override void WndProc(ref Message m)
    {
        // Change WM_LBUTTONDBLCLK to WM_LBUTTONCLICK
        if (m.Msg == 0x203) m.Msg = 0x201;
        else if (m.Msg == 0x206) m.Msg = 0x204;
        base.WndProc(ref m);
    }
}