using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Xml;

namespace FactionColonies
{
    public class BiomeResourceDef : Def, IExposable
    {
        public List<double> BaseProductionAdditive = new List<double>();
        public List<double> BaseProductionMultiplicative = new List<double>();
        public bool canSettle;

        public BiomeResourceDef()
        {

        }

        public void ExposeData()
        {
            Scribe_Collections.Look<double>(ref BaseProductionAdditive, "BaseProductionAdditive", LookMode.Value);
            Scribe_Collections.Look<double>(ref BaseProductionAdditive, "BaseProductionMultiplicative", LookMode.Value);
            Scribe_Values.Look<bool>(ref canSettle, "canSettle");
        }
    }


    [DefOf]
    public class BiomeResourceDefOf
    {
        public static BiomeResourceDef defaultBiome;
        static BiomeResourceDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BiomeResourceDefOf));
        }
    }
}
