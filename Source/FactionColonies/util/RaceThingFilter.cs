using System.Collections.Generic;
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
        }
        
        public new void SetAllow(ThingDef thingDef, bool allow)
        {
            if (faction == null)
            {
                faction = DefDatabase<FactionDef>.GetNamed("PColony");
            }
            
            if (allow)
            {
                PawnGroupMaker maker = new PawnGroupMaker();
                PawnKindDef pawnKindDef = new PawnKindDef {race = thingDef};
                PawnGenOption genOption = new PawnGenOption {kind = pawnKindDef};
                maker.options.Add(genOption);
                faction.pawnGroupMakers.Add(maker);
            }
            else
            {
                int index = faction.pawnGroupMakers.FindIndex(
                    groupMaker => groupMaker.options.Find(
                        type => type.kind.race.Equals(thingDef)) != null);
                if (index >= 0)
                {
                    faction.pawnGroupMakers.RemoveAt(index);
                }
            }

            foreach (PawnGroupMaker maker in faction.pawnGroupMakers)
            {
                Log.Message("Found maker " + maker.options[0].kind.race);
            }
            base.SetAllow(thingDef, allow);
        }
    }
}