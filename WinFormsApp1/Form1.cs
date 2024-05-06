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
        private static float angle = 0.0f;

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


            // Compute the model-view-projection on CPU
            Matrix4x4f projection = Matrix4x4f.Perspective(90.0f, (float)senderControl.ClientSize.Width / senderControl.ClientSize.Height, 0.1f, 100.0f);
            Matrix4x4f modelview = Matrix4x4f.Scaled(0.01f, 0.01f, 0.01f)  * Matrix4x4f.RotatedZ(angle);


            program.Use();

            // Set uniform state
            Gl.UniformMatrix4f(program.LocationMVP, 1, false, projection * modelview);
            // Use the vertex array
            model.Draw(program);
            
        }

        private void RenderControl_CreateGL320()
        {
            program = new ShaderProgram("shaders/vertex.glsl", "shaders/fragment.glsl");
            model = new Model("models/cube.obj");
            model.LoadModel();
        }

        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {
            angle += 0.01f;
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


    }
}