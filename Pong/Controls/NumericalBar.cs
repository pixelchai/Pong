using Microsoft.Xna.Framework.Input;
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
   public  class NumericalBar : ProgressBar
    {
        private bool _isPointerDown = false;

        public new event EventHandler ProgressChanged;
        public event EventHandler ValueChanged;

        private float _progress = 1.0f;
        public new float Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    base.Progress = value;
                    ProgressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public string Suffix = String.Empty;
        public int DecimalPlaces = 0;
        public float LBound = 0f;
        public float UBound = 100f;
        public float Value
        {
            get
            {
                return Progress * (Math.Abs(UBound - LBound)) + LBound;
            }
            set
            {
                Progress = (value - LBound) / Math.Abs(UBound - LBound);
            }
        }


        public NumericalBar() : base()
        {
        }

        public override bool OnPointerDown(IGuiContext context, PointerEventArgs args)
        {
            if (IsEnabled)
            {
                _isPointerDown = true;
            }
            return base.OnPointerDown(context, args);
        }

        public override void Update(IGuiContext context, float deltaSeconds)
        {
            if (IsEnabled)
            {
                if (_isPointerDown)
                {
                    int x = Mouse.GetState().X - ContentRectangle.X;
                    if (x > ContentRectangle.Width)
                    {
                        x = ContentRectangle.Width;
                    }
                    else if (x < 0)
                    {
                        x = 0;
                    }
                    //dragging
                    this.Progress = x / (float)ContentRectangle.Width;

                    if(Mouse.GetState().LeftButton == ButtonState.Released)
                    {
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        _isPointerDown = false;
                    }
                }
            }
            base.Update(context, deltaSeconds);
        }

        public override void Draw(IGuiContext context, IGuiRenderer renderer, float deltaSeconds)
        {
            base.Draw(context, renderer, deltaSeconds);

            var text = Math.Round(Value,DecimalPlaces).ToString()+Suffix;
            var textInfo = GetTextInfo(context, text, ContentRectangle, HorizontalTextAlignment, VerticalTextAlignment);

            if (!string.IsNullOrWhiteSpace(textInfo.Text))
                renderer.DrawText(textInfo.Font, textInfo.Text, textInfo.Position + TextOffset, textInfo.Color, textInfo.ClippingRectangle);

        }
    }
}
