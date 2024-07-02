using ChungusEngine.Graphics;
using ChungusEngine.Physics;
using ChungusEngine.WinForms;
using Khronos;
using OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Quaternion = System.Numerics.Quaternion;
using ChungusEngine.UsefulStuff;

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

    public partial class WindowProvider : Form
    {
        //TODO move this
        public static Camera Camera = new();
        private static ShaderProgram Program;
        private static MouseEventMessageFilter MouseFilter = new();
        public List<GameObject> GameObjects = [];

        public WindowProvider()
        {
            KeyPreview = true;

            MouseFilter.TheMouseMoved += TheMouseMoved;
            Application.AddMessageFilter(MouseFilter);

            InitializeComponent();
        }

        private void TheMouseMoved()
        {
            if (ActiveForm != null)
            {
                var CursorPos = Cursor.Position;
                var WindowRectangle = RectangleToScreen(ClientRectangle);
                // Some cool magic numbers that I need to add to compensate for some weird
                // missing space
                // The 31px makes sense because the top bar is 32px. Not sure where the last one went tho
                // Anyway this code basically imposes a 2d coordinate plane with its origin centered at
                // the center of the Forms window. 
                var Vec = new Vector2(CursorPos.X + 8 - WindowRectangle.X - Width / 2, CursorPos.Y - WindowRectangle.Y + 31 - Height / 2);
                // This updates the bingus rotation based on the mouse vector.
                Camera.UpdateCameraRotation(Vec);
                var (X_N, Y_N) = (Location.X, Location.Y);
                Cursor.Position = new(X_N + Width / 2, Y_N + Height / 2);
            }
        }

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
            if (Gl.CurrentVersion?.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
                Gl.Enable(EnableCap.Multisample);

            // https://developer.nvidia.com/content/depth-precision-visualized
            Gl.ClipControl(ClipControlOrigin.LowerLeft, ClipControlDepth.ZeroToOne);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Less);
            Gl.DepthMask(true);

            StbImageSharp.StbImage.stbi_set_flip_vertically_on_load(1);
            StbImageSharp.StbImage.stbi_set_flip_vertically_on_load_thread(1);

            GameObjects.Add(new GameObject("models/backpack/backpack.obj", Quaternion.Identity, new Vector3(0.0f, 0.0f, -10.0f), new Vector3(6.0f, 6.0f, 6.0f)));
            //GameObjects.Add(new GameObject("models/floor.obj", Quaternion.Identity, new Vector3(0.0f, -5.0f, 0.0f)));
            // Update the delta time provider,
            // now that everything is set up.
            DeltaTime.Update();
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            Control senderControl = (Control)sender;


            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Program.Use();
            // Set uniform state

            Gl.UniformMatrix4f(Program.ViewMatrix, 1, false, Camera.View());
            Gl.UniformMatrix4f(Program.ProjectionMatrix, 1, false, Camera.Projection());
            Program.SetBool("ApplyViewTransform", true);

            // Check keyboard state every frame
            // Originally I was using the WinForms-supported
            // event-driven API but this made delta time calculations
            // impossible. Now it uses a Win32 function that is called
            // every frame.
            if (ActiveForm != null)
            {
                KeyboardPoller.PollAndHandleKeyboardState();
            }

            ModelRegistry.DrawAll(Program);

            Camera.BoundingBox.Draw(Program);

            foreach (var obj in GameObjects)
            {
                obj.AABB.Draw(Program);
            }

            DeltaTime.Update();
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

            list.Add(new SimpleModel("Cube 1", Matrix4x4f.Translated(0.6f, 0.0f, 0.0f), vertices, colors, indices));
            list.Add(new SimpleModel("Cube 2", Matrix4x4f.Translated(-0.6f, 0.0f, 0.0f), vertices, colors, indices));

            return list;
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
    }
}