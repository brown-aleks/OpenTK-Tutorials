using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace CreatingWindow
{
    class Program
    {
        static void Main(string[] args)
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Location = new Vector2i (100,100),
                Title = "CreatingWindow",
                APIVersion = new Version(4,6),
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core
            };

            using (Window game = new Window(nativeWindowSettings))
            {
                game.Run();
            }
        }
    }
}
