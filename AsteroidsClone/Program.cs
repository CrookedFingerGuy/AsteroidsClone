using System;
using SharpDX.Windows;

namespace AsteroidsClone
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            RForm rForm = new RForm("Sharp DX Template");

            RenderLoop.Run(rForm, () => rForm.rLoop());

            rForm.Dispose();
        }  
    }
}
