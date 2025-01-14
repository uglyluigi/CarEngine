using ChungusEngine.Graphics;
using OpenGL;
using System.Diagnostics;
using System.Numerics;

namespace ChungusEngine.Physics.Collision
{
    public class AxisAlignedBoundingBox
    {

        public Matrix4x4f Transform
        {
            get
            {
                if (BoundsCamera)
                {
                    //return WindowProvider.Camera.BuildRotationMatrix();
                }

                return Matrix4x4f.Identity;
            }
        }

        // This is set to true if the bounding box bounds the camera
        // so the vertex shader does not apply the view transform
        // and the visual properly tracks the position of the camera.
        public bool BoundsCamera { get; set; } = false;

        // Does the collision tester care if this box is being intersected?
        public bool Active { get; set; } = true;

        public AxisAlignedBoundingBox(Vector3 position, Vector3 halfExtents)
        {
            Position = position;
            HalfExtents = halfExtents;
        }

        private static readonly uint[] Indices = [
            0, 1, 2, 0, 2, 3,
            0, 3, 7, 0, 4, 7,
            2, 3, 7, 2, 6, 7,
            1, 2, 6, 1, 5, 6,
            1, 0, 4, 1, 5, 4,
            4, 5, 6, 4, 6, 7
        ];

        public Vector3 Position
        {
            get;
            set;
        }

        public Vector3 HalfExtents;

        private uint VAO;
        private uint VBO;
        private uint EBO;

        private bool Bound = false;

        private void Bind()
        {
            (VAO, VBO, EBO) = OGLServices.Buffer(GetVertices(), Indices);
        }

        public void Draw(ShaderProgram program)
        {
            if (!Bound)
            {
                Bind();
                Bound = true;
            }

            Gl.BindVertexArray(VAO);
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            Gl.UniformMatrix4f(program.ModelMatrix, 1, false, Transform);
            program.SetBool("ApplyViewTransform", !BoundsCamera);
            program.SetBool("DrawingAABB", true);
            Gl.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            program.SetBool("ApplyViewTransform", false);
            program.SetBool("DrawingAABB", false);
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            Gl.BindVertexArray(0);
        }

        private void Unbind()
        {
            Debug.WriteLine("Unbinding");
            Gl.DeleteVertexArrays(VAO);
            Gl.DeleteBuffers(VBO, EBO);
        }

        private Vertex[] GetVertices()
        {
            Vertex[] vertices = new Vertex[8];
            Vector3[] bounds;
            
            if (BoundsCamera)
            {
                bounds = CameraColliderBounds();
            }
            else
            {
                bounds = Bounds();
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var (x, y, z) = (bounds[i].X, bounds[i].Y, bounds[i].Z);
                vertices[i] = new Vertex(new Vector3(x, y, z), Vector3.Zero, Vector2.Zero);
            }

            return vertices;
        }

        public static Vector3[] Bounds(Vector3 halfExtents, Vector3 position)
        {
            Vector3[] points = new Vector3[8];
            var (x, y, z) = (halfExtents.X, halfExtents.Y, halfExtents.Z);
            var (cx, cy, cz) = (position.X, position.Y, position.Z);

            // Upper plane is defined in clockwise order with points 0-3
            // points 4-7 
            points[0] = new(cx + x, cy + y, cz + z);
            points[1] = new(cx + x, cy + y, cz - z);
            points[2] = new(cx - x, cy + y, cz - z);
            points[3] = new(cx - x, cy + y, cz + z);
            points[4] = new(cx + x, cy - y, cz + z);
            points[5] = new(cx + x, cy - y, cz - z);
            points[6] = new(cx - x, cy - y, cz - z);
            points[7] = new(cx - x, cy - y, cz + z);

            return points;
        }

        public Vector3[] Bounds()
        {
            //if (BoundsCamera) return Bounds(HalfExtents, WindowProvider.Camera.InvertedPosition);
            return Bounds(HalfExtents, Position);
        }

        private Vector3[] CameraColliderBounds()
        {
            return Bounds(HalfExtents, Vector3.Zero);
        }

        public bool IsPointBound(Vector3 p)
        {
            var x = p.X;
            var y = p.Y;
            var z = p.Z;

            var maxes = GetMaxes();
            var minimums = GetMinimums();

            // see if it is less than the maxes AND more than the mins
            // maxes are in indices 0-3
            // mins are in 4-7

            // If the point is below the 3rd point in the bounds
            // or above the first, it is outside of the box
            if (x < minimums.x || x > maxes.x) return false;
            if (y < minimums.y || y > maxes.y) return false;
            if (z < minimums.z || z > maxes.z) return false;

            return true;
        }

        public (float x, float y, float z) GetMaxes()
        {
            float[] maxes = new float[3];

            foreach (var v in Bounds())
            {
                var (x, y, z) = (v.X, v.Y, v.Z);

                if (x > maxes[0]) maxes[0] = x;
                if (y > maxes[1]) maxes[1] = y;
                if (z > maxes[2]) maxes[2] = z;
            }

            return (maxes[0], maxes[1], maxes[2]);
        }

        public (float x, float y, float z) GetMinimums()
        {
            float[] mins = new float[3];

            foreach (var v in Bounds())
            {
                var (x, y, z) = (v.X, v.Y, v.Z);

                if (x < mins[0]) mins[0] = x;
                if (y < mins[1]) mins[1] = y;
                if (z < mins[2]) mins[2] = z;
            }

            return (mins[0], mins[1], mins[2]);
        }

        /**
         * Returns true if this AABB is completely bound by the
         * supplied AABB.
         */
        public bool IsCompletelyBoundBy(AxisAlignedBoundingBox box)
        {
            foreach (var v in Bounds())
            {
                if (!box.IsPointBound(v))
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Returns true if any point in this AABB is bound by the supplied box.
         */
        public bool IsAnyPointBoundBy(AxisAlignedBoundingBox box)
        {
            foreach (var v in Bounds())
            {
                if (box.IsPointBound(v))
                {
                    Debug.WriteLine($"Point {v} @ {Position} bound by box centered @ {box.Position} {string.Join(",", box.Bounds())}");
                    return true;
                }
            }

            return false;
        }
    }
}
