using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.XInput;

namespace AsteroidsClone
{
    public class UserInputProcessor
    {
        TextFormat linesTextFormat;
        RawRectangleF linesTextArea;
        string textForDebug = "Test";
        Controller[] controllers;
        Controller controller = null;
        public int oldPacketNumber;

        public UserInputProcessor()
        {
            // Initialize XInput
            controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            // Get 1st controller available

            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    controller = selectControler;
                    break;
                }
            }

            linesTextFormat = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Isolated), "Gill Sans", FontWeight.UltraBold, FontStyle.Normal, 20);
            linesTextArea = new SharpDX.Mathematics.Interop.RawRectangleF(10, 80, 550, 150);
        }

        public void DisplayGamePadState(RenderTarget d2dRT, Brush brush)
        {
            if (controller == null)
            {
                textForDebug = "No XInput controller installed";
            }
            else
            {

                textForDebug = "Found a XInput controller available";

                // Poll events from joystick
                var state = controller.GetState();
                d2dRT.DrawText("button pressed: " + state.Gamepad.ToString(), linesTextFormat, linesTextArea, brush);
            }
        }

        public int CheckGamePadButtons()
        {
            State state = controller.GetState();
            if (controller != null)
            {
                if (state.Gamepad.Buttons == GamepadButtonFlags.DPadRight)
                {
                    return 1;
                }

                if (state.Gamepad.Buttons == GamepadButtonFlags.DPadLeft)
                {
                    return -1;
                }
            }

            return 0;
        }

        public State GetGamePadState()
        {
            if (controller != null)
                return controller.GetState();
            else
                return new State();
        }
    }

}
