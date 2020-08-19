using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsteroidsClone
{
    public class Ship
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 velocity; //{ get; set; }
        public float rotationSpeed { get; set; }
        public Vector2 direction;

        public Ship(int x,int y)
        {
            X = x;
            Y = y;
            velocity = new Vector2(0.0f);
            rotationSpeed = 0.0f;
            direction = new Vector2(0.0f, 1.0f);
        }

        public void DrawShip(RenderTarget D2DRT,SolidColorBrush scb,Factory d2dFactory)
        {
            PathGeometry shape = new PathGeometry(d2dFactory);
            GeometrySink sink = shape.Open();
            //Ship Front
            sink.BeginFigure(new Vector2((float)X + direction.X * 20, (float)Y - direction.Y * 20), FigureBegin.Hollow);

            //Calculating Ship Right Wing
            float rwX= (float)(direction.X * 0 - direction.Y * -1);
            float rwY = (float)(direction.X * 1 + direction.Y * 0);

            sink.AddLine(new Vector2(X +  rwX*7, Y + rwY*7));
            //sink.AddLine(new Vector2(X + 7, Y));

            float lwX = (float)(direction.X * 0 - direction.Y * 1);
            float lwY = (float)(direction.X * -1 + direction.Y * 0);


            sink.AddLine(new Vector2(X + 7*lwX, Y-rwY*7));

            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
            D2DRT.DrawGeometry(shape, scb, 1f);
            //D2DRT.DrawLine(new Vector2(X, Y),new Vector2( X + direction.X * 20, Y - direction.Y * 20),scb);
        }

        public void UpdateRotation()
        {
            float rX = (float)(direction.X * Math.Cos(rotationSpeed) - direction.Y * Math.Sin(rotationSpeed));
            float rY = (float)(direction.X * Math.Sin(rotationSpeed) + direction.Y * Math.Cos(rotationSpeed));
            direction.X = rX;
            direction.Y = rY;

            rotationSpeed = 0.0f;
        }
        public void UpdateSpeed()
        {
            X += (int)(velocity.X);
            Y -= (int)(velocity.Y);
        }
    }
}
