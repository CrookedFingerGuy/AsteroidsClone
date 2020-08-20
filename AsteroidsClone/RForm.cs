using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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
        KeyboardUpdate[] keyData;
        KeyboardState keys;
        State gamePadState;

        UserInputProcessor userInputProcessor;
        Stopwatch gameInputTimer;
        TextFormat TestTextFormat;
        RawRectangleF TestTextArea;
        Bitmap Background;
        Random rand;

        int screenWidth=1024;
        int screenHeight=768;
        Ship ship;
        List<Bullet> bullets;
        List<Asteroid> asteroids;

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
            solidColorBrush.Color = Color.Purple;
            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();
            userInputProcessor = new UserInputProcessor();
            TestTextFormat = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated), "Gill Sans", FontWeight.UltraBold, FontStyle.Normal, 36);
            TestTextArea = new SharpDX.Mathematics.Interop.RawRectangleF(10, 10, 400, 400);
            rand = new Random();


            ship = new Ship(screenWidth / 2, screenHeight / 2);
            bullets = new List<Bullet>();
            asteroids = new List<Asteroid>();

            int count = rand.Next(5, 10);
            for (int i = 0; i < count; i++)
            {
                asteroids.Add(new Asteroid(rand.Next(-25,screenWidth+25), rand.Next(-25, screenWidth + 25), rand.Next(1,4)*10, rand));
            }

            gameInputTimer = new Stopwatch();
            gameInputTimer.Start();
        }

        int bulletLimiter = 0;

        public void rLoop()
        {
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(Color.Black);
            //d2dRenderTarget.DrawText("Test", TestTextFormat, TestTextArea, solidColorBrush);
            //userInputProcessor.DisplayGamePadState(d2dRenderTarget, solidColorBrush);


            ship.DrawShip(d2dRenderTarget, solidColorBrush, d2dFactory);
            foreach(Bullet b in bullets)
            {
                b.Draw(d2dRenderTarget, solidColorBrush);
            }

            foreach (Asteroid asteoid in asteroids)
            { 
                asteoid.DrawAsteroid(d2dRenderTarget, d2dFactory, solidColorBrush);
            }


            if (gameInputTimer.ElapsedMilliseconds >= 25)
            {
                userInputProcessor.oldPacketNumber = gamePadState.PacketNumber;
                gamePadState = userInputProcessor.GetGamePadState();
                gameInputTimer.Restart();
                HandleInputs();
                ship.UpdateSpeed(screenWidth,screenHeight);
                foreach(Bullet b in bullets)
                {
                    b.Update();
                }

                for(int i=0;i<asteroids.Count;i++)
                {
                    asteroids[i]=asteroids[i].Update(screenWidth,screenHeight);
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
                ship.rotationSpeed = 0.2f;
                ship.UpdateRotation();
            }

            if (keys.PressedKeys.Contains(Key.Right))
            {
                ship.rotationSpeed = -0.2f;
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
                            tempAs = asteroid.BreakupAsteroid();
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
