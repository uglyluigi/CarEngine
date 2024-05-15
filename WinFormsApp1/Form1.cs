using ChungusEngine.Vector;
using Khronos;
using OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Quaternion = System.Numerics.Quaternion;

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

        private static ShaderProgram Program;

        private static (uint X, uint Y, uint Z) AxisGridVAOs = new();

        private static (float X, float Y) MouseDelta = new();
        private static Point PrevCursorPos = Cursor.Position;


        private static List<SimpleModel> ModelList = new();


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
            ApplyCameraRotation(MouseDelta);
        }

        private void CenterMousePosition()
        {
            var (X, Y) = (Location.X, Location.Y);
            Cursor.Position = new(X + Width / 2, Y + Height / 2);
        }

        private void ApplyCameraRotation((float X, float Y) mouseDelta)
        {
            const float sensitivity = 0.005f;

            // Users are more used to X-axis movements on the mouse corresponding to horizontal rotation
            // and Y-axis movements corresponding to vertical rotation.
            // Originally, the x-Axis quaternion was created wrt. the X-axis and the mouse's change in X.
            // with the Y-axis the same way, except with the mouse's Y-delta.
            // This actually produces an inverted rotation where X movements move the cube up and down (really,
            // down and up) and Y-movements move it left and right (really right and left). So, I swapped them around
            // and now the rotation works as I expect. I should really figure out how these things work
            // to avoid issues later.
            var xAxis = Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), -mouseDelta.Y * sensitivity);
            var yAxis = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), -mouseDelta.X * sensitivity);

            Camera.Rotation *= xAxis * yAxis;
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

            Program = new ShaderProgram("shaders/vertex_debug.glsl", "shaders/fragment_debug.glsl");

            // Uses multisampling, if available
            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
                Gl.Enable(EnableCap.Multisample);

            //Gl.Enable(EnableCap.DepthTest);
            // https://developer.nvidia.com/content/depth-precision-visualized
            Gl.ClipControl(ClipControlOrigin.LowerLeft, ClipControlDepth.ZeroToOne);
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            ModelList = GetModels();
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            Control senderControl = (Control)sender;

            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Program.Use();
            // Set uniform state

            Gl.UniformMatrix4f(Program.ViewMatrix, 1, false, Matrix4x4f.Identity);
            Gl.UniformMatrix4f(Program.ProjectionMatrix, 1, false, Matrix4x4f.Translated(0.0f, 0.0f, -3.0f) * Camera.Perspective());

            foreach (var Model in ModelList)
            {
                Model.DrawModel(Program);
            }
        }


        private static List<SimpleModel> GetModels()
        {
            var list = new List<SimpleModel>();

            float[] vertices = [
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

            float[] colors = [
                1.0f, 0.4f, 0.6f,
                1.0f, 0.9f, 0.2f,
                0.7f, 0.3f, 0.8f,
                0.5f, 0.3f, 1.0f,

                0.2f, 0.6f, 1.0f,
                0.6f, 1.0f, 0.4f,
                0.6f, 0.8f, 0.8f,
                0.4f, 0.8f, 0.8f,
            ];

            ushort[] indices = [
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

            list.Add(new SimpleModel("Cube 1", Matrix4x4f.Identity, vertices, colors, indices));
            list.Add(new SimpleModel("Cube 2", Matrix4x4f.Identity, vertices, colors, indices));

            return list;
        }


        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {
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

        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
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