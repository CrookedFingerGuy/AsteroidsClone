using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;

namespace AsteroidsClone
{
    public class Ship
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 velocity; //{ get; set; }
        public float rotationSpeed { get; set; }
        public Vector2 direction;
        public List<Vector2> edgePoints;

        public Ship(int x,int y)
        {
            X = x;
            Y = y;
            velocity = new Vector2(0.0f);

            //rotation speed is in radians
            rotationSpeed = 0.0f;
            //unit vector of the direction the ship is pointed
            direction = new Vector2(0.0f, 1.0f);
            edgePoints = new List<Vector2>();
        }

        public void DrawShip(RenderTarget D2DRT,SolidColorBrush scb,Factory d2dFactory)
        {

            PathGeometry shape = new PathGeometry(d2dFactory);
            GeometrySink sink = shape.Open();
            //Ship Front
            sink.BeginFigure(new Vector2((float)X + direction.X * 20, (float)Y - direction.Y * 20), FigureBegin.Hollow);

            //Calculating Ship Right Wing by rotating unit vector 90 degrees
            float rwX= (float)(direction.X * 0 - direction.Y * -1);
            float rwY = (float)(direction.X * 1 + direction.Y * 0);
            sink.AddLine(new Vector2(X +  rwX*7, Y + rwY*7));

            //Calculating Ship Left Wing by rotating unit vector 270 degrees
            float lwX = (float)(direction.X * 0 - direction.Y * 1);
            float lwY = (float)(direction.X * -1 + direction.Y * 0);
            sink.AddLine(new Vector2(X + lwX*7, Y-rwY*7));

            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
            D2DRT.DrawGeometry(shape, scb, 1f);

            //draw a line that is 10 times the length of the unit vector ship.direction
            D2DRT.DrawLine(new Vector2(X, Y),new Vector2( X + direction.X * 10, Y - direction.Y * 10),scb);
            edgePoints = new List<Vector2>();
            edgePoints.Add(new Vector2((float)X + direction.X * 20, (float)(Y - direction.Y * 20)));
            edgePoints.Add(new Vector2(X + lwX * 7, Y - rwY * 7)); 
            edgePoints.Add(new Vector2(X + lwX * 7, Y - rwY * 7));

            shape.Dispose();
            sink.Dispose();
        }

        public void UpdateRotation()
        {
            //Rotate ship by rotationSpeed radians.
            float rX = (float)(direction.X * Math.Cos(rotationSpeed) - direction.Y * Math.Sin(rotationSpeed));
            float rY = (float)(direction.X * Math.Sin(rotationSpeed) + direction.Y * Math.Cos(rotationSpeed));
            direction.X = rX;
            direction.Y = rY;

            rotationSpeed = 0.0f;
        }
        public void UpdateSpeed(int screenWidth, int screenHeight)
        {
            X += (int)(velocity.X);
            Y -= (int)(velocity.Y);

            if (X < -25)
                X = screenWidth + 25;
            else if (X > screenWidth + 25)
                X = -25;

            if (Y < -25)
                Y = screenHeight + 25;
            else if (Y > screenHeight + 25)
                Y = -25;
        }
    }
}
