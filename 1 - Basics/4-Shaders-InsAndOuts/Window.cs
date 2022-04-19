using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace _4_Shaders

{   // Здесь мы подробно рассмотрим, что могут делать шейдеры из проекта Hello World, над которым мы работали раньше.
    // В частности, мы покажем, как шейдеры обрабатывают ввод и вывод из основной программы
    // и между собой.
    public class Window : GameWindow
    {

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Нижняя левая вершина
             0.5f, -0.5f, 0.0f, // Внизу правая вершина
             0.0f,  0.5f, 0.0f  // Верхняя вершина
        };

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        public Window(NativeWindowSettings nativeWindowSettings): base(GameWindowSettings.Default, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
            CursorVisible = true;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Атрибуты вершин - это данные, которые мы отправляем в качестве входных данных в вершинный шейдер из основной программы.
            // Итак, здесь мы проверяем, сколько атрибутов вершин может обработать наше оборудование.
            // OpenGL поддерживает как минимум 16 атрибутов вершин. Это нужно только назвать
            // когда ваш интенсивный атрибут работает и нужно точно знать, сколько вам доступно.
            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

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
    }
}