using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Pong
{
    public class RectSprite : IDisposable
    {
        private Texture2D plain = null;

        public float X1;
        public float Y1;
        public float X2;
        public float Y2;

        #region constructors
        public RectSprite(Texture2D plain, float x1, float y1, float x2, float y2)
        {
            this.plain = plain;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public RectSprite(Texture2D plain, Vector2 v1, Vector2 v2)
        {
            this.plain = plain;
            X1 = v1.X;
            Y1 = v1.Y;
            X2 = v2.X;
            Y2 = v2.Y;
        }

        public RectSprite(Vector2 v1, Vector2 v2)
        {
            X1 = v1.X;
            Y1 = v1.Y;
            X2 = v2.X;
            Y2 = v2.Y;
        }

        public RectSprite(float x1, float y1, float x2, float y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
        #endregion

        public void Draw(SpriteBatch spriteBatch, Color color, float width)
        {
            if (plain ==null) {
                plain = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                plain.SetData(new[] { Color.White });
            }
            spriteBatch.Disposing += SpriteBatch_Disposing;

            float Xr = X2 - X1;
            float Yr = Y2 - Y1;

            Tuple<double, double> pol = MathHelper.ToPol(Xr, Yr);

            spriteBatch.Draw(plain, new Rectangle((int)Math.Round(X1,MidpointRounding.AwayFromZero), (int)Math.Round(Y1, MidpointRounding.AwayFromZero), (int)Math.Round(pol.Item2, MidpointRounding.AwayFromZero), (int)Math.Round(width, MidpointRounding.AwayFromZero)),
                null,
                color,
                (float)pol.Item1,
                new Vector2(0, 0), SpriteEffects.None, 1);
        }

        private void SpriteBatch_Disposing(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            plain.Dispose();
        }
    }
}
