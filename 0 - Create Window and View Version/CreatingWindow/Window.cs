using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CreatingWindow
{
    /*  Класс OpenTK.Windowing.Desktop.GameWindow содержит кроссплатформенные методы для создания рендеринга в окне OpenGL,
     *  для обработки ввода и загрузки ресурсов. GameWindow содержит несколько событий, которые вы можете перехватить или
     *  переопределить в дочернем классе Window, чтобы добавить собственную логику. */

    public class Window : GameWindow
    {
        private bool _WriteEvents_GameWindow = false;
        private bool _WriteEvents_NativeWindow = true;

        public Window(NativeWindowSettings nativeWindowSettings) : base(GameWindowSettings.Default, nativeWindowSettings)
        {
            Debug.WriteLine("Version:  " + GL.GetString(StringName.Version));
            Debug.WriteLine("Vendor:   " + GL.GetString(StringName.Vendor));
            Debug.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Debug.WriteLine("ShadingLanguageVersion: " + GL.GetString(StringName.ShadingLanguageVersion));
            Debug.WriteLine("Extensions:             " + GL.GetString(StringName.Extensions));
            Debug.WriteLine("-------------------------------------------");

            VSync = VSyncMode.On;
            CursorVisible = true;
        }

        /* OpenTK.Windowing.Desktop.GameWindow.OnLoad: происходит после создания контекста OpenGL, но перед входом в основной цикл.
         * Выполняется сразу после вызова Run(). Переопределить для загрузки ресурсов. */
        protected override void OnLoad()
        {
            if (_WriteEvents_GameWindow) Debug.WriteLine("OnLoad()");
            base.OnLoad();
        }

        /* OpenTK.Windowing.Desktop.GameWindow.OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs):
         * происходит с указанной частотой рендеринга кадров. Переопределите, чтобы добавить свой код рендеринга.
         * args: Аргументы события для этого фрейма. */
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (_WriteEvents_GameWindow) Debug.WriteLine($"OnRenderFrame({args.Time})");
            base.OnRenderFrame(args);
        }

        //     Run when the update thread is started. This will never run if you set IsSingleThreaded
        //     to true.
        protected override void OnRenderThreadStarted()
        {
            if (_WriteEvents_GameWindow) Debug.WriteLine("OnRenderThreadStarted()");
            base.OnRenderThreadStarted();
        }

        /* OpenTK.Windowing.Desktop.GameWindow.OnUnload: Происходит после выхода из основного цикла, но перед удалением контекста OpenGL.
         * Выполняется сразу после вызова Close(). Переопределить, чтобы выгрузить ресурсы. */
        protected override void OnUnload()
        {
            if (_WriteEvents_GameWindow) Debug.WriteLine("OnUnload()");
            base.OnUnload();
        }

        /* OpenTK.Windowing.Desktop.GameWindow.OnUpdateFrame(OpenTK.Windowing.Common.FrameEventArgs):
         * происходит с указанной частотой обновления логики. Переопределите, чтобы добавить свою игровую логику.
         * args: Аргументы события для этого фрейма. */
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (_WriteEvents_GameWindow) Debug.WriteLine($"OnUpdateFrame({args.Time})");
            base.OnUpdateFrame(args);
        }

        //=================================================================================================================

        /*  А также Класс OpenTK.Windowing.Desktop.GameWindow содержит унаследованные события от класса NativeWindow,
         *  которые вы можете перехватить или переопределить в дочернем классе Window, чтобы добавить собственную логику
         *  к интерактивным свойствам окна. */

        // Происходит, когда окно собирается закрыться.
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnClosing(Cancel:{e.Cancel})");
            base.OnClosing(e);
        }

        //  Вызывается при перетавкивании на окно файлов (FileDrop). e: Аргументы события для этого фрейма.
        //  Содержит string[] путь\наименование файлов.
        protected override void OnFileDrop(FileDropEventArgs e)
        {
            if (_WriteEvents_NativeWindow)
            {
                foreach (var item in e.FileNames)
                {
                    Debug.WriteLine($"OnFileDrop({item})");
                }
            }
            base.OnFileDrop(e);
        }

        // Вызывается каждый раз, когда окно получает или теряет фокус. e: Аргумент события для этого фрейма.
        // Содержит bool значение текущего состояния фокуса.
        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnFocusedChanged(IsFocused:{e.IsFocused})");
            base.OnFocusedChanged(e);
        }

        // Вызывается каждый раз при подключении/отключении джойстика. e: Аргумент события для этого фрейма.
        // Содержит JoystickId - идетификатор джойстика который вызвал это событие, IsConnected - состояние подключения.
        protected override void OnJoystickConnected(JoystickEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnJoystickConnected(Joystick:{e.JoystickId}  {e.IsConnected})");
            base.OnJoystickConnected(e);
        }

        // Вызывается каждый раз при нажатии клавиши на клавиатуре. e: Аргумен события для этого фрейма.
        // Содержит bool Alt - Нажата ли клавиша Alt
        //          bool Command - Нажата ли специализированная клавиша Windows
        //          bool Ctrl - Нажата ли клавиша Ctrl
        //          bool Shift - Нажата ли клавиша Shift
        //          bool IsRepeat - Является ли вызов события повторным с одного и того же факта нажатия клавиши
        //          Keys Key - Значение клавиши которая вызвала это событие
        //          int  ScanCode - Код клавиши которая вызвала это событие
        //          KeyModifiers Modifiers - Получает побитовую комбинацию, представляющую модификаторы клавиш, которые были активны при создании этого события.
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (_WriteEvents_NativeWindow && !e.IsRepeat) Debug.WriteLine($"OnKeyDown(Alt:{e.Alt}  Command:{e.Command}  Ctrl:{e.Control}  IsRepeat:{e.IsRepeat}  Key:{e.Key}  Modifiers:{e.Modifiers}  ScanCode:{e.ScanCode}  Shift:{e.Shift})");
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
            {
                Close();
            }
            base.OnKeyDown(e);
        }

        // Вызывается каждый раз при отпущенной клавиши на клавиатуре. e: Аргумен события для этого фрейма.
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnKeyUp(Alt:{e.Alt}  Command:{e.Command}  Ctrl:{e.Control}  IsRepeat:{e.IsRepeat}  Key:{e.Key}  Modifiers:{e.Modifiers}  ScanCode:{e.ScanCode}  Shift:{e.Shift})");
            base.OnKeyUp(e);
        }

        // Вызывается при использовании кнопки (развернуть/свернуть в окно). e: Аргумен события для этого фрейма.
        // Содержит bool IsMaximized - развёрнуто ли окно на весь экран.
        protected override void OnMaximized(MaximizedEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMaximized(IsMaximized:{e.IsMaximized})");
            base.OnMaximized(e);
        }

        // Вызывается при использовании кнопки (свернуть). e: Аргумен события для этого фрейма.
        // Содержит bool IsMinimized - свёрнуто ли окно.
        protected override void OnMinimized(MinimizedEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMinimized(IsMinimized:{e.IsMinimized})");
            base.OnMinimized(e);
        }

        // Вызывается при нажатии кнопок мыши. e: Аргумен события для этого фрейма.
        // Содержит InputAction Action - Действие кнопки: Release - отпущена, Press - нажата, Repeat - клавиша удерживалась до тех пор пока событие не повторилось.
        //          MouseButton Button - Наименование кнопки вызвавшей событие.
        //          bool IsPressed - Получает значение, указывающее, была ли нажата или отпущена кнопка.
        //          KeyModifiers Modifiers - Получает побитовую комбинацию, представляющую модификаторы клавиш, которые были активны при создании этого события.
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseDown(Action:{e.Action}  Button:{e.Button}  IsPressed:{e.IsPressed}  Modifiers:{e.Modifiers})");
            base.OnMouseDown(e);
        }

        // Вызывается при отпускании кнопок мыши. e: Аргумен события для этого фрейма.
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseUp(Action:{e.Action}  Button:{e.Button}  IsPressed:{e.IsPressed}  Modifiers:{e.Modifiers})");
            base.OnMouseDown(e);
        }

        // Вызывается всякий раз когда курсор мыши попадает в область отрисовки с внешней стороны окна.
        protected override void OnMouseEnter()
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseEnter()");
            base.OnMouseEnter();
        }

        // Вызывается всякий раз когда курсор мыши покидает область отрисовки и переходит на внешнюю сторону окна.
        protected override void OnMouseLeave()
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseLeave()");
            base.OnMouseLeave();
        }

        // Вызывается всякий раз при движении колёсика мыши. e: Аргумен события для этого фрейма.
        // Содержит Vector2 Offset - Y (1,-1,) Поворот колёсика вперёд, назад, нет поворота соответственно. X (1,-1,0) Наклон колёсика влево, вправо, нет наклона.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseWheel(Offset:{e.Offset.X}, {e.Offset.Y}  OffsetX:{e.OffsetX}  OffsetY:{e.OffsetY})");
            base.OnMouseWheel(e);
        }

        // Вызывается каждый раз при изменении координат мыши. e: Аргумент события для этого фрейма.
        // Содержит Vector2 Position - Текущие координаты пикселя. 0,0 - верхний левый угол.
        //          Vector2 Delta - Разница между координатами мыши на текущем фрейме и предыдущем.
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMouseMove(Position:{e.Position}  Position:{e.Delta})");
            base.OnMouseMove(e);
        }

        // Вызывается каждый раз, когда изменяется положение верхнего левого угла окна, относительно экрана. e: Аргумент события для этого фрейма.
        // Содержит Vector2 Position - Текущие координаты пикселя верхнего левого угла окна, от верхнего левого угла экрана. 0,0 - верхний левый угол экрана.
        protected override void OnMove(WindowPositionEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnMove(Position:{e.Position})");
            base.OnMove(e);
        }

        // Вызывается после перерисовки окна.
        protected override void OnRefresh()
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnRefresh()");
            base.OnRefresh();
        }

        // Вызывается каждый раз, когда изменяется размер окна. e: Аргумент события для этого фрейма.
        // Содержит Vector2 Size - Новые Size.X и Size.Y, ширина и высота соответственно.
        protected override void OnResize(ResizeEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnResize(Size:{e.Size}  Width:{e.Width}  Height:{e.Height})");
            base.OnResize(e);
        }

        // Вызывается каждый раз при вводе текста. e: Аргумент события для этого фрейма.
        // Содержит string AsString - Стока с текстом.
        //          int Unicode - Код введёного символа из Unicode.
        protected override void OnTextInput(TextInputEventArgs e)
        {
            if (_WriteEvents_NativeWindow) Debug.WriteLine($"OnTextInput(AsString:{e.AsString}  Unicode:{e.Unicode})");
            base.OnTextInput(e);
        }
    }
}
