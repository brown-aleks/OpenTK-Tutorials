using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace _3_MoreObjects
{
    // В этом примере рассматривается возможность отрисовки нескольких разных треугольников.
    public class Window : GameWindow
    {
        private float FremeTime = 0.0f;
        private int FPS = 0;

        private readonly float[] _verticesfirst =
        {
            0.4f,  0.8f, 0.0f,
             0.8f, 0.0f, 0.0f,
            0.4f, -0.8f, 0.0f,
            -0.4f,  0.0f, 0.0f,
        };
        private readonly float[] _verticessecond =
        {
            -0.4f,  0.8f, 0.0f,
             0.4f, 0.0f, 0.0f,
            -0.4f, -0.8f, 0.0f,
            -0.8f,  0.0f, 0.0f,
        };

        private readonly uint[] _index =
        {
            0,1,3,
            1,2,3
        };

        private int[] VBO = new int[2];     //  VBO
        private int[] VAO = new int[2];     //  VAO
        private int[] EBO = new int[2];     //  EBO 

        private Shader _shader_Orange, _shader_Green;             //  Shader

        public Window(NativeWindowSettings nativeWindowSettings): base(GameWindowSettings.Default, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
            CursorVisible = true;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.GenBuffers(2, VBO);           //  VBO
            GL.GenVertexArrays(2, VAO);     //  VAO
            GL.GenBuffers(2, EBO);          //  EBO

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticesfirst.Length * sizeof(float), _verticesfirst, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VAO[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO[0]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _index.Length * sizeof(int), _index, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticessecond.Length * sizeof(float), _verticessecond, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VAO[1]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _index.Length * sizeof(int), _index, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);

            _shader_Orange = new Shader("Shaders/shader.vert", "Shaders/OrangeShader.frag");
            _shader_Green = new Shader("Shaders/shader.vert", "Shaders/GreenShader.frag");
            _shader_Orange.Use();
            _shader_Green.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader_Orange.Use();
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(VAO[0]);    //  первый ромб
            GL.DrawElements(PrimitiveType.Triangles, _index.Length, DrawElementsType.UnsignedInt, 0);

            _shader_Green.Use();
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(VAO[1]);   //  второй ромб
            GL.DrawElements(PrimitiveType.Triangles, _index.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            FremeTime += (float)e.Time;
            FPS++;
            if (FremeTime >= 1.0f)
            {
                Title = $"More Objects FPS {FPS}";
                FremeTime = 0;
                FPS = 0;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffers(2,VBO);
            GL.DeleteVertexArrays(2,VAO);

            GL.DeleteProgram(_shader_Orange.Handle);
            GL.DeleteProgram(_shader_Green.Handle);
        }

    }
}