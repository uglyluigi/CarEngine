﻿using ChungusEngine.Vector;
using Khronos;
using OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Quaternion = System.Numerics.Quaternion;

namespace ChungusEngine
{
    public enum _Direction
    {
        UP,
        DOWN
    }

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

        private static Point PrevCursorPos = Cursor.Position;
        private bool CareAboutMouseDelta = false;


        private static List<SimpleModel> ModelList = [];
        private readonly MouseEventMessageFilter filter = new();

        private readonly Model BackpackModel = new Model("models/cube.obj", new(0.0f, 0.0f, -10.0f));
        private readonly Model BackpackModel2 = new Model("models/cube.obj", new(0.0f, 5.0f, -10.0f));
        private static bool ItDoBeRotating = false;


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

        private void TheMouseMoved()
        { 
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
            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
                Gl.Enable(EnableCap.Multisample);

            //Gl.Enable(EnableCap.DepthTest);
            // https://developer.nvidia.com/content/depth-precision-visualized
            Gl.ClipControl(ClipControlOrigin.LowerLeft, ClipControlDepth.ZeroToOne);
            //Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            //ModelList = GetModels();
            //CenterMousePosition();
            BackpackModel.LoadModel();
            BackpackModel2.LoadModel();
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            Control senderControl = (Control)sender;


            Gl.Viewport(0, 0, senderControl.ClientSize.Width, senderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Program.Use();
            // Set uniform state

            Gl.UniformMatrix4f(Program.ViewMatrix, 1, false, Camera.View());
            Gl.UniformMatrix4f(Program.ProjectionMatrix, 1, false, Matrix4x4f.Translated(0.0f, -1.0f, -5.0f) * Camera.Perspective());

            filter.TheMouseMoved += new(TheMouseMoved);
            Application.AddMessageFilter(filter);

            
            BackpackModel.Draw(Program);
            BackpackModel2.Draw(Program);

            if (ItDoBeRotating)
            {
                BackpackModel.Rotation = Quaternion.Normalize(Quaternion.Slerp(Quaternion.Normalize(BackpackModel.Rotation), Quaternion.Normalize(-BackpackModel.Rotation * Util.J), 0.05f));
                Debug.WriteLine(BackpackModel.Rotation.ToString());
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

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D)
            {
                Camera.HandleKeyboardInput(e, _Direction.DOWN);
            } else if (e.KeyCode is Keys.Space)
            {
                ItDoBeRotating = true;
            }
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.W or Keys.A or Keys.S or Keys.D)
            {
                Camera.HandleKeyboardInput(e, _Direction.UP);
            } else if (e.KeyCode is Keys.Space)
            {
                ItDoBeRotating = false;
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
    }
}