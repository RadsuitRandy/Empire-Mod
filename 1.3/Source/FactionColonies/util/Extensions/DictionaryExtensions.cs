using System.Collections.Generic;
using Verse;

namespace FactionColonies.util
{
    static class DictionaryExtensions
    {
        public static string ToLetterString<TKey,TValue>(this Dictionary<TKey, TValue> dic)
        {
            string returnString = "";
            foreach(KeyValuePair<TKey, TValue> keyValuePair in dic)
            {
                returnString += keyValuePair.Key.ToString() + " " + keyValuePair.Value.ToString() + "x";
            }

            return returnString;
        }
    }
}
