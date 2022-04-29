using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace _7_Transformations
{
    // Итак, вы можете настроить OpenGL, вы можете рисовать базовые формы, не тратя впустую вершины, и можете текстурировать их.
    // Однако осталась одна большая вещь: перемещение фигур.
    // Для этого мы используем линейную алгебру для перемещения вершин в вершинном шейдере.

    // Как отказ от ответственности: это руководство НЕ будет объяснять линейную алгебру или матрицы; эти темы слишком сложны, чтобы заниматься комментариями.
    // Если вы хотите получить более подробное представление о том, что здесь происходит, посмотрите веб-версию этого руководства.
    // Глубокое понимание линейной алгебры не потребуется для этого руководства, поскольку OpenTK включает встроенные типы матриц, которые абстрагируются от фактической математики.

    // Перейдите в RenderFrame, чтобы увидеть, как мы можем применить преобразования к нашей форме.
    public class Window : GameWindow
    {
        private readonly float[] _vertices =
        {
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        private Texture _texture2;

        public Window(NativeWindowSettings nativeWindowSettings) : base(GameWindowSettings.Default, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
            CursorVisible = true;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Shader.vert был изменен, взгляните и на него.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Resources/container.png");
            _texture.Use(TextureUnit.Texture0);

            _texture2 = Texture.LoadFromFile("Resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            // Примечание: все матрицы, которые мы будем использовать для преобразований, имеют размер 4x4.

            // Начнем с единичной матрицы. Это простая матрица, которая вообще не перемещает вершины.
            var transform = Matrix4.Identity;

            // Следующие несколько шагов просто показывают, как использовать матричные функции OpenTK, и не являются необходимыми для того, чтобы матрица преобразования действительно работала.
            // Если хотите, можете просто передать шейдеру матрицу идентичности, хотя это никак не повлияет на вершины.

            // Следует отметить, что о матрицах важен порядок умножения. «matrixA * matrixB» и «matrixB * matrixA» означают разные вещи.
            // ОЧЕНЬ важно знать, что матрицы OpenTK так называемые строковые. Мы не будем вдаваться в подробности здесь, но вот хорошее место, чтобы узнать об этом больше:
            // https://www.scratchapixel.com/lessons/mathematics-physics-for-computer-graphics/geometry/row-major-vs-column-major-vector
            // Для нас это означает, что умножение матриц можно рассматривать как движение слева направо.
            // Итак, «вращать * переводить» означает сначала вращать (вокруг начала координат), а затем переводить, в отличие от «транслировать * вращать», что означает переводить, а затем вращать (вокруг начала координат).

            // Чтобы объединить две матрицы, вы их умножаете. Здесь мы объединяем матрицу преобразования с другой, созданной OpenTK, чтобы повернуть ее на 20 градусов.
            // Обратите внимание, что все функции Matrix4.CreateRotation принимают радианы, а не градусы. Используйте MathHelper.DegreesToRadians () для преобразования в радианы, если вы хотите использовать градусы.
            transform = transform * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(20f));

            // Далее масштабируем матрицу. Это сделает прямоугольник немного больше.
            transform = transform * Matrix4.CreateScale(1.1f);

            // Затем мы переводим матрицу, что немного сдвинет ее вправо вверху.
            // Обратите внимание, что мы еще не используем полную систему координат, поэтому перевод выполняется в нормализованных координатах устройства.
            // В следующем руководстве мы расскажем, как настроить его, чтобы мы могли использовать более удобочитаемые числа.
            transform = transform * Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            // Теперь, когда матрица готова, передаем ее вершинному шейдеру.
            // Переходим к shader.vert, чтобы увидеть, как мы наконец применим это к вершинам. (комент в шейдере // Тогда все, что вам нужно сделать, это умножить вершины на матрицу преобразования, и вы увидите свое преобразование в сцене!)
            _shader.SetMatrix4("transform", transform);

            // Вот и все! В следующем уроке мы увидим, как настроить полную систему координат.

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
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}