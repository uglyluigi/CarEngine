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

        private static uint DebugCubeVAO = 0;

        private static Vec3 DebugCubeTheta = Vec3.Zero;

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
            //CenterMousePosition();
        }

        private void CenterMousePosition()
        {
            var (X, Y) = (Location.X, Location.Y);
            Cursor.Position = new(X + Width / 2, Y + Height / 2);
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

            //Camera.Position += CamVelocity;

            Program.Use();
            // Set uniform state
            DebugCubeTheta += (0.0f, 0.3f, 0.0f);
            Camera.RotateMat = Matrix4x4f.RotatedY(DebugCubeTheta.Y);
            Gl.UniformMatrix4f(Program.LocationMVP, 1, false, Camera.MVP_DEBUG);
            Gl.UniformMatrix4f(Program.WorldScaleMat, 1, false, Matrix4x4f.Identity);
            // Use the vertex array
            //Model.Draw(Program);
            DrawDebugCube();
        }




        private void RenderControl_CreateGL320()
        {
            Program = new ShaderProgram("shaders/vertex_debug.glsl", "shaders/fragment_debug.glsl");
            //Model = new Model("models/cube.obj");
            // Model.LoadModel();

            SetupDebugCube();
        }

        private void DrawDebugCube()
        {
            Gl.BindVertexArray(DebugCubeVAO);
            Gl.DrawElements(PrimitiveType.Triangles, 6 * 2 * 3, DrawElementsType.UnsignedShort, null);
            Gl.BindVertexArray(0);
        }

        #region DebugCube

        readonly float[] CubeVertices = 
        [
            // Front face
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,

            // Back face
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
        ];

        readonly float[] CubeColors =
        [
            1.0f, 0.4f, 0.6f,
            1.0f, 0.9f, 0.2f,
            0.7f, 0.3f, 0.8f,
            0.5f, 0.3f, 1.0f,

            0.2f, 0.6f, 1.0f,
            0.6f, 1.0f, 0.4f,
            0.6f, 0.8f, 0.8f,
            0.4f, 0.8f, 0.8f,
        ];

        readonly ushort[] CubeTriangleIndices = 
        [
            // Front
            0, 1, 2,
            2, 3, 0,

            // Right
            0, 3, 7,
            7, 4, 0,

            // Bottom
            2, 6, 7,
            7, 3, 2,

            // Left
            1, 5, 6,
            6, 2, 1,

            // Back
            4, 7, 6,
            6, 5, 4,

            // Top
            5, 1, 0,
            0, 4, 5,
        ];

        readonly float[] CubeNormals =
        [
            0, 0, -1,
            1, 0, 0,
            0, -1, 0,
            -1, 0, 0,
            0, 0, 1,
            0, 1, 0
        ];

        private void SetupDebugCube()
        {
            DebugCubeVAO = Gl.GenVertexArray();
            Gl.BindVertexArray( DebugCubeVAO );

            uint CubeVBO = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, CubeVBO);
            Gl.BufferData(BufferTarget.ArrayBuffer, Convert.ToUInt32(sizeof(float) * CubeVertices.Length), CubeVertices, BufferUsage.StaticDraw);

            uint IndicesBufferObj = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBufferObj);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, Convert.ToUInt32(sizeof(ushort) * CubeTriangleIndices.Length), CubeTriangleIndices, BufferUsage.StaticDraw);


            // Position attrib
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, 0);
            Gl.EnableVertexAttribArray(0);


            Gl.BindVertexArray(0);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        #endregion

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