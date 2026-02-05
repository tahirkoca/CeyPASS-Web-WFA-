using System;
using System.Windows.Forms;

namespace CeyPASS.WFA.Forms
{
    /// <summary>
    /// Scrollbar'ları hiç göstermeyen FlowLayoutPanel (kaydırma fare tekerleği ile çalışır).
    /// </summary>
    public class NoScrollBarFlowLayoutPanel : FlowLayoutPanel
    {
        private const int WS_VSCROLL = 0x00200000;
        private const int WS_HSCROLL = 0x00100000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_VSCROLL;
                cp.Style &= ~WS_HSCROLL;
                return cp;
            }
        }
    }
}
