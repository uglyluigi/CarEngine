using ChungusEngine.Vector;
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

        enum ControlMode
        {
            RotX,
            RotY,
            RotZ
        }

        private static Camera Camera;

        private static Model model;
        private static ShaderProgram program;
        private static ControlMode Mode = ControlMode.RotX;
        private static bool ApplyRotation = false;
        private static float ThetaX = 0.0f, ThetaY = 0.0f, ThetaZ = 0.0f;

        private static Vec3 CamVelocity = Vec3.Zero;
        private static readonly float MoveFactor = 5.0f;

        #region Constructors

        /// <summary>
        /// Construct a SampleForm.
        /// </summary>
        public SampleForm()
        {
            KeyPreview = true;
            KeyDown += new(Form1_KeyDown);
            KeyUp += new(Form1_KeyUp);

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

            // apply velocity to camera

            Camera.Position += CamVelocity;
            Camera.ModelView *= Matrix4x4f.RotatedX(ThetaX) * Matrix4x4f.RotatedY(ThetaY) * Matrix4x4f.RotatedZ(ThetaZ);

            program.Use();
            // Set uniform state
            Gl.UniformMatrix4f(program.LocationMVP, 1, false, Camera.MVP);
            // Use the vertex array
            model.Draw(program);

        }

        private void RenderControl_CreateGL320()
        {
            Camera = new Camera();
            program = new ShaderProgram("shaders/vertex.glsl", "shaders/fragment.glsl");
            model = new Model("models/cube.obj");
            model.LoadModel();
        }

        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {
            const float RotFactor = 0.1f;

            if (ApplyRotation)
            {
                switch (Mode)
                {
                    case ControlMode.RotX:
                        ThetaX += RotFactor;
                        break;
                    case ControlMode.RotY:
                        ThetaY += RotFactor;
                        break;
                    case ControlMode.RotZ:
                        ThetaZ += RotFactor;
                        break;
                }
            }
        }

        private void RenderControl_ContextDestroying(object sender, GlControlEventArgs e)
        {

        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.X:
                    Mode = ControlMode.RotX;
                    Debug.WriteLine("Rot: X");
                    break;
                case Keys.Y:
                    Mode = ControlMode.RotY;
                    Debug.WriteLine("Rot: Y");

                    break;
                case Keys.Z:
                    Mode = ControlMode.RotZ;
                    Debug.WriteLine("Rot: Z");
                    break;
                case Keys.R:
                    ThetaX = 0.0f;
                    ThetaY = 0.0f;
                    ThetaZ = 0.0f;
                    break;
                case Keys.Space:
                    ApplyRotation = true;
                    break;
                case Keys.W:
                    // we add these here because we want
                    // to be able to strafe if multiple
                    // keys are pressed
                    CamVelocity += (0.0f, MoveFactor, 0.0f);
                    break;
                case Keys.A:
                    CamVelocity += (MoveFactor, 0.0f, 0.0f);
                    break;
                case Keys.S:
                    CamVelocity += (0.0f, -MoveFactor, 0.0f);
                    break;
                case Keys.D:
                    CamVelocity += (-MoveFactor, 0.0f, 0.0f);
                    break;
                case Keys.E:
                    CamVelocity += (0.0f, 0.0f, MoveFactor);
                    break;
                case Keys.Q:
                    CamVelocity += (0.0f, 0.0f, -MoveFactor);
                    break;
            }

        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    CamVelocity *= (0.0f, 1.0f, 1.0f);
                    break;
                case Keys.S:
                    CamVelocity *= (1.0f, 0.0f, 1.0f);
                    break;
                case Keys.D:
                    CamVelocity *= (0.0f, 1.0f, 1.0f);
                    break;
                case Keys.E:
                    CamVelocity *= (1.0f, 1.0f, 0.0f);
                    break;
                case Keys.Q:
                    CamVelocity *= (0.0f, 0.0f, 1.0f);
                    break;
                case Keys.Space:
                    ApplyRotation = false;
                    break;
            }
        }


        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            ApplyRotation = e.Button == MouseButtons.Left;
            Debug.WriteLine($"{ApplyRotation}");
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