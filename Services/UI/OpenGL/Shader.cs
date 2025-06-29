using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Windows;
namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class Shader : IDisposable
    {
        public int Handle => _handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "UI", "OpenGL");

            string vertexShaderSource = File.ReadAllText(Path.Combine(basePath, vertexPath));
            string fragmentShaderSource = File.ReadAllText(Path.Combine(basePath, fragmentPath));

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            // bind the shader source code to the shader
            GL.ShaderSource(vertexShader, vertexShaderSource);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            // bind the shader source code to the shader


            GL.ShaderSource(fragmentShader, fragmentShaderSource);

            // compile shaders
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) // failure
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                MessageBox.Show(infoLog);
            }

            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0) // failure
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                MessageBox.Show(infoLog);
            }

            // link shaders into a program run on GPU
            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);
            GL.LinkProgram(_handle);

            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) // failure
            {
                string infoLog = GL.GetProgramInfoLog(_handle);
                MessageBox.Show(infoLog);
            }

            // keep _handle, cleanup the rest
            GL.DetachShader(_handle, vertexShader);
            GL.DetachShader(_handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }


        public void Use()
        {
            GL.UseProgram(_handle);
        }


        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                GL.DeleteProgram(_handle);
                _disposedValue = true;
            }
        }

        ~Shader()
        {
            // We don't want to call Dispose(false) in the finalizer, since during GC it may be too late to dispose the shader here;
            // Instead, Dispose() should be always explicitly called and this finalizer is supposed to be suppressed, so here we just check that
            if (!_disposedValue)
            {
                MessageBox.Show("GPU resource leak! Did you forget to call Dispose()?");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion


        private readonly int _handle;
        private bool _disposedValue;
    }
}
