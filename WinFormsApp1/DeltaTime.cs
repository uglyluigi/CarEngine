using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChungusEngine
{
    internal class DeltaTime
    {
        // The previous system time a frame was rendered.
        private static DateTime TimeSinceLastDt = DateTime.Now;

        /**
         * With respect to the above comment,
         * returns the time that has elapsed
         * since the previous frame was rendered.
         *          
         * Measured in SECONDS (hence the / 1000)
         * 
         * It's useful to have it measured in seconds
         * because it allows you to specify things like
         * velocities and accelerations in more easily
         * visualizeable ways and units. This is because
         * we already have units that are expressed over
         * the period of 1 second. (m/s anyone?)
         * 
         * For example, is it easier to visualize a
         * cube moving at 10e-5 units per frame,
         * (which can be different for identical
         * periods of time if the frame rate changes)
         * or 10 units per second?
         */
        public static float Dt()
        {
            var CurrentTime = DateTime.Now;
            var Dt = CurrentTime.Ticks - TimeSinceLastDt.Ticks;
            TimeSinceLastDt = CurrentTime;
            return Dt;
        }
    }
}
