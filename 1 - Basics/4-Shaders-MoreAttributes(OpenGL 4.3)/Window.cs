using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace _4_Shaders
{
    /* Суть проекта такаяже как и упредидущего. Но в OpenGL 4.3 появился альтернативный
     * (необязательно лучший) способ определения состояния объекта массива вершин
     * (формат атрибутов, активные атрибуты и буферы). В предыдущем примере вызов
     * функции GL.VertexAttribPointer решает две важные задачи. Во-первых, она косвенно
     * определяет, какой буфер содержит данные для атрибута, какой буфер в настоящий
     * момент(на момент вызова) связан с BufferTarget.ArrayBuffer. Во-вторых, она
     * определяет формат данных(тип, смещение, шаг и другие характеристики). Возможно,
     * код будет выглядеть яснее, если каждую из задач решать вызовом отдельной
     * функции.Именно это решение и было реализовано в OpenGL 4.3.
     * источник: https://dmkpress.com/catalog/computer/programming/978-5-97060-255-3/ 
     *           OPENGL 4. ЯЗЫК ШЕЙДЕРОВ. КНИГА РЕЦЕПТОВ   */
    public class Window : GameWindow
    {

        // Мы назначаем два разных массива в позиции ассоциированных вершин:
        private readonly float[] _positionData =
        {
             -0.8f, -0.8f, 0.0f,    //  левый нижний угол
              0.8f, -0.8f, 0.0f,    //  правый нижний угол
              0.8f,  0.8f, 0.0f,    //  правый верхний угол
             -0.8f,  0.8f, 0.0f     //  левый верхний угол
        };
        private readonly float[] _colorData = 
        {
             1.0f, 0.0f, 0.0f,    //  левый нижний угол
             0.0f, 1.0f, 0.0f,    //  правый нижний угол
             0.0f, 0.0f, 1.0f,    //  правый верхний угол
             1.0f, 0.0f, 1.0f     //  левый верхний угол
        };
        private readonly uint[] _index =
{
            0,1,3,
            1,2,3
        };

        private int[] _vbo;
        private int _vao, _ebo;

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

            //  Создать и заполнить буферные объекты
            _vbo = new int[2];
            GL.GenBuffers(2, _vbo);
            int positionBufferHandle = _vbo[0];
            int colorBufferHandle = _vbo[1];

            //  Заполняем буфер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, positionBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, _positionData.Length * sizeof(float), _positionData, BufferUsageHint.StaticDraw);

            //  Заполняем буфер цветов
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, _colorData.Length * sizeof(float), _colorData, BufferUsageHint.StaticDraw);

            //  Создаём и связываем VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            //  Активируем атрибуты 0 и 1
            GL.EnableVertexArrayAttrib(_vao, 0);
            GL.EnableVertexArrayAttrib(_vao, 1);

            /*  С помощью GL.BindVertexBuffer, два буфера связываются с двумя разнынми
             *  индексами внутри точки привязки вершинных буферов (vertex buffer binding point).
             *  Обратите внимание, что здесь больше не используется точка привязки
             *  BufferTarget.ArrayBuffer. Вместо нее у нас теперь имеется новая точка привязки,
             *  предназначенная для вершинных буферов. Эта точка привязки имеет
             *  несколько индексов (обычно от 0 до 15), что дает возможность привязывать
             *  к ней несколько буферов. */
            GL.BindVertexBuffer(0, positionBufferHandle, IntPtr.Zero, sizeof(float) * 3);
            GL.BindVertexBuffer(1, colorBufferHandle, IntPtr.Zero, sizeof(float) * 3);

            /* C помощью GL.VertexAttribFormat задается формат данных для атрибута.
             * Обратите внимание, что на этот раз не указывается буфер, хранящий
             * данные, мы просто указываем формат данных для этого атрибута. */
            GL.VertexAttribFormat(0, 3, VertexAttribType.Float, false, 0);

            /*  Функция GL.VertexAttribBinding определяет отношения между буферами,
             *  связанными с точкой привязки вершинных буферов, и атрибутами. В первом
             *  аргументе передается индекс атрибута, а во втором – индекс внутри
             *  точки привязки вершинных буферов. В данном примере они совпадают,
             *  но их совпадение не является обязательным условием. */
            GL.VertexAttribBinding(0, 0);
            GL.VertexAttribFormat(1, 3, VertexAttribType.Float, false, 0);
            GL.VertexAttribBinding(1, 1);
            
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _index.Length * sizeof(int), _index, BufferUsageHint.StaticDraw);

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
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _index.Length, DrawElementsType.UnsignedInt, 0);

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