using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace FactionColonies.util
{
    static class TicksExtensions
    {
        public static string ToTimeString(this int ticks)
        {
            string minutes = "" + ticks / 3600;
            string seconds = "" + ticks % 3600 / 60;

            while (minutes.Length < 2) minutes = "0" + minutes;
            while (seconds.Length < 2) seconds = "0" + seconds;

            if (ticks < 60000) return minutes + ":" + seconds + " mins:secs";
            return GenDate.ToStringTicksToDays(ticks);
        }

    }
}
