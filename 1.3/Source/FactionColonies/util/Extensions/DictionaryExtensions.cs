using System.Collections.Generic;
using Verse;

namespace FactionColonies.util
{
    static class ListExtensions
    {
        public static string ToLetterString(this List<Thing> list)
        {
            string returnString = "";
            Dictionary<string, int> thingCountDic = new Dictionary<string, int>();
            foreach (Thing thing in list)
            {
                if (thingCountDic.ContainsKey(thing.LabelCapNoCount))
                {
                    thingCountDic[thing.LabelCapNoCount] += thing.stackCount;
                }
                else
                {
                    thingCountDic.Add(thing.LabelCapNoCount, thing.stackCount);
                }
            }

            foreach (KeyValuePair<string, int> keyValuePair in thingCountDic)
            {
                returnString += keyValuePair.Key.ToString() + " " + keyValuePair.Value.ToString() + "x\n";
            }

            return returnString;
        }
    }
}
