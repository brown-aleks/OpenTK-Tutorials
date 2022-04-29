using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace _7_Transformations
{
    // Вспомогательный класс, очень похожий на Shader, предназначен для упрощения загрузки текстур.
    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path)
        {
            // Генерируем дескриптор
            int handle = GL.GenTexture();

            // Связываем дескриптор
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // В этом примере мы собираемся использовать встроенную в .NET библиотеку System.Drawing для загрузки текстур.

            // Загружаем изображение
            using (var image = new Bitmap(path))
            {
                // Наше растровое изображение загружается из левого верхнего пикселя, тогда как OpenGL загружается из левого нижнего угла, в результате чего текстура переворачивается по вертикали.
                // Это исправит это, и текстура будет отображаться правильно.
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                // Сначала мы получаем пиксели из загруженного растрового изображения.
                // Аргументы:
                // Площадь в пикселях, которую мы хотим. Обычно вы хотите оставить его как от (0,0) до (ширина, высота), но вы можете
                // использовать другие прямоугольники для получения сегментов текстур, полезных для таких вещей, как таблицы спрайтов.
                // Режим блокировки. В основном, как вы хотите использовать пиксели. Поскольку мы передаем их в OpenGL,
                // нам нужен только ReadOnly.
                // Далее следует формат пикселей, в котором должны быть пиксели. В этом случае будет достаточно ARGB.
                // Мы должны полностью указать имя, потому что OpenTK также имеет перечисление с именем PixelFormat.
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Теперь, когда наши пиксели подготовлены, пора сгенерировать текстуру. Мы делаем это с помощью GL.TexImage2D.
                // Аргументы:
                // Тип создаваемой текстуры. Существуют разные типы текстур, но сейчас нам нужна только Texture2D.
                //   Уровень проработанности деталей. Мы можем использовать это, чтобы начать с меньшего MIP-карты (если мы хотим), но нам не нужно этого делать, поэтому оставьте его на 0.
                // Целевой формат пикселей. Это формат, в котором OpenGL будет хранить наше изображение.
                // Ширина изображения
                // Высота изображения.
                // Граница изображения. Всегда должно быть 0; это устаревший параметр, от которого Хронос так и не избавился.
                // Формат пикселей, описанный выше. Поскольку раньше мы загружали пиксели как ARGB, нам нужно использовать BGRA.
                // Тип данных пикселей.
                // И, наконец, собственно пиксели.
                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            // Теперь, когда наша текстура загружена, мы можем установить несколько параметров, чтобы повлиять на то, как изображение будет отображаться при рендеринге.

            // Сначала мы устанавливаем фильтр min и mag. Они используются, когда текстура масштабируется соответственно вниз и вверх.
            // Здесь мы используем Linear для обоих. Это означает, что OpenGL попытается смешать пиксели, а это означает, что текстуры, масштабированные слишком далеко, будут выглядеть размытыми.
            // Вы также можете использовать (среди других параметров) Nearest, который просто захватывает ближайший пиксель, что делает текстуру пиксельной при слишком большом масштабировании.
            // ПРИМЕЧАНИЕ: настройками по умолчанию для обоих из них являются LinearMipmap. Если вы оставите их по умолчанию, но не создадите MIP-карты,
            // ваше изображение вообще не будет отображаться (обычно вместо этого получается чистый черный цвет).
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Теперь устанавливаем режим обтекания. S для оси X, а T для оси Y.
            // Мы устанавливаем значение Repeat, чтобы текстуры повторялись при переносе. Здесь не показано, так как координаты текстуры точно совпадают.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Затем создаем MIP-карты.
            // MIP-карты - это уменьшенные копии текстуры в уменьшенном масштабе. Каждый уровень MIP-карты вдвое меньше предыдущего.
            // Сгенерированные MIP-карты уменьшаются до одного пикселя.
            // OpenGL будет автоматически переключаться между MIP-картами, когда объект удаляется достаточно далеко.
            // Это предотвращает эффект муара, а также экономит полосу пропускания текстуры.
            // Здесь вы можете увидеть и прочитать об эффекте Морье https://en.wikipedia.org/wiki/Moir%C3%A9_pattern
            // Вот пример mips в действии https://en.wikipedia.org/wiki/File:Mipmap_Aliasing_Comparison.png
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(handle);
        }

        public Texture(int glHandle)
        {
            Handle = glHandle;
        }

        // Активируем текстуру
        // Можно связать несколько текстур, если вашему шейдеру требуется больше одной.
        // Если вы хотите это сделать, используйте GL.ActiveTexture, чтобы указать, к какому слоту привязывается GL.BindTexture.
        // Стандарт OpenGL требует, чтобы их было как минимум 16, но их может быть больше в зависимости от вашей видеокарты.
        /// <summary>
        /// Привязывает данную текстуру к текстурному слоту. По умолчанию вновь загруженная тектура привязывается к слоту TextureUnit.Texture0
        /// </summary>
        /// <param name="unit"> Слот к которому привязывается текстура </param>
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
