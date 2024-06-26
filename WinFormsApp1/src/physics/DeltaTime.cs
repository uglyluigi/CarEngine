using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine.Physics
{
    internal class DeltaTime
    {
        // The previous system time a frame was rendered.
        private static DateTime TimeSinceLastDt = DateTime.Now;

        public static void Update()
        {
            TimeSinceLastDt = DateTime.Now;
        }

        /**
         * Number of seconds since the last frame was rendered by the OpenGL context.
         */
        public static float Dt { get { return 1.0f * (DateTime.Now.Ticks - TimeSinceLastDt.Ticks) / TimeSpan.TicksPerSecond; } }
    }
}
