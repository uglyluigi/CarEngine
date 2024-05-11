using ChungusEngine.Vector;
using Khronos;
using OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace ChungusEngine
{

    public delegate void MouseMovedEvent();

    public class MouseEventMessageFilter : IMessageFilter
    {
        private const int WM_MOUSEMOVE = 0x0200;

        public event MouseMovedEvent? TheMouseMoved;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE)
            {
                TheMouseMoved?.Invoke();
            }

            return false;
        }
    }

    public partial class SampleForm : Form
    {
        private static Camera Camera = new();

        private static Model Model;
        private static ShaderProgram Program;

        private static Vec3 CamVelocity = Vec3.Zero;
        private static readonly float MoveFactor = 0.05f;
        private static (uint X, uint Y, uint Z) AxisGridVAOs = new();

        private static (float X, float Y) MouseDelta = new();
        private static Point PrevCursorPos = Cursor.Position;

        /// <summary>
        /// Construct a SampleForm.
        /// </summary>
        public SampleForm()
        {
            KeyPreview = true;
            KeyDown += new(Form1_KeyDown);
            KeyUp += new(Form1_KeyUp);

            MouseEventMessageFilter filter = new();
            filter.TheMouseMoved += new(TheMouseMoved);
            Application.AddMessageFilter(filter);


            InitializeComponent();
        }

        private void TheMouseMoved()
        { 
            Point CursorPos = Cursor.Position;
            MouseDelta = (PrevCursorPos.X - CursorPos.X, PrevCursorPos.Y - CursorPos.Y);
            PrevCursorPos = CursorPos;
            Debug.WriteLine($"Mouse delta: {MouseDelta}");
            ApplyCameraRotation(MouseDelta);
        }

        private void ApplyCameraRotation((float X, float Y) mouseDelta)
        {
            const float sensitivity = 0.005f;

            var xAxis = System.Numerics.Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), mouseDelta.X * sensitivity);
            var yAxis = System.Numerics.Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), mouseDelta.Y * sensitivity);

            Camera.Rotation *= xAxis * yAxis;
            Debug.WriteLine("CameraRot: " + Camera.Rotation);
        }

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

            //Gl.Enable(EnableCap.DepthTest);
            // https://developer.nvidia.com/content/depth-precision-visualized
            Gl.ClipControl(ClipControlOrigin.LowerLeft, ClipControlDepth.ZeroToOne);
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            // Common GL commands
            Control senderControl = (Control)sender;

            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // apply velocity to camera

            Camera.Position += CamVelocity;
            //Camera.View *= Matrix4x4f.RotatedX(Theta.X) * Matrix4x4f.RotatedY(Theta.Y) * Matrix4x4f.RotatedZ(Theta.Z);

            Program.Use();
            // Set uniform state
            Gl.UniformMatrix4f(Program.LocationMVP, 1, false, Camera.MVP);
            Gl.UniformMatrix4f(Program.WorldScaleMat, 1, false, Matrix4x4f.Identity);
            // Use the vertex array
            Model.Draw(Program);
        }

        private void RenderControl_CreateGL320()
        {
            Program = new ShaderProgram("shaders/vertex.glsl", "shaders/fragment.glsl");
            Model = new Model("models/cube.obj");
            Model.LoadModel();
        }

        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {
        }

        public static void PrepareAxisGrid()
        {
            float[] Y_AXIS = [0, -300, 0, 0, 300, 0];
            float[] X_AXIS = [-300, 0, 0, 300, 0, 0];
            float[] Z_AXIS = [0, 0, -300, 0, 0, 300];

            var (X_VAO, Y_VAO, Z_VAO) = (Gl.GenVertexArray(), Gl.GenVertexArray(), Gl.GenVertexArray());
            var (X_VBO, Y_VBO, Z_VBO) = (Gl.GenBuffer(), Gl.GenBuffer(), Gl.GenBuffer());

            int VERTEX_SIZE = Marshal.SizeOf(typeof(Vertex));
            uint BUF_SIZE = (uint)(2 * VERTEX_SIZE);

            Gl.BindVertexArray(X_VAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, X_VBO); 
            Gl.BufferData(BufferTarget.ArrayBuffer, BUF_SIZE, X_AXIS, BufferUsage.StaticDraw);

            Gl.BindVertexArray(Y_VAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, Y_VBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, BUF_SIZE, Y_AXIS, BufferUsage.StaticDraw);

            Gl.BindVertexArray(Z_VAO);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, Z_VBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, BUF_SIZE, Z_AXIS, BufferUsage.StaticDraw);

            AxisGridVAOs = (X_VAO, Y_VAO, Z_VAO);

            Gl.BindVertexArray(0);
        }

        public static void DrawAxisGrid()
        {
            Gl.BindVertexArray(AxisGridVAOs.X);
            Gl.DrawArrays(PrimitiveType.Lines, 0, 1);
            Gl.BindVertexArray(AxisGridVAOs.Y);
            Gl.DrawArrays(PrimitiveType.Lines, 0, 1);
            Gl.BindVertexArray(AxisGridVAOs.Z);
            Gl.DrawArrays(PrimitiveType.Lines, 0, 1);
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
                case Keys.S:
                    // we add these here because we want
                    // to be able to strafe if multiple
                    // keys are pressed
                    CamVelocity += (0.0f, MoveFactor, 0.0f);
                    break;
                case Keys.A:
                    CamVelocity += (MoveFactor, 0.0f, 0.0f);
                    break;
                case Keys.W:
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
                case Keys.S:
                    CamVelocity *= (1.0f, 0.0f, 1.0f);
                    break;
                case Keys.A:
                    CamVelocity *= (0.0f, 1.0f, 1.0f);
                    break;
                case Keys.W:
                    CamVelocity *= (1.0f, 0.0f, 1.0f);
                    break;
                case Keys.D:
                    CamVelocity *= (0.0f, 1.0f, 1.0f);
                    break;
                case Keys.E:
                    CamVelocity *= (1.0f, 1.0f, 0.0f);
                    break;
                case Keys.Q:
                    CamVelocity *= (1.0f, 1.0f, 0.0f);
                    break;
            }
        }


        private void Form1_MouseDown(object? sender, MouseEventArgs e)
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