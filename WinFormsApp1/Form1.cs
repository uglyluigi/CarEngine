using Khronos;
using OpenGL;
using System.Diagnostics;
using System.Text;

namespace ChungusEngine
{


    /// <summary>
    /// Sample drawing a simple, rotating and colored triangle.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// - OpenGL 3.2
    /// </remarks>
    public partial class SampleForm : Form
    {
        private static Model model;
        private static ShaderProgram program;

        #region Constructors

        /// <summary>
        /// Construct a SampleForm.
        /// </summary>
        public SampleForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Allocate GL resources or GL states.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="object"/> that has rasied the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="GlControlEventArgs"/> that specifies the event arguments.
        /// </param>
        private void RenderControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            GlControl glControl = (GlControl)sender;

            // GL Debugging
            if (Gl.CurrentExtensions != null && Gl.CurrentExtensions.DebugOutput_ARB)
            {
                Gl.DebugMessageCallback(GLDebugProc, IntPtr.Zero);
                Gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }

            RenderControl_CreateGL320();


            // Uses multisampling, if available
            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
                Gl.Enable(EnableCap.Multisample);
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            // Common GL commands
            Control senderControl = (Control)sender;

            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            RenderControl_RenderGL320();
        }

        private void RenderControl_CreateGL320()
        {
            program = new ShaderProgram("shaders/vertex.glsl", "shaders/fragment.glsl");
            model = new Model("models/backpack/backpack.obj");
            model.LoadModel();
        }

        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {

        }

        private void RenderControl_ContextDestroying(object sender, GlControlEventArgs e)
        {

        }

        private static void GLDebugProc(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string strMessage;

            // Decode message string
            unsafe
            {
                strMessage = Encoding.ASCII.GetString((byte*)message.ToPointer(), length);
            }

            Console.WriteLine($"{source}, {type}, {severity}: {strMessage}");
            Debug.WriteLine($"{source}, {type}, {severity}: {strMessage}");
        }

        #endregion

        #region Common Shading

        // Note: abstractions for drawing using programmable pipeline.

        /// <summary>
        /// Shader object abstraction.
        /// </summary>
        private class Object : IDisposable
        {
            public Object(ShaderType shaderType, string[] source)
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));

                // Create
                ShaderName = Gl.CreateShader(shaderType);
                // Submit source code
                Gl.ShaderSource(ShaderName, source);
                // Compile
                Gl.CompileShader(ShaderName);
                // Check compilation status
                int compiled;

                Gl.GetShader(ShaderName, ShaderParameterName.CompileStatus, out compiled);
                if (compiled != 0)
                    return;

                // Throw exception on compilation errors
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetShaderInfoLog(ShaderName, logMaxLength, out infologLength, infolog);

                throw new InvalidOperationException($"unable to compile shader: {infolog}");
            }

            public readonly uint ShaderName;

            public void Dispose()
            {
                Gl.DeleteShader(ShaderName);
            }
        }

        /// <summary>
        /// Buffer abstraction.
        /// </summary>
        private class Buffer : IDisposable
        {
            public Buffer(float[] buffer)
            {
                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));

                // Generate a buffer name: buffer does not exists yet
                BufferName = Gl.GenBuffer();
                // First bind create the buffer, determining its type
                Gl.BindBuffer(BufferTarget.ArrayBuffer, BufferName);
                // Set buffer information, 'buffer' is pinned automatically
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * buffer.Length), buffer, BufferUsage.StaticDraw);
            }

            public readonly uint BufferName;

            public void Dispose()
            {
                Gl.DeleteBuffers(BufferName);
            }
        }

        #endregion

        #region Shaders

        private void RenderControl_RenderGL320()
        {
            // Compute the model-view-projection on CPU
            Matrix4x4f projection = Matrix4x4f.Ortho2D(-1.0f, +1.0f, -1.0f, +1.0f);
            Matrix4x4f modelview = Matrix4x4f.Translated(-0.5f, -0.5f, 0.0f) * Matrix4x4f.RotatedZ(_Angle);

            program.Use();

            // Set uniform state
            Gl.UniformMatrix4f(program.LocationMVP, 1, false, projection * modelview);
            // Use the vertex array
            model.Draw(program);
        }

        #endregion

        #region Common Data

        private static float _Angle;

        #endregion
    }
}