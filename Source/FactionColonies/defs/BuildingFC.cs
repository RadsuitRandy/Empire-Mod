using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FactionColonies
{
    public class BuildingFCDef : Def, IExposable
    {
        public void ExposeData()
        {

            Scribe_Values.Look(ref desc, "desc");
            Scribe_Values.Look(ref cost, "cost");
            Scribe_Values.Look(ref techLevel, "techLevel");
            Scribe_Values.Look(ref constructionDuration, "constructionDuration");
            Scribe_Collections.Look(ref traits, "traits", LookMode.Def);
            Scribe_Collections.Look(ref applicableBiomes, "applicableBiomes", LookMode.Value);
            Scribe_Values.Look(ref upkeep, "upkeep");
            Scribe_Values.Look(ref iconPath, "iconPath");
        }

        public string desc;
        public double cost;
        public int constructionDuration;
        public TechLevel techLevel = TechLevel.Undefined;
        public List<FCTraitEffectDef> traits;
        public List<string> applicableBiomes = new List<string>();
        public int upkeep;
        public string iconPath = "GUI/unrest";
        public Texture2D iconLoaded;
        //public required research

        public Texture2D icon
        {
            get
            {
                if (iconLoaded == null)
                {
                    if (!iconPath.NullOrEmpty()) {
                        iconLoaded = ContentFinder<Texture2D>.Get(iconPath);
                    } else
                    {
                        Log.Message("Failed to load icon");
                        iconLoaded = TexLoad.questionmark;
                    }
                }
                return iconLoaded;
            }
        }


    }

    [DefOf]
    public class BuildingFCDefOf
    {
        public static BuildingFCDef Empty;
        public static BuildingFCDef Construction;
        public static BuildingFCDef artilleryOutpost;
        static BuildingFCDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BuildingFCDefOf));
        }
    }

}
