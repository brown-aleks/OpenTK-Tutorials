using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace _8_Cube_3D
{
    public static class Program
    {
        private static void Main()
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Location = new Vector2i(100, 100),
                Title = "Coordinates Systems",
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.ForwardCompatible,
                Profile = ContextProfile.Core
            };

            using Window game = new(nativeWindowSettings);
            game.Run();
        }
    }
}
