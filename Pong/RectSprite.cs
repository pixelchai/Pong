using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    public class RectSprite
    {
        private Texture2D plain;

        public float X1;
        public float Y1;
        public float X2;
        public float Y2;

        public RectSprite(GraphicsDevice gd,Vector2 v1, Vector2 v2)
        {
            plain = new Texture2D(gd, 1, 1);
            plain.SetData(new[] { Color.White });

            X1 = v1.X;
            Y1 = v1.Y;
            X2 = v2.X;
            Y2 = v2.Y;
        }

        public RectSprite(GraphicsDevice gd, float x1, float y1, float x2, float y2)
        {
            plain = new Texture2D(gd, 1, 1);
            plain.SetData(new[] { Color.White });

            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public void Draw(SpriteBatch spriteBatch, Color color, float width)
        {
            float Xr = X2 - X1;
            float Yr = Y2 - Y1;

            Tuple<double, double> pol = MathHelper.ToPol(Xr, Yr);

            spriteBatch.Draw(plain, new Rectangle((int)Math.Round(X1,MidpointRounding.AwayFromZero), (int)Math.Round(Y1, MidpointRounding.AwayFromZero), (int)Math.Round(pol.Item2, MidpointRounding.AwayFromZero), (int)Math.Round(width, MidpointRounding.AwayFromZero)),
                null,
                color,
                (float)pol.Item1,
                new Vector2(0, 0), SpriteEffects.None, 1);
        }

        

    }
}
