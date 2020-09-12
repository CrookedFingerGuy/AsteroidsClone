using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectInput;
using SharpDX.XInput;


using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;

namespace AsteroidsClone
{
    public class RForm : RenderForm
    {
        SwapChainDescription desc;
        Device device;
        SwapChain swapChain;
        SharpDX.Direct2D1.Factory d2dFactory;
        Factory factory;
        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface surface;
        RenderTarget d2dRenderTarget;
        SolidColorBrush solidColorBrush;
        DirectInput directInput;
        Keyboard keyboard;
        KeyboardState keys;
        State gamePadState;

        UserInputProcessor userInputProcessor;
        Stopwatch gameInputTimer;
        TextFormat scoreTextFormat;
        RawRectangleF scoreTextArea;
        TextFormat gameOverTextFormat;
        RawRectangleF gameOverTextArea;
        RawRectangleF livesTextArea;
        Random rand;

        int screenWidth=1024;
        int screenHeight=768;
        Ship ship;
        List<Bullet> bullets;
        List<Asteroid> asteroids;
        int score = 0;
        int lives = 3;
        bool gameOver = false;

        public RForm(string text) : base(text)
        {
            this.ClientSize = new System.Drawing.Size(screenWidth, screenHeight);

            desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(this.ClientSize.Width, this.ClientSize.Height, new Rational(144, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = this.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out device, out swapChain);
            d2dFactory = new SharpDX.Direct2D1.Factory();
            factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(this.Handle, WindowAssociationFlags.IgnoreAll);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
            solidColorBrush = new SolidColorBrush(d2dRenderTarget, Color.White);
            solidColorBrush.Color = Color.White;
            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();
            userInputProcessor = new UserInputProcessor();
            scoreTextFormat = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated), "Gill Sans", FontWeight.UltraBold, FontStyle.Normal, 36);
            scoreTextArea = new SharpDX.Mathematics.Interop.RawRectangleF(10, 10, 400, 400);
            livesTextArea = new SharpDX.Mathematics.Interop.RawRectangleF(10,46,400,400);
            gameOverTextFormat = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated), "Gill Sans", FontWeight.UltraBold, FontStyle.Normal, 108);
            gameOverTextArea = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, screenWidth, screenHeight);
            gameOverTextFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
            gameOverTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center; ;
            rand = new Random();


            ship = new Ship(screenWidth / 2, screenHeight / 2);
            bullets = new List<Bullet>();
            asteroids = new List<Asteroid>();

            int count = rand.Next(10, 15);
            for (int i = 0; i < count; i++)
            {
                asteroids.Add(new Asteroid(rand.Next(-25,screenWidth+25), rand.Next(-25, screenWidth + 25), rand.Next(1,4)*10, rand, d2dFactory));
            }

            gameInputTimer = new Stopwatch();
            gameInputTimer.Start();
        }

        int bulletLimiter = 0;

        public void rLoop()
        {
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(Color.Black);

            d2dRenderTarget.DrawText(score.ToString(), scoreTextFormat, scoreTextArea, solidColorBrush);
            d2dRenderTarget.DrawText("Lives: "+lives.ToString(), scoreTextFormat, livesTextArea, solidColorBrush);

            ship.DrawShip(d2dRenderTarget, solidColorBrush, d2dFactory);
            foreach(Bullet b in bullets)
            {
                b.Draw(d2dRenderTarget, solidColorBrush);
            }

            foreach (Asteroid asteoid in asteroids)
            { 
                asteoid.DrawAsteroid(d2dRenderTarget, solidColorBrush,d2dFactory);
            }

            if(gameOver)
            {
                d2dRenderTarget.DrawText("Game Over", gameOverTextFormat, gameOverTextArea, solidColorBrush);
            }
            else if (gameInputTimer.ElapsedMilliseconds >= 25)
            {
                userInputProcessor.oldPacketNumber = gamePadState.PacketNumber;
                gamePadState = userInputProcessor.GetGamePadState();
                gameInputTimer.Restart();
                HandleInputs();
                ship.UpdateSpeed(screenWidth, screenHeight);

                for (int i = 0; i < asteroids.Count; i++)
                {
                    for (int j = 0; j < ship.edgePoints.Count; j++)
                    {
                        Vector2 tempA = new Vector2(asteroids[i].X, asteroids[i].Y);
                        Vector2 tempB = new Vector2(ship.edgePoints[j].X, ship.edgePoints[j].Y);
                        if (Math.Sqrt(((tempA.X - tempB.X) * (tempA.X - tempB.X) + (tempA.Y - tempB.Y) * (tempA.Y - tempB.Y))) < asteroids[i].size)
                        {
                            lives--;
                            if(lives==0)
                                gameOver = true;
                            ship = new Ship(screenWidth / 2, screenHeight / 2);
                            break;
                        }
                    }
                }

                Bullet toDelete=null;
                foreach (Bullet b in bullets)
                {
                    b.Update();
                    if (b.X < 0 || b.X > screenWidth || b.Y < 0 || b.Y > screenHeight)
                        toDelete = b;
                }
                if (toDelete != null)
                    bullets.Remove(toDelete);

                for (int i = 0; i < asteroids.Count; i++)
                {
                    asteroids[i] = asteroids[i].Update(screenWidth, screenHeight, d2dFactory);
                }
                 
                for (int i = 0; i < asteroids.Count; i++)
                {
                    for (int j = 0; j < bullets.Count; j++)
                    {
                        Vector2 tempA = new Vector2(asteroids[i].X, asteroids[i].Y);
                        Vector2 tempB = new Vector2(bullets[j].X, bullets[j].Y);
                        if (Math.Sqrt(((tempA.X - tempB.X) * (tempA.X - tempB.X) + (tempA.Y - tempB.Y) * (tempA.Y - tempB.Y))) < asteroids[i].size)
                        {
                            score +=asteroids[i].size * 50;
                            List<Asteroid> newAsteroids;
                            newAsteroids = asteroids[i].BreakupAsteroid(d2dFactory);
                            asteroids.Remove(asteroids[i]);
                            foreach (Asteroid asteroid in newAsteroids)
                            {
                                asteroids.Add(asteroid);
                            }
                            bullets.Remove(bullets[j]);
                            break;
                        }
                    }
                }

                if (asteroids.Count < 10)
                {
                    int count = rand.Next(1, 4);
                    for (int i = 0; i < count; i++)
                    {
                        switch (count)
                        {
                            case 1:
                                {
                                    asteroids.Add(new Asteroid(rand.Next(0, 100) - 100, rand.Next(0, screenHeight), rand.Next(1, 4) * 10, rand, d2dFactory));
                                }
                                break;
                            case 2:
                                {
                                    asteroids.Add(new Asteroid(rand.Next(0, 100) + screenWidth, rand.Next(0, screenHeight), rand.Next(1, 4) * 10, rand, d2dFactory));
                                }
                                break;
                            case 3:
                                {
                                    asteroids.Add(new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100) - 100, rand.Next(1, 4) * 10, rand, d2dFactory));
                                }
                                break;
                            case 4:
                                {
                                    asteroids.Add(new Asteroid(rand.Next(0, screenWidth), rand.Next(0, 100) + screenHeight, rand.Next(1, 4) * 10, rand, d2dFactory));
                                }
                                break;
                        }
                    }
                }
            }

            d2dRenderTarget.EndDraw();
            swapChain.Present(0, PresentFlags.None);
            //Thread.Sleep(100);
        }

        void HandleInputs()
        {
            keys = keyboard.GetCurrentState();


            if (keys.PressedKeys.Contains(Key.Up))
            {
                ship.velocity.X += (float)0.2f * ship.direction.X;
                ship.velocity.Y += (float)0.2f * ship.direction.Y;

            }

            if (keys.PressedKeys.Contains(Key.Down))
            {
                ship.velocity.X -= (float)0.2f * ship.direction.X;
                ship.velocity.Y -= (float)0.2f * ship.direction.Y;
            }

            if (keys.PressedKeys.Contains(Key.Left))
            {
                ship.rotationSpeed = 0.1f;
                ship.UpdateRotation();
            }

            if (keys.PressedKeys.Contains(Key.Right))
            {
                ship.rotationSpeed = -0.1f;
                ship.UpdateRotation();
            }

            if (bulletLimiter == 10)
            {
                if (keys.PressedKeys.Contains(Key.Space))
                {
                    bullets.Add(new Bullet((int)(ship.X + ship.direction.X * 20), (int)(ship.Y - ship.direction.Y * 20), ship.direction));
                }
                bulletLimiter = 0;
                if (keys.PressedKeys.Contains(Key.Return))
                {
                    List<Asteroid> tempAs = new List<Asteroid>();
                    Asteroid toDelete;
                    foreach (Asteroid asteroid in asteroids)
                    {
                        if (asteroid.size > 10)
                        {
                            tempAs = asteroid.BreakupAsteroid(d2dFactory);
                        }
                        else
                        {
                            toDelete = asteroid;
                        }
                    }

                    if (tempAs.Count > 0)
                    {
                        foreach (Asteroid a in tempAs)
                        {
                            asteroids.Add(a);
                        }
                        asteroids.Remove(asteroids[0]);
                    }

                }
            }
            bulletLimiter++;
        }

        ~RForm()
        {
            keyboard.Unacquire();
            keyboard.Dispose();
            directInput.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            device.ImmediateContext.ClearState();
            device.ImmediateContext.Flush();
            device.Dispose();
            device.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }

    }

}
