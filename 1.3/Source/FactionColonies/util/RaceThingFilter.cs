using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FactionColonies.util
{
    public class RaceThingFilter : ThingFilter
    {
        private FactionDef faction;
        private FactionFC factionFc;
        private MilitaryCustomizationUtil militaryUtil;

        public RaceThingFilter()
        {
        }

        //Useless parameter to only reset when reset instead of when loaded
        public RaceThingFilter(FactionFC factionFc)
        {
            this.factionFc = factionFc;
            militaryUtil = factionFc.militaryCustomizationUtil;
            faction = DefDatabase<FactionDef>.GetNamed("PColony");
            
        }

        public void FinalizeInit(FactionFC factionFc)
        {
            this.factionFc = factionFc;
            militaryUtil = factionFc.militaryCustomizationUtil;
            faction = DefDatabase<FactionDef>.GetNamed("PColony");

            faction.pawnGroupMakers = new List<PawnGroupMaker>
            {
                new PawnGroupMaker
                {
                    kindDef = PawnGroupKindDefOf.Combat
                },
                new PawnGroupMaker
                {
                    kindDef = PawnGroupKindDefOf.Trader
                },
                new PawnGroupMaker
                {
                    kindDef = PawnGroupKindDefOf.Settlement
                },
                new PawnGroupMaker
                {
                    kindDef = PawnGroupKindDefOf.Peaceful
                }
            };

            foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(kind =>
                kind.RaceProps.packAnimal))
            {
                faction.pawnGroupMakers[1].carriers.Add(new PawnGenOption { kind = pawnKindDef, selectionWeight = 1 });
            }

            List<string> races = new List<string>();
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanlikeWithLabelRace() && !races.Contains(def.race.label) && AllowedThingDefs.Any(thingDef => thingDef.label == def.race.label)))
            {
                if (def.race.label == "Human" && def.LabelCap != "Colonist") continue;
                races.Add(def.race.label);
                SetAllow(def.race, true);
            }

            WorldSettlementTraderTracker.reloadTraderKind();
        }

        public new bool SetAllow(ThingDef thingDef, bool allow)
        {
            if (faction == null)
            {
                faction = DefDatabase<FactionDef>.GetNamed("PColony");
            }

            if (allow)
            {
                //0 = combat, 1 = trader, 2 = settlement, 3 = peaceful
                foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanLikeRace() && def.race.label == thingDef.label))
                {
                    if (def.defaultFactionType == null || def.defaultFactionType.defName == "Empire") continue;

                    PawnGenOption type = new PawnGenOption {kind = def, selectionWeight = 1};
                    faction.pawnGroupMakers[2].options.Add(type);
                    if (def.label != "mercenary")
                    {
                        faction.pawnGroupMakers[1].options.Add(type);
                        faction.pawnGroupMakers[3].options.Add(type);
                    }

                    if (def.isFighter)
                    {
                        faction.pawnGroupMakers[1].guards.Add(type);
                        faction.pawnGroupMakers[0].options.Add(type);
                    }

                    if (def.trader)
                    {
                        faction.pawnGroupMakers[1].traders.Add(type);
                    }
                }
            }
            else
            {
                faction.pawnGroupMakers.ForEach(
                    groupMaker =>
                    {
                        groupMaker.options.RemoveAll(
                            type => type.kind.race.label.Equals(thingDef.label));
                        groupMaker.traders.RemoveAll(
                            type => type.kind.race.label.Equals(thingDef.label));
                        groupMaker.guards.RemoveAll(
                            type => type.kind.race.label.Equals(thingDef.label));
                    });

                if (!faction.pawnGroupMakers[1].traders.Any() || !faction.pawnGroupMakers[0].options.Any() ||
                    !faction.pawnGroupMakers[3].options.Any())
                {
                    SetAllow(thingDef, true);
                    return false;
                }

                WorldSettlementTraderTracker.reloadTraderKind();
                if (WorldSettlementTraderTracker.BaseTraderKinds == null
                    || !WorldSettlementTraderTracker.BaseTraderKinds.Any())
                {
                    SetAllow(thingDef, true);
                    return false;
                }
                
                base.SetAllow(thingDef, false);
                foreach (MercenarySquadFC mercenarySquadFc in militaryUtil.mercenarySquads)
                {
                    List<Mercenary> newMercs = new List<Mercenary>();
                    foreach (Mercenary mercenary in mercenarySquadFc.mercenaries)
                    {
                        if (!Allows(mercenary.pawn.kindDef.race))
                        {
                            Mercenary merc = mercenary;
                            mercenarySquadFc.createNewPawn(ref merc,
                                faction.pawnGroupMakers[0].options.RandomElement().kind);
                            newMercs.Add(merc);
                        }
                        else
                        {
                            newMercs.Add(mercenary);
                        }
                    }

                    mercenarySquadFc.mercenaries = newMercs;
                }

                return true;
            }
            
            base.SetAllow(thingDef, allow);
            return true;
        }
    }
}