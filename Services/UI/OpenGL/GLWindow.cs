using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BauphysikToolWPF.Services.Application;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    internal partial class GLWindow : GameWindow
    {
        #region WPF Integration
        
        /// <summary>
        /// GLWindow has a rendering loop that is inherited from its parent class GameWindow - the Run method - which makes its calling thread busy.
        /// That means the thread cannot be the same as the main thread, which is the WPF UI thread.
        /// The window is therefore created and run in a Task that acquires a background thread from the thread pool.
        /// To be able to do this, we need to turn off a flag (property) in OpenTK: GLFWProvider.CheckForMainThread = false,
        /// otherwise, we get an exception when creating the window.
        /// We do this in a static method, along with creating and running the task:
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static GLWindow? CreateAndRun(IntPtr hWndParent, Rect bounds)
        {
            GLFWProvider.CheckForMainThread = false;
            using var mres = new ManualResetEventSlim(false);
            GLWindow? glWnd = null;

            _GLTask = Task.Run(() => // this is the task running the GLWindow in a background thread from the thread pool
            {
                glWnd = new GLWindow(hWndParent, ((int)bounds.X, (int)bounds.Y), ((int)bounds.Width, (int)bounds.Height));
                mres.Set(); // signal the main thread that window was created
                using (glWnd)
                {
                    glWnd?.Run();
                }
            });
            mres.Wait(); // wait on signal of window creation from task
            return glWnd;
        }

        /// <summary>
        /// GLWindow is the subclass of OpenTK's GameWindow responsible for the OpenGL rendering.
        /// invokes the base class version passing in some settings via NativeWindowSettings: the initial location and size of the window, which are received from the main window
        /// </summary>
        /// <param name="hWndParent"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        private GLWindow(IntPtr hWndParent, Vector2i location, Vector2i size)
            : base(GameWindowSettings.Default,
                  new NativeWindowSettings
                  {
                      Location = location,
                      ClientSize = size,
                      WindowBorder = WindowBorder.Hidden,
                  })
        {
            var osVer = Environment.OSVersion;
            unsafe  // requires checking unsafe option in project settings
            {
                GLFW.HideWindow(WindowPtr);
                IntPtr ptr = GLFW.GetWin32Window(WindowPtr);

                uint childStyle = Interop.GetWindowLong(ptr, Interop.GWL_STYLE);
                childStyle |= Interop.WS_CHILD;
                childStyle &= ~Interop.WS_POPUP;
                _ = Interop.SetWindowLong(ptr, Interop.GWL_STYLE, childStyle);

                uint exStyle = osVer.Version.Major >= 10 ?
                    Interop.WS_EX_TOOLWINDOW | Interop.WS_EX_LAYERED | Interop.WS_EX_TRANSPARENT :
                    Interop.WS_EX_TOOLWINDOW;
                _ = Interop.SetWindowLong(ptr, Interop.GWL_EXSTYLE, exStyle);
                _ = Interop.SetParent(ptr, hWndParent);
                _ = Interop.EnableWindow(ptr, false); // no keyboard and mouse input

                // Since TransparentFramebuffer is ineffective for child window, using win32 layered window but only for Windows 10+
                if (osVer.Version.Major >= 10)
                {
                    _ = Interop.SetLayeredWindowAttributes(ptr,
                        0x00000000, // 00bbggrr - must match the rgb of GL.ClearColor() to be transparent
                        0, // used with LWA_ALPHA, but it would affect WHOLE window (all drawing)
                        Interop.LWA_COLORKEY); // use the passed color key
                }
                GLFW.ShowWindow(WindowPtr);
            }
            _xpos = location.X;
            _ypos = location.Y;
        }

        #region Communicating with GLWindow

        // Called when the main thread decides to close this window, which completes the task running it
        public void Cleanup()
        {
            EnqueueCommand(() =>
            {
                Close();
            });
            // wait for task to complete the Close and exit
            _GLTask?.Wait();
        }


        public void SetBoundingBox(Rect bounds)
        {
            EnqueueCommand(() =>
            {
                ClientLocation = ((int)bounds.X, (int)bounds.Y);
                ClientSize = ((int)bounds.Width, (int)bounds.Height);
                _xpos = bounds.X; _ypos = bounds.Y;
            });
        }


        public void MoveBy(int deltaX, int deltaY)
        {
            EnqueueCommand(() =>
            {
                _xpos += deltaX; _ypos += deltaY;
                ClientLocation = ((int)_xpos, (int)_ypos);
            });
        }


        public void Show()
        {
            EnqueueCommand(() =>
            {
                unsafe
                {
                    GLFW.ShowWindow(WindowPtr);
                }
            });
        }


        public void Hide()
        {
            EnqueueCommand(() =>
            {
                unsafe
                {
                    GLFW.HideWindow(WindowPtr);
                }
            });
        }


        public void ToggleSpin()
        {
            EnqueueCommand(() =>
            {
                _isSpinStopped = !_isSpinStopped;
            });
        }


        private void EnqueueCommand(Action command) =>
            _commands.Enqueue(command);


        private bool TryDequeueCommand([MaybeNullWhen(returnValue: false)] out Action command) =>
            _commands.TryDequeue(out command);

        #endregion


        // our task for running GLWindow in its own thread
        private static Task? _GLTask;


        // record the position of this window at any time
        private double _xpos;
        private double _ypos;

        // our queue used to send commands from the main window's thread to this window's thread
        private readonly ConcurrentQueue<Action> _commands = new();

        #endregion

        #region Tutorial part

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.5f);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _shader = new Shader("shader.vert", "shader.frag");
            _shader.Use();
        }


        protected override void OnUnload()
        {
            // cleanup the OpenGL objects
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);

            _shader?.Dispose();

            base.OnUnload();
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            while (TryDequeueCommand(out Action? command))
            {
                command();
            }
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (_shader is null)
            {
                return;
            }

            _shader.Use();

            SetRotationDegrees(args.Time);
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)_rotationDegrees));
            Matrix4 transform = rotation * _scale * _translate;
            int transformLocation = GL.GetUniformLocation(_shader.Handle, "aTransform");
            GL.UniformMatrix4(transformLocation, true, ref transform);

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }


        private void SetRotationDegrees(double deltaTime)
        {
            if (_isSpinStopped)
            {
                return;
            }
            // the triangle should rotate 20 degrees every second; based on the time elapsed since last frame,
            // calculate the rotation amount
            _rotationDegrees += deltaTime * 20.0;
            _rotationDegrees -= Math.Truncate(_rotationDegrees / 360.0) * 360.0; // take remainder of div by 360
        }

        // the amount of triangle rotation at each frame
        private double _rotationDegrees;

        private bool _isSpinStopped = true;

        // OpenGL objects
        private readonly float[] _vertices =
        {
             // positions        // colors
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
             0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
        };
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader? _shader;

        #endregion

        #region Bonus Part

        // Scale the whole window
        public void Scale(float scaleFactor, Point center, double parentWidth, double parentHeight)
        {
            EnqueueCommand(() =>
            {
                double width = ClientSize.X * scaleFactor;
                double height = ClientSize.Y * scaleFactor;
                if (width > height)
                {
                    height = width * parentHeight / parentWidth;
                }
                else if (width < height)
                {
                    width = height * parentWidth / parentHeight;
                }
                if (width > parentWidth * 2.0 || width < 20.0 || height > parentHeight * 2.0 || height < 20.0)
                {
                    // put some limit on the zoom 
                    return;
                }
                double xdist = _xpos - center.X;
                double ydist = _ypos - center.Y;
                _xpos += xdist * scaleFactor - xdist;
                _ypos += ydist * scaleFactor - ydist;
                ClientLocation = ((int)_xpos, (int)_ypos);
                ClientSize = ((int)width, (int)height);
            });
        }

        // --------------------------------------------------------------------------------------------------------------
        // The correct way of panning and zooming is by applying transform matrices to the content - something like below
        // --------------------------------------------------------------------------------------------------------------

        public void MoveContentBy(double deltaX, double deltaY)
        {
            EnqueueCommand(() =>
            {
                // convert from WPF coordinates to OpenGL coordinates
                _translateX += deltaX / ClientSize.X * 2.0;
                _translateY += -deltaY / ClientSize.Y * 2.0;
                _translate = Matrix4.CreateTranslation((float)_translateX, (float)_translateY, 0f);
            });
        }


        public void ScaleContent(float scaleFactor)
        {
            EnqueueCommand(() =>
            {
                float newFactor = _totalScaleFactor * scaleFactor;
                if (newFactor > 3f || newFactor < 0.1f)
                {
                    // put some limit on the zoom
                    return;
                }
                _totalScaleFactor = newFactor;
                _scale = Matrix4.CreateScale(_totalScaleFactor);
            });
        }


        public void ResetContent()
        {
            EnqueueCommand(() =>
            {
                _totalScaleFactor = 1f;
                _translateX = _translateY = 0.0;
                _scale = _translate = Matrix4.Identity;
            });
        }


        private float _totalScaleFactor = 1f;
        private double _translateX, _translateY;
        private Matrix4 _scale = Matrix4.Identity;
        private Matrix4 _translate = Matrix4.Identity;

        #endregion
    }
}
