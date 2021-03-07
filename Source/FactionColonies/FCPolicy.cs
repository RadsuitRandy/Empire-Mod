using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace FactionColonies
{
    public enum FCPolicyCategory : byte
    {
        Undefined = 0,
        Trait = 1,
        Core = 2,
        Tax = 3,
        Military = 4
    }

    public class FCPolicy : IExposable
    {
        public FCPolicy()
        {

        }
        public FCPolicy( FCPolicyDef def)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            this.def = def;
            this.timeEnacted = Find.TickManager.TicksGame;


            //Road Builder Trait
            if (def == FCPolicyDefOf.roadBuilders)
            {
                ResearchProjectDef researchdef = DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingDirt", false);
                if (researchdef == null)
                    Log.Message("Empire Error - Road research returned Null");
                if (!(Find.ResearchManager.GetProgress(researchdef) == researchdef.baseCost))
                {
                    Find.ResearchManager.FinishProject(researchdef);
                }

            }

            //Mercantile Trait
            if (def == FCPolicyDefOf.mercantile)
            {
                faction.resetTraitMercantileCaravanTime();
            }
        }

        public FCPolicyDef def;
        public int timeEnacted;

        public void ExposeData()
        {
            Scribe_Defs.Look<FCPolicyDef>(ref def, "def");
            Scribe_Values.Look<int>(ref timeEnacted, "timeEnacted");
            
        }


    }

    public class FCPolicyDef : Def, IExposable
    {

        public FCPolicyDef()
        {
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref factionLevelRequirement, "factionLevelRequirement");
            Scribe_Values.Look<TechLevel>(ref techLevelRequirement, "techLevelRequirement");
            Scribe_Values.Look<string>(ref desc, "desc");
            Scribe_Values.Look<FCPolicyCategory>(ref category, "category");
            Scribe_Collections.Look<string>(ref positiveEffects, "positiveEffects", LookMode.Value);
            Scribe_Collections.Look<string>(ref negativeEffects, "negativeEffects", LookMode.Value);
        }

        public string desc;
        public FCPolicyCategory category;
        public TechLevel techLevelRequirement;
        public int factionLevelRequirement;
        public List<string> positiveEffects;
        public List<string> negativeEffects;

        public Texture2D IconLight
        {
            get
            {
                switch (defName)
                {
                    case "militaristic":
                        return texLoad.traitMilitaristicLight;
                    case "pacifist":
                        return texLoad.traitPacifistLight;
                    case "authoritarian":
                        return texLoad.traitAuthoritarianLight;
                    case "egalitarian":
                        return texLoad.traitEgalitarianLight;
                    case "isolationist":
                        return texLoad.traitIsolationistLight;
                    case "expansionist":
                        return texLoad.traitExpansionistLight;
                    case "technocratic":
                        return texLoad.traitTechnocraticLight;
                    case "feudal":
                        return texLoad.traitFeudalLight;
                    default:
                        Log.Message("Could not find icon for " + defName);
                        return null;
                }
            }
        }

        public Texture2D IconDark
        {
            get
            {
                switch (defName)
                {
                    case "militaristic":
                        return texLoad.traitMilitaristicDark;
                    case "pacifist":
                        return texLoad.traitPacifistDark;
                    case "authoritarian":
                        return texLoad.traitAuthoritarianDark;
                    case "egalitarian":
                        return texLoad.traitEgalitarianDark;
                    case "isolationist":
                        return texLoad.traitIsolationistDark;
                    case "expansionist":
                        return texLoad.traitExpansionistDark;
                    case "technocratic":
                        return texLoad.traitTechnocraticDark;
                    case "feudal":
                        return texLoad.traitFeudalDark;
                    default:
                        Log.Message("Could not find icon for " + defName);
                        return null;
                }
            }
        }
    }

    [DefOf]
    public class FCPolicyDefOf
    {
        //Faction Traits
        public static FCPolicyDef empty;

        public static FCPolicyDef resilient;
        public static FCPolicyDef raiders;
        public static FCPolicyDef defenseInDepth;
        public static FCPolicyDef industrious;
        public static FCPolicyDef roadBuilders;
        public static FCPolicyDef mercantile;
        public static FCPolicyDef innovative;
        public static FCPolicyDef lucky;

        //Policies
        public static FCPolicyDef militaristic;
        public static FCPolicyDef pacifist;
        public static FCPolicyDef authoritarian;
        public static FCPolicyDef egalitarian;
        public static FCPolicyDef isolationist;
        public static FCPolicyDef expansionist;
        public static FCPolicyDef technocratic;
        public static FCPolicyDef feudal;

        static FCPolicyDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FCPolicyDefOf));
        }
    }

}
