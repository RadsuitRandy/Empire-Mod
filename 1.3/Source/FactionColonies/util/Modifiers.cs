using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FactionColonies.util
{
    static class Modifiers
    {
        public static int GetModifier
        {
            get
            {
                return 1 * (Event.current.shift ? 5 : 1) * (Event.current.control ? 10 : 1);
            }
        }
    }
}
