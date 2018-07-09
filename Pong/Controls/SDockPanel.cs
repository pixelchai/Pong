using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Controls
{
    public class SDockPanel : DockPanel
    {
        public event EventHandler OnPointerMoveEvent;
        public override bool OnPointerMove(IGuiContext context, PointerEventArgs args)
        {
            OnPointerMoveEvent?.Invoke(this, EventArgs.Empty);
            return base.OnPointerMove(context, args);
        }

    }
}
