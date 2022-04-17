using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace _2_ElementBufferObjects
{
    // Итак, вы нарисовали первый треугольник. Но как насчет рисования нескольких?
    // Вы можете просто добавить больше вершин в массив, и это технически сработает, но предположим, что вы рисуете прямоугольник.
    // Для этого нужны только четыре вершины, но поскольку OpenGL работает с треугольниками, вам нужно определить 6.
    // Ничего особенного, но быстро складывается, когда вы переходите к более сложным моделям. Например, кубу нужно всего 8 вершин, но
    // для этого потребуется 36 вершин!

    // OpenGL предоставляет способ повторного использования вершин, что может значительно снизить использование памяти сложными объектами.
    // Это называется объектом-буфером элемента. Это руководство будет посвящено тому, как его настроить.
    public class Window : GameWindow
    {
        // Мы модифицируем массив вершин, чтобы включить четыре вершины для нашего прямоугольника.
        private readonly float[] _vertices =
        {
             0.5f,  0.5f, 0.0f, // в правом верхнем углу
             0.5f, -0.5f, 0.0f, // внизу справа
            -0.5f, -0.5f, 0.0f, // Нижняя левая
            -0.5f,  0.5f, 0.0f, // верхний левый
        };

        // Затем мы создаем новый массив: index.
        // Этот массив контролирует, как EBO будет использовать эти вершины для создания треугольников
        private readonly uint[] _indices =
        {
            // Обратите внимание, что индексы начинаются с 0!
            0, 1, 3, // Первый треугольник будет правой нижней половиной треугольника
            1, 2, 3  // Тогда вторая будет правая верхняя половина треугольника
        };

        private int _fps;
        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        // Добавляем дескриптор EBO
        private int _elementBufferObject;

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

            // Мы создаем / связываем EBO объекта буфера элемента так же, как и VBO, за исключением того, что здесь есть существенная разница, которая может ДЕЙСТВИТЕЛЬНО сбить с толку.
            // Точка привязки для ElementArrayBuffer на самом деле не является глобальной точкой привязки, как ArrayBuffer.
            // Вместо этого это фактически свойство связанного в данный момент VertexArrayObject, и привязка EBO без VAO является неопределенным поведением.
            // Это также означает, что если вы привяжете другой VAO, текущий ElementArrayBuffer изменится вместе с ним.
            // Еще одна хитрость в том, что вам не нужно отвязать буфер в ElementArrayBuffer, так как это сделает отсоединение VAO,
            // и отмена привязки EBO удалит его из VAO вместо того, чтобы отвязать его, как вы это делали бы для VBO или VAO.
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            // Мы также загружаем данные в EBO так же, как и с VBO.
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
            // EBO теперь правильно настроен. Перейдите к функции Render, чтобы увидеть, как мы теперь рисуем наш прямоугольник!

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            _shader.Use();

            // Поскольку ElementArrayObject является свойством привязанного в данный момент VAO,
            // буфер, который вы найдете в ElementArrayBuffer, изменится в соответствии с текущим привязанным VAO.
            GL.BindVertexArray(_vertexArrayObject);

            // Затем замените вызов DrawTriangles на вызов DrawElements
            // Аргументы:
            // Примитивный тип для рисования. Треугольники в данном случае.
            // Сколько индексов нужно нарисовать. Шесть в этом случае.
            // Тип данных индексов. Индексы представляют собой беззнаковое целое число, поэтому мы тоже хотим этого здесь.
            // Смещение в EBO. Установите значение 0, потому что мы хотим нарисовать все целиком.

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); //Способ отображения    //  PolygonMode.Fill - Отрисовывает примитив заполненным.
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

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

            Title = $"Element Buffer Objects FPS - {_fps++}";
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
        protected override void OnUnload()
        {
            base.OnUnload();

            // Отключаем все ресурсы, привязывая цели к 0 / null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Удаляем все ресурсы.
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);
        }

    }
}