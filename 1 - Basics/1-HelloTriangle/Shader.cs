using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace _1_HelloTriangle
{
    // Простой класс, предназначенный для помощи в создании шейдеров. https://github.com/opentk/LearnOpenTK/blob/master/Common/Shader.cs
    public class Shader
    {
        public readonly int Handle;
        private readonly Dictionary<string, int> _uniformLocations;

        // Вот как вы создаете простой шейдер.
        // Шейдеры написаны на GLSL, языке, очень похожем на C по своей семантике.
        // Исходный код GLSL компилируется *во время выполнения*, поэтому он может оптимизировать себя для видеокарты,
        // на которой он в данный момент используется.
        // Пример GLSL с комментариями можно найти в shaper.vert.
        public Shader(string vertPath, string fragPath)
        {
            // Существует несколько различных типов шейдеров, но для базового рендеринга вам нужны только два — вершинный и фрагментный шейдеры.
            // Вершинный шейдер отвечает за перемещение по вершинам и загрузку этих данных во фрагментный шейдер.
            // Здесь вершинный шейдер не будет слишком важен, но позже он станет более важным.
            // Фрагментный шейдер отвечает за последующее преобразование вершин во «фрагменты», которые представляют все данные,
            // необходимые OpenGL для отрисовки пикселя.
            // Фрагментный шейдер — это то, что мы будем здесь использовать больше всего.

            // Загружаем вершинный шейдер и компилируем
            var vertShaderSource = File.ReadAllText(vertPath);
            // Создаём пустой шейдер (очевидно). Перечисление ShaderType указывает, какой тип шейдера будет создан.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            // Теперь привяжем исходный код GLSL
            GL.ShaderSource(vertexShader, vertShaderSource);
            // А затем скомпилировать
            CompileShader(vertexShader);

            // Загружаем фрагментный шейдер и компилируем
            var fragShaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragShaderSource);
            CompileShader(fragmentShader);

            // Затем эти два шейдера должны быть объединены в шейдерную программу, которую затем может использовать OpenGL.
            // Для этого создаём программу...
            Handle = GL.CreateProgram();

            // Прикрепляем оба шейдера...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // А затем связываем их вместе.
            LinkProgram(Handle);

            // Когда шейдерная программа связана, ей больше не нужны прикрепленные к ней отдельные шейдеры;
            // скомпилированный код копируется в шейдерную программу.
            // Отсоединяем их, а затем удаляем.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Теперь шейдер готов к работе, но сначала мы собираемся кэшировать все местоположения юниформ шейдера.
            // Запрос этого из шейдера очень медленный, поэтому мы делаем это один раз при инициализации и повторно используем эти значения
            // потом.

            // Во-первых, нам нужно получить количество активных юниформ в шейдере.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Затем выделяем словарь для хранения местоположений.
            _uniformLocations = new Dictionary<string, int>();

            // Перебираем все униформы,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // получаем имя этой униформы,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                // получить местоположение,
                var location = GL.GetUniformLocation(Handle, key);
                // и затем добавляем его в словарь.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Пытаемся скомпилировать шейдер
            GL.CompileShader(shader);

            // Проверяем ошибки компиляции
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать GL.GetShaderInfoLog(shader), чтобы получить информацию об ошибке.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // Связываем программу
            GL.LinkProgram(program);

            // Проверяем ошибки линковки
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // Мы можем использовать GL.GetProgramInfoLog (программа), чтобы получить информацию об ошибке.
                var infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occurred whilst linking Program({program}).\n\n{infoLog}");
            }
        }

        // Функция-оболочка, которая включает программу шейдера.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // Источники шейдеров, предоставленные с этим проектом, используют жестко запрограммированный layout (location) -s. Если вы хотите сделать это динамически,
        // вы можете опустить строки макета (location = X) в вершинном шейдере и использовать их в VertexAttribPointer вместо жестко заданных значений.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Установщики униформы
        // Униформы - это переменные, которые могут быть установлены пользовательским кодом вместо чтения их из VBO.
        // Вы используете VBO для данных, связанных с вершинами, и униформы почти для всего остального.

        // Установка униформы почти всегда одна и та же, поэтому я объясню это здесь один раз, а не в каждом методе:
        // 1. Свяжите программу, на которую хотите установить униформу.
        // 2. Получить дескриптор местоположения униформы с помощью GL.GetUniformLocation.
        // 3. Используйте соответствующую функцию GL.Uniform *, чтобы установить униформу.

        /// <summary>
        /// Устанавливает униформу int на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу float на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу Matrix4 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        /// <remarks>
        ///   <para>
        ///   Матрица транспонируется перед отправкой в шейдер.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Устанавливает униформу Vector3 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        /// <summary>
        /// Устанавливает униформу Vector4 на этом шейдере.
        /// </summary>
        /// <param name="name">Название униформы</param>
        /// <param name="data">Данные для установки</param>
        public void SetVector4(string name, Vector4 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform4(_uniformLocations[name], data);
        }
    }
}