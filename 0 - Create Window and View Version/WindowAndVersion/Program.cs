using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace WindowAndVersion
{
    class Program
    {
        public static WindowIcon CreateWindowIcon(string path)
        {
            var image = new Bitmap(path);
            List<byte> data = new();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    data.AddRange(new byte[]
                    {
                        image.GetPixel(x,y).R,
                        image.GetPixel(x,y).G,
                        image.GetPixel(x,y).B,
                        image.GetPixel(x,y).A
                    });
                }
            }
            var windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, data.ToArray()));

            return windowIcon;
        }

				static void Main(string[] args)
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                // Получает или устанавливает начальный размер содержимого окна.
                // размер отображаемого окна в пикселях (по горизонтали, по вертикали)
                Size = new Vector2i(800, 600),

                // Получает или устанавливает место для открытия нового окна.
                // Если null, окно будет размещено ОС.
                // расположение окна в пикселях от левого верхнего края монитора (по горизонтали, по вертикали)
                Location = new Vector2i (100,100),

                // Устанавливает свойства рамки окна. Fixed(фиксированные), Hidden(скрытые), Resizable(по умолчанию изменяемые)
                WindowBorder = WindowBorder.Resizable,

                // Устанавливает начальное состояние окна. Normal(по умолчанию, развёрнуто, задаётся ОС),
                // Fullscreen(вью порт развёрнут на весь экран), Maximized(размер окна развёрнут на весь экран), Minimized(окно свёрнуто)
                WindowState = WindowState.Normal,

                //  Получает или задает значение, указывающее, должно ли новое окно быть видимым при создании, или нет. (по умолчанию true)
                StartVisible = true,

                // Получает или устанавливает значение, указывающее, должно ли это окно начинаться с фокуса при создании. (по умолчанию true)
                StartFocused = true,

                // Получает или устанавливает заголовок нового окна.
                Title = "WindowAndVersion",

                // Получает или задает значение, представляющее текущую версию графического API (OpenGL).
                // Проверить поддерживаемую версию OpenGL драйверами можно припомощи утилиты http://www.realtech-vr.com/home/glview
                APIVersion = new Version(4,6),

                // Получает или устанавливает значение, представляющее текущие флаги графического профиля.
                // (Default)Значение перечисления GraphicsContextFlags по умолчанию.
                // (Debug)Указывает, что это отладочный GraphicsContext. Контексты отладки могут предоставлять дополнительные отладочная
                // информация за счет производительности.
                // (ForwardCompatible)Указывает, что это GraphicsContext с прямой совместимостью. Совместимость с будущими версиями
                // контексты не поддерживают функциональные возможности, отмеченные как устаревшие в текущей версии GraphicsContextVersion.
                // Контексты с прямой совместимостью определены только для OpenGL версии 3.0 и выше.
                // (Offscreen)Указывает, что этот GraphicsContext предназначен для внеэкранного рендеринга.
                Flags = ContextFlags.ForwardCompatible,

                // Это влияет только на OpenGL 3.2 и выше. В более старых версиях этот параметр ничего не делает.
                // (Any)Используется для неизвестного профиля OpenGL или OpenGL ES.
                // (Compatability)Выбирает профиль совместимости. Вы должны использовать это только при поддержке устаревшего кода.
                // (Core)Выбирает основной профиль. Все новые проекты должны использовать это, если у них нет веских причин не делать этого.
                Profile = ContextProfile.Core,

                // Получает или задает значение, представляющее текущий графический API.
                // Если это изменится, вам также придется изменить версию API, так как версии OpenGL и OpenGL ES не совпадают.
                // Сводка:
                // (NoAPI)Указывает, что API специально не запрашивался для создания контекста.
                // Это в первую очередь для интеграции внешнего API с этим окном, например, Vulkan.
                // (OpenGLES)Указывает, что контекст должен быть создан для OpenGL ES.
                // (OpenGL)Указывает, что контекст должен быть создан для OpenGL.
                API = ContextAPI.OpenGL,

                // Получает или устанавливает значение, указывающее, является ли это окно управляемым событиями.
                // Окно, управляемое событиями, будет ждать событий перед обновлением/рендерингом. Это полезно
                // для неигровых приложений, где программе нужно только выполнять какую-либо обработку
                // после того, как пользователь что-то вводит.
                IsEventDriven = false,

                // Получает или устанавливает текущий OpenTK.Windowing.Common.Input.WindowIcon для этого окна.
                // Это ничего не делает в macOS; на этой платформе значок определяется пакетом приложений.
                Icon = CreateWindowIcon("helmet.ico")
            };

            using (Window game = new Window(nativeWindowSettings))
            {
                // Запуск бесконечного игрового цикла.
                game.Run();
            }
        }
    }

    public class Window : GameWindow
    {
        public Window(NativeWindowSettings nativeWindowSettings):base(GameWindowSettings.Default, nativeWindowSettings)
        {
            Console.WriteLine("Version:  " + GL.GetString(StringName.Version));
            Console.WriteLine("Vendor:   " + GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Console.WriteLine("ShadingLanguageVersion: " + GL.GetString(StringName.ShadingLanguageVersion));
            Console.WriteLine("Extensions:             " + GL.GetString(StringName.Extensions));
						
			// Получает или задает состояние VSync. VSync определяет, будет ли процес рендеринга кооличества кадров в секунду
			// синхронизирован с количеством кадров отображаемых оборудованием.
			// (Off) Вертикальная синхронизация отключена.
			// (On) Вертикальная синхронизация включена.
			// (Adaptive) VSync включен, если частота кадров не падает ниже половины целевой частоты кадров.
			// Если целевая частота кадров не указана, это ведет себя точно так же, как OpenTK.Windowing.Common.VSyncMode.On.
            VSync = VSyncMode.On;
						
			// Получает или задает значение, указывающее, виден ли курсор мыши.
            CursorVisible = true;
        }
    }
}
