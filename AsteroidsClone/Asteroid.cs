using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;

namespace AsteroidsClone
{
    public class Asteroid
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int size { get; set; }
        public float speed { get; set; }
        public float rotationSpeed;
        public Vector2 direction;
        List<Vector2> points;
        public int numberOfPoints { get; set; }
        Random rand;


        public Asteroid(int x,int y, int s,Random r,Factory fact)
        {
            rand = r;
            X = x;
            Y = y;
            rotationSpeed = (float)rand.NextDouble()/5;
            speed = (float)(rand.NextDouble()+0.5f)*5;
            direction = new Vector2(1f, 0f);
            direction = RotatePointArroundXY(direction,(float)(rand.NextDouble() * 2 * Math.PI));
            numberOfPoints = 10;
            size = s;
            points = new List<Vector2>();
            Vector2 temp;
            Vector2 unitAtAsteroid;

            for (int i =0;i<numberOfPoints;i++)
            {
                float tempMagnitude = (float)(rand.NextDouble() +0.5f)* size;
                temp = RotatePointArroundXY(direction,Map(i,0,numberOfPoints,0,(float)(2*Math.PI)));                
                temp.X *= tempMagnitude;
                temp.Y *= tempMagnitude;
                points.Add(temp);
            }

        }


        public void DrawAsteroid(RenderTarget D2DRT,SolidColorBrush scb,Factory fact)
        {
            PathGeometry shape = new PathGeometry(fact);
            GeometrySink sink = shape.Open();

            Vector2 tempPoint = points[0];
            tempPoint.X += X;
            tempPoint.Y += Y;
            sink.BeginFigure(tempPoint, FigureBegin.Hollow);
            foreach (Vector2 p in points)
            {
                tempPoint = p;
                tempPoint.X += X;
                tempPoint.Y += Y;
                sink.AddLine(tempPoint);
            }
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();

            D2DRT.DrawGeometry(shape, scb, 1.0f);
            shape.Dispose();
            sink.Dispose(); 
        }

        public Asteroid Update(int screenWidth,int screenHeight,Factory fact)
        {
            X += (int)(direction.X * speed);
            Y += (int)(direction.Y * speed);
            
            for(int i=0;i<points.Count;i++)
            {
                points[i] = RotatePointArroundXY(points[i], rotationSpeed);
            }

            Asteroid tempA;
            if (X < -50)
            {
                tempA = new Asteroid(rand.Next(0, 100) - 100, rand.Next(0, screenHeight),size, rand,fact);
                return tempA;
            }
            else if (X > screenWidth + 50)
            {
                tempA = new Asteroid(rand.Next(0, 100) + screenWidth, rand.Next(0, screenHeight), size, rand, fact);
                return tempA;
            }

            if (Y < -25)
            {
                tempA = new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100)-100, size, rand, fact);
                return tempA;
            }
            else if (Y > screenHeight + 50)
            {
                tempA = new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100) +screenHeight, size, rand, fact);
                return tempA;
            }

            return this;
        }
        Vector2 RotatePointArroundXY(Vector2 value,float rotation)
        {            
            float rX = (float)(value.X * Math.Cos(rotation) - value.Y * Math.Sin(rotation));
            float rY = (float)(value.X * Math.Sin(rotation) + value.Y * Math.Cos(rotation));
            value.X = rX;
            value.Y = rY;
            return value;
        }

        public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public List<Asteroid> BreakupAsteroid(Factory fact)
        {
            List<Asteroid> tempAsteroids = new List<Asteroid>();
            int count = rand.Next(2,4);
            if (size > 15)
            {
                for (int i = 0; i < count; i++)
                {
                    tempAsteroids.Add(new Asteroid(X, Y, (int)size / 2, rand, fact));
                }
            }
            return tempAsteroids;
        }

    }
}
