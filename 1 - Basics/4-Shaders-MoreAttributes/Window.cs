using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace _4_Shaders
{
    // В этом проекте мы назначим треугольнику 3 цвета, по одному для каждой вершины.
    // Результатом будет интерполированное значение, основанное на расстоянии от каждой вершины.
    // Если вы хотите больше узнать об этом, промежуточный шаг называется Rasterizer.
    public class Window : GameWindow
    {

        // Мы назначаем три разных цвета в позиции ассоциированной вершины:
		// синий для вершины, зеленый для левого нижнего и красный для правого нижнего.
        private readonly float[] _vertices =
        {
             // positions        // colors
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
             0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
        };

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        public Window(NativeWindowSettings nativeWindowSettings): base(GameWindowSettings.Default, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
            CursorVisible = true;
        }

        // Теперь мы начинаем инициализировать OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Как и раньше, мы создаем указатель для трех компонентов положения наших вершин.
            // Единственная разница в том, что нам нужно учитывать 3 значения цвета в переменной шага.
            // Таким образом, шаг содержит 6 чисел с плавающей запятой вместо 3.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Создаем новый указатель для значений цвета.
            // Как и в предыдущем указателе, мы присваиваем 6 в значении шага.
            // Нам также нужно правильно установить смещение, чтобы получить значения цвета.
            // Данные цвета начинаются после данных позиции, поэтому смещение составляет 3 числа с плавающей запятой.
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            // Затем мы включаем атрибут цвета (location = 1), чтобы он был доступен шейдеру.
            GL.EnableVertexAttribArray(1);

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