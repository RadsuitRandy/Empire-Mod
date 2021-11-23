using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Text;
using System.Threading.Tasks;

namespace FactionColonies.util
{
    /// <summary>
    /// Arguably only generates short names so far, but should contain everything "random text" in future
    /// </summary>
    static class TextGen
    {
        public static string ToShortName(string name)
        {
            IEnumerable<string> strings = name.Split(' ').Where(str => !str.NullOrEmpty() && char.IsUpper(str[0]));
            string seed = strings.First();

            return strings.Aggregate(seed, (total, next) => total + ((seed == next) ? ' ' : next[0]));
        }
    }
}
