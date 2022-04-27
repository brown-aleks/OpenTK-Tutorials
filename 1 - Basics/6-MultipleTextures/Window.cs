using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace _6_MultipleTextures
{
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

        private Texture _texture_container;

        private Texture _texture_awesomeface;

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

            // shader.frag снова был изменен, взгляните и на него.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            // Создаём экземпляры обьекта Texture. По умолчанию вновь загруженная тектура привязывается к слоту TextureUnit.Texture0
            _texture_container = Texture.LoadFromFile("Resources/container.png");

            // Следующий созданный экземпляр так же привязывается к слоту TextureUnit.Texture0 вместо предидущего.
            // И на данный момент предыдущий экземпляр остаётся ни куда не привязанным.
            _texture_awesomeface = Texture.LoadFromFile("Resources/awesomeface.png");

            // Здесь мы привязываем каждую текстуру к нужному нам слоту.
            _texture_container.Use(TextureUnit.Texture0);
            _texture_awesomeface.Use(TextureUnit.Texture1);

            // Через установщик униформы, переменным типа (uniform sampler2D) присваиваем значения типа (int).
            // Которое указывает номер текстурного слота на который необходимо ссылаться при использовании переменной в шейдере.
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(_vertexArrayObject);

            // Перед отрисовкой, обновим привязки текстур к слоту, на тот случай если они были изменины в других
            // участках программы. Конкретно в этом примере, тут повторную привязку можно было и не делать.
            // Привязка сохраняется на протяжении всего времени существования экземпляра класса Window.
            // Или до тех пор пока не будет изменина другими текстурными обьектами при помощи GL.BindTexture(TextureTarget target, int texture)
            _texture_container.Use(TextureUnit.Texture0);
            _texture_awesomeface.Use(TextureUnit.Texture1);
            _shader.Use();

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