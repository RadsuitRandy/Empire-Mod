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
                PawnGroupMaker combat = new PawnGroupMaker {kindDef = PawnGroupKindDefOf.Combat};
                PawnGroupMaker trader = new PawnGroupMaker
                {
                    kindDef = PawnGroupKindDefOf.Trader
                };
                foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(kind => kind.RaceProps.packAnimal))
                {
                    trader.carriers.Add(new PawnGenOption { kind = pawnKindDef, selectionWeight = 1 });
                }
                
                PawnGroupMaker settlement = new PawnGroupMaker {kindDef = PawnGroupKindDefOf.Settlement};
                PawnGroupMaker peaceful = new PawnGroupMaker {kindDef = PawnGroupKindDefOf.Peaceful};
                foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(
                    def => def.race.race.intelligence == Intelligence.Humanlike && def.race.BaseMarketValue != 0
                        && def.race.label == thingDef.label))
                {
                    PawnGenOption type = new PawnGenOption { kind = def, selectionWeight = 1 };
                    settlement.options.Add(type);
                    if (def.label != "mercenary")
                    {
                        trader.options.Add(type);
                        peaceful.options.Add(type);
                    }

                    if (def.isFighter)
                    {
                        trader.guards.Add(type);
                        combat.options.Add(type);
                    }

                    if (def.trader)
                    {
                        trader.traders.Add(type);
                    }
                }
                
                faction.pawnGroupMakers.Add(combat);
                faction.pawnGroupMakers.Add(trader);
                faction.pawnGroupMakers.Add(settlement);
                faction.pawnGroupMakers.Add(peaceful);
            }
            else
            {
                int index = faction.pawnGroupMakers.FindIndex(
                    groupMaker => groupMaker.options.Find(
                        type => type.kind.race.label.Equals(thingDef.label)) != null);
                if (index >= 0)
                {
                    faction.pawnGroupMakers.RemoveAt(index);
                    if (!FactionColonies.getPlayerColonyFaction().TryGenerateNewLeader())
                    {
                        Log.Error("Couldn't generate new leader! " + FactionColonies.getPlayerColonyFaction().def.pawnGroupMakers.Count);
                    }
                }
            }

            base.SetAllow(thingDef, allow);
        }
    }
}