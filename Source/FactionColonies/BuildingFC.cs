using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace FactionColonies
{
    public class BuildingFCDef : Def, IExposable
    {

        public BuildingFCDef()
        {
        }

        public void ExposeData()
        {

            Scribe_Values.Look<string>(ref desc, "desc");
            Scribe_Values.Look<double>(ref cost, "cost");
            Scribe_Values.Look<TechLevel>(ref techLevel, "techLevel");
            Scribe_Values.Look<int>(ref constructionDuration, "constructionDuration");
            Scribe_Collections.Look<FCTraitEffectDef>(ref traits, "traits", LookMode.Def);
            Scribe_Collections.Look<string>(ref applicableBiomes, "applicableBiomes", LookMode.Value);
            Scribe_Values.Look<int>(ref upkeep, "upkeep");
            Scribe_Values.Look<string>(ref iconPath, "iconPath");
        }

        public string desc;
        public double cost;
        public int constructionDuration;
        public TechLevel techLevel = TechLevel.Undefined;
        public List<FCTraitEffectDef> traits;
        public List<string> applicableBiomes = new List<string>();
        public int upkeep = 0;
        public string iconPath = "GUI/unrest";
        public Texture2D iconLoaded;
        //public required research

        public Texture2D icon
        {
            get
            {
                if (this.iconLoaded == null)
                {
                    if (!this.iconPath.NullOrEmpty()) {
                        this.iconLoaded = ContentFinder<Texture2D>.Get(this.iconPath, true);
                    } else
                    {
                        Log.Message("Failed to load icon");
                        this.iconLoaded = TexLoad.questionmark;
                    }
                }
                return this.iconLoaded;
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
