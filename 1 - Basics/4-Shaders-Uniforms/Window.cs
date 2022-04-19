using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace _4_Shaders
{
    // Этот проект исследует, как использовать тип uniform переменной, который позволяет вам присваивать значения
    // в шейдеры в любой момент проекта.
    public class Window : GameWindow
    {
        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        // Итак, мы заставим треугольник пульсировать между цветовым диапазоном.
        // Для этого нам понадобится постоянно меняющееся значение.
        // Секундомер идеально подходит для этого, поскольку он постоянно движется вверх.
        private Stopwatch _timer;

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

            GL.GetInteger(GetPName.MaxVertexAttribs, out int maxAttributeCount);
            Debug.WriteLine($"Maximum number of vertex attributes supported: {maxAttributeCount}");

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            // Здесь мы запускаем секундомер, так как этот метод вызывается только один раз.
            _timer = new Stopwatch();
            _timer.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            // Здесь мы получаем общее количество секунд, прошедших с момента последнего сброса этого метода
            // и присваиваем его переменной timeValue, чтобы ее можно было использовать для пульсирующего цвета.
            double timeValue = _timer.Elapsed.TotalSeconds;

            // Мы увеличиваем / уменьшаем зеленое значение, которое мы передаем
            // шейдер, основанный на timeValue, который мы создали в предыдущей строке,
            // а также использование некоторых встроенных математических функций, чтобы изменение было более плавным.
            float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;

            // Это получает унифицированное расположение переменной от фрагментарного шейдера, чтобы мы могли
            // присвоить ему новое значение зеленого цвета.
            int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");

            // Здесь мы назначаем переменную ourColor во фрагментном шейдере
            // через метод OpenGL Uniform, который принимает значение как отдельные значения vec (в данном случае всего 4).
            GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);

            // В качестве альтернативы вы можете использовать эту перегрузку той же функции.
            // GL.Uniform4 (vertexColorLocation, new OpenTK.Mat Mathematics.Color4 (0f, greenValue, 0f, 0f));

            // Bind the VAO
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