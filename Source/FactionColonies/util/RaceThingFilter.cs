using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace FactionColonies.util
{
    public class RaceThingFilter : ThingFilter
    {
        private FactionDef faction;

        public RaceThingFilter()
        {
        }

        //Useless parameter to only reset when reset instead of when loaded
        public RaceThingFilter(bool reset)
        {
            faction = DefDatabase<FactionDef>.GetNamed("PColony");
            faction.pawnGroupMakers = new List<PawnGroupMaker>();

            List<string> races = new List<string>();
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def =>
                def.race.race.intelligence == Intelligence.Humanlike &
                !races.Contains(def.race.label) && def.race.BaseMarketValue != 0))
            {
                if (def.race.label == "Human" && def.LabelCap != "Colonist") continue;
                Log.Message("Adding " + def.race.label);
                races.Add(def.race.label);
                SetAllow(def.race, true);
            }
        }

        public new void SetAllow(ThingDef thingDef, bool allow)
        {
            if (faction == null)
            {
                faction = DefDatabase<FactionDef>.GetNamed("PColony");
            }

            if (allow)
            {
                PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Find(
                    def => def.race.race.intelligence == Intelligence.Humanlike && def.race.BaseMarketValue != 0
                        && def.race.label == thingDef.label);
                PawnGroupMaker maker = new PawnGroupMaker();
                PawnGenOption genOption = new PawnGenOption {kind = pawnKind};
                maker.options.Add(genOption);
                if (genOption.kind.trader)
                {
                    maker.traders.Add(genOption);
                }

                if (genOption.kind.isFighter)
                {
                    maker.guards.Add(genOption);
                }

                faction.pawnGroupMakers.Add(maker);
            }
            else
            {
                int index = faction.pawnGroupMakers.FindIndex(
                    groupMaker => groupMaker.options.Find(
                        type => type.kind.race.label.Equals(thingDef.label)) != null);
                if (index >= 0)
                {
                    Log.Message("Removing " + thingDef.label);
                    faction.pawnGroupMakers.RemoveAt(index);
                    FactionColonies.getPlayerColonyFaction().TryGenerateNewLeader();
                }
            }
            Log.Message("Size: " + faction.pawnGroupMakers.Count);

            base.SetAllow(thingDef, allow);
        }

    }
}