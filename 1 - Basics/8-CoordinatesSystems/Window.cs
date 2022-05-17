using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace _8_CoordinatesSystems
{
    // Теперь мы можем перемещаться по объектам. Однако как мы можем переместить нашу «камеру» или изменить нашу перспективу?
    // В этом руководстве я покажу вам, как настроить полную матрицу проекции / вида / модели (PVM).
    // Кроме того, мы заставим прямоугольник вращаться во времени.
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

        private Shader? _shader;

        private Texture? _texture;

        private Texture? _texture2;

        // Мы создаем двойное значение, чтобы узнать, сколько времени прошло с момента открытия программы.
        private double _time;

        // Затем мы создаем две матрицы для хранения нашего обзора и проекции. Они инициализируются внизу OnLoad.
        // Матрица вида - это то, что вы можете считать «камерой». Он представляет текущий видовой экран в окне.
        private Matrix4 _view;

        // Это показывает, как будут проецироваться вершины. Сложно объяснить через комментарии,
        // так что ознакомьтесь с веб-версией, чтобы хорошо продемонстрировать, что это делает.
        private Matrix4 _projection;

        public Window(NativeWindowSettings nativeWindowSettings) : base(GameWindowSettings.Default, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Здесь мы включаем проверку глубины. Если вы попытаетесь без этого нарисовать что-то более сложное, чем одна плоскость,
            // вы заметите, что многоугольники дальше на заднем плане иногда будут рисоваться поверх полигонов на переднем плане.
            // Очевидно, мы этого не хотим, поэтому включаем проверку глубины. Мы также очищаем буфер глубины в GL. Очистить в OnRenderFrame.
            GL.Enable(EnableCap.DepthTest);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // shader.vert был изменен. Взгляните на это после объяснения в OnRenderFrame.
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

            // Что касается представления, мы здесь особо не делаем. Следующий урок будет посвящен классу Camera, который упростит управление видом.
            // А пока перемещаем его назад на три единицы по оси Z.
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            // Для матрицы мы используем несколько параметров.
            //   Поле зрения. Это определяет, сколько одновременно может видеть область просмотра. 45 считается наиболее "реалистичным", но в настоящее время в большинстве видеоигр используется 90
            //   Соотношение сторон. Это должно быть установлено на ширину / высоту.
            // Почти обрезка. Любые вершины, расположенные ближе к камере, чем это значение, будут обрезаны.
            // Дальний отсечение. Любые вершины, находящиеся дальше от камеры, чем это значение, будут обрезаны.
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float) Size.Y, 0.1f, 100.0f);

            // Теперь перейдите в OnRenderFrame, чтобы увидеть, как мы настраиваем матрицу модели.
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // Мы добавляем время, прошедшее с последнего кадра, умноженное на 4,0 для ускорения анимации, к общему количеству прошедшего времени.
            _time += 4.0 * e.Time;

            // Мы очищаем буфер глубины в дополнение к буферу цвета.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            _texture.Use(TextureUnit.Texture0);
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            // Наконец, у нас есть матрица модели. Это определяет положение модели.
            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));

            // Затем мы передаем все эти матрицы в вершинный шейдер.
            // Вы также можете умножить их здесь, а затем передать, что быстрее, но наличие отдельных доступных матриц используется для некоторых дополнительных эффектов.

            // ВАЖНО: типы матриц OpenTK транспонируются из того, что ожидает OpenGL - строки и столбцы меняются местами.
            // Затем они правильно транспонируются при передаче в шейдер.
            // Это означает, что мы сохраняем один и тот же порядок умножения как в коде OpenTK C #, так и в коде шейдера GLSL.
            // Если вы передаете отдельные матрицы шейдеру и там умножаете, вы должны делать это в порядке «модель * вид * проекция».
            // Вы можете думать так: сначала примените матрицу modelToWorld (она же модель), затем примените матрицу worldToView (она же вид),
            // и, наконец, применяем матрицу viewToProjectedSpace (также известную как проекция).
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);

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