using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsteroidsClone
{
    public class Bullet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 direction;// { get; set; }

        public Bullet(int x,int y,Vector2 dir)
        {
            X = x;
            Y = y;
            direction.X = dir.X;
            direction.Y = -dir.Y;
        }

        public void Draw(RenderTarget D2DRT,SolidColorBrush scb)
        {
            D2DRT.DrawLine(new Vector2(X,Y),new Vector2(X+direction.X*10,(Y+direction.Y*10)),scb);
        }

        public void Update()
        {
            X += (int)(direction.X * 15f);
            Y += (int)(direction.Y * 15f);
        }
    }
}
