using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong
{
    public class MathHelper
    {
        /// <returns>theta, r</returns>
        public static Tuple<double, double> ToPol(float xr, float yr)
        {
            double theta = Math.Atan(yr / xr);
            double r = Math.Sqrt(Math.Pow(xr, 2) + Math.Pow(yr, 2));

            if (xr < 0)
            {
                theta += Math.PI;
            }
            else if (yr < 0 && xr > 0)
            {
                theta += Math.PI*2;
            }

            return new Tuple<double, double>(theta, r);
        }

        public static Vector2 ToRec(double theta, double r)
        {
            return new Vector2((float)(r * Math.Cos(theta)), (float)(r * Math.Sin(theta)));
        }

        public static Vector2 ToRec(Vector2 origin, double theta, double r)
        {
            return Add(origin,new Vector2((float)(r * Math.Cos(theta)), (float)(r * Math.Sin(theta))));
        }

        #region vector operations
        public static Vector2 Add(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2 Mult(Vector2 vec, float x)
        {
            return new Vector2(vec.X * x, vec.Y * x);
        }

        public static Vector2 Mult(Vector2 left, Vector2 right)
        {
            return new Vector2(left.X * right.X, left.Y * right.Y);
        }
        #endregion
    }
}
