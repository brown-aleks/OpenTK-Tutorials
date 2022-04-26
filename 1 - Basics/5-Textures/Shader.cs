using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace _5_Textures
{
    public class Shader
    {
        public readonly int Handle;
        private readonly Dictionary<string, int> _uniformLocations;
        private readonly Dictionary<string, int> _attribLocations;

        /// <summary>
        /// Создаём единую программу из двух шейдеров.
        /// </summary>
        /// <param name="vertPath">Путь/наименование вертексного шейдера</param>
        /// <param name="fragPath">Путь/наименование фрагментного шейдера</param>
        public Shader(string vertPath, string fragPath)
        {
            var vertShaderSource = File.ReadAllText(vertPath);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertShaderSource);
            CompileShader(vertexShader);

            var fragShaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragShaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveAttributes, out var numberOfAttrib);

            _attribLocations = new Dictionary<string, int>();

            Debug.WriteLine($"Active attributes: - {numberOfAttrib}");
            for (var i = 0; i < numberOfAttrib; i++)
            {
                var key = GL.GetActiveAttrib(Handle, i, out int size, out var type);
                var location = GL.GetAttribLocation(Handle, key);
                _attribLocations.Add(key, location);
                Debug.WriteLine($"{i} {key}\t{location}\t{Handle}\t{size}\t{type}");
            }

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            Debug.WriteLine($"Active uniformes: - {numberOfUniforms}");
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out int size, out var type);
                var location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
                Debug.WriteLine($"{i} {key}\t{location}\t{Handle}\t{size}\t{type}");
            }
        }
        /// <summary>
        /// Пытаемся скомпилировать шейдер. Проверяем ошибки компиляции.
        /// </summary>
        /// <param name="shader"></param>
        /// <exception cref="Exception"></exception>
        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }
        /// <summary>
        /// Связываем программу. Проверяем ошибки линковки
        /// </summary>
        /// <param name="program"></param>
        /// <exception cref="Exception"></exception>
        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occurred whilst linking Program({program}).\n\n{infoLog}");
            }
        }

        /// <summary>
        /// Функция-оболочка, которая включает программу шейдера.
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Источники шейдеров, предоставленные с этим проектом, используют жестко запрограммированный layout (location) -s. Если вы хотите сделать это динамически,
        /// вы можете опустить строки макета (location = X) в вершинном шейдере и использовать их в VertexAttribPointer вместо жестко заданных значений.
        /// </summary>
        /// <param name="attribName"> Наименование переменной атрибута </param>
        /// <returns> Возвращает int индекс месторасположения переменной атрибута </returns>
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Установщики атрибутов
        // Атрибуты - это входящие переменные, из VBO. Которые асоциированны через VAO для каждой вершины.

        /// <summary>
        /// Связывает атрибут шейдера и точку привязки в VAO.
        /// </summary>
        /// <param name="attribname">Наименование атрибута, которое указано в шейдере</param>
        /// <param name="bindingindex">Индек в точке привязки VAO</param>
        public void SetAttrib(string attribname, int bindingindex)
        {
            GL.UseProgram(Handle);
            GL.VertexAttribBinding(_attribLocations[attribname], bindingindex);
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