using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace AsteroidsClone
{
    public class Asteroid
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float size { get; set; }
        public float speed { get; set; }
        public float rotationSpeed;
        public Vector2 direction;
        List<Vector2> points;
        public int numberOfPoints { get; set; }
        Random rand;


        public Asteroid(int x,int y, int s,Random r)
        {
            rand = r;
            X = x;
            Y = y;
            rotationSpeed = 0f;
            speed = (float)(rand.NextDouble()+0.5f)*5;
            direction = new Vector2(1f, 0f);
            direction = RotatePointArroundXY((float)(rand.NextDouble() * 2 * Math.PI));
            numberOfPoints = 10;
            size = s;
            points = new List<Vector2>();
            Vector2 temp;
            Vector2 unitAtAsteroid;
            

            for(int i =0;i<numberOfPoints;i++)
            {
                float tempMagnitude = (float)(rand.NextDouble() +0.5f)* size;
                temp = RotatePointArroundXY(Map(i,0,numberOfPoints,0,(float)(2*Math.PI)));                
                temp.X *= tempMagnitude;
                temp.Y *= tempMagnitude;
                points.Add(temp);
            }

        }


        public void DrawAsteroid(RenderTarget D2DRT,Factory fact,SolidColorBrush scb)
        {
            PathGeometry shape = new PathGeometry(fact);
            GeometrySink sink = shape.Open();

            Vector2 tempPoint = points[0];
            tempPoint.X += X;
            tempPoint.Y += Y;
            sink.BeginFigure(tempPoint, FigureBegin.Hollow);
            foreach(Vector2 p in points)
            {
                tempPoint = p;
                tempPoint.X += X;
                tempPoint.Y += Y;
                sink.AddLine(tempPoint);
            }

            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
            D2DRT.DrawGeometry(shape, scb, 1f);

        }

        public Asteroid Update(int screenWidth,int screenHeight)
        {
            X += (int)(direction.X * speed);
            Y += (int)(direction.Y * speed);

            Asteroid tempA;
            if (X < -50)
            {
                tempA = new Asteroid(rand.Next(0, 100) - 100, rand.Next(0, screenHeight), rand.Next(1, 4) * 10, rand);
                return tempA;
            }
            else if (X > screenWidth + 50)
            {
                tempA = new Asteroid(rand.Next(0, 100) + screenWidth, rand.Next(0, screenHeight), rand.Next(1, 4) * 10, rand);
                return tempA;
            }

            if (Y < -25)
            {
                tempA = new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100)-100, rand.Next(1, 4) * 10, rand);
                return tempA;
            }
            else if (Y > screenHeight + 50)
            {
                tempA = new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100) +screenHeight, rand.Next(1, 4) * 10, rand);
                return tempA;
            }

            return this;
        }
        Vector2 RotatePointArroundXY(float rotation)
        {
            Vector2 value;
            float rX = (float)(direction.X * Math.Cos(rotation) - direction.Y * Math.Sin(rotation));
            float rY = (float)(direction.X * Math.Sin(rotation) + direction.Y * Math.Cos(rotation));
            value.X = rX;
            value.Y = rY;
            return value;
        }

        public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public List<Asteroid> BreakupAsteroid()
        {
            List<Asteroid> tempAsteroids = new List<Asteroid>();
            int count = rand.Next(2, 4);
            for(int i=0;i<count;i++)
            {
                tempAsteroids.Add(new Asteroid(X,Y,(int)size/2,rand));
            }

            return tempAsteroids;
        }

    }
}
