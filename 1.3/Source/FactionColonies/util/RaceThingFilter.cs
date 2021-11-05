using System;
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

            if (AllowedDefCount == 0)
            {
                SetAllow(DefDatabase<PawnKindDef>.AllDefsListForReading.First(def => def.IsHumanlikeWithLabelRace()).race, true);
            }

            foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(kind =>
                kind.RaceProps.packAnimal))
            {
                faction.pawnGroupMakers[1].carriers.Add(new PawnGenOption { kind = pawnKindDef, selectionWeight = 1 });
            }

            List<string> races = new List<string>();
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanlikeWithLabelRace() && !races.Contains(def.race.label) && AllowedThingDefs.Contains(def.race)))
            {
                if (def.race.label == "Human" && def.LabelCap != "Colonist") continue;
                races.Add(def.race.label);
                SetAllow(def.race, true);
            }

            WorldSettlementTraderTracker.reloadTraderKind();
        }

        private IEnumerable<PawnKindDef> DefaultList => DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanLikeRace() && AllowedThingDefs.Contains(def.race) && def.defaultFactionType != null && def.defaultFactionType.defName != "Empire");
        private IEnumerable<PawnKindDef> PawnKindDefsForTechLevel(TechLevel techLevel) => DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanLikeRace() && AllowedThingDefs.Contains(def.race) && def.defaultFactionType != null && def.defaultFactionType.defName != "Empire" && def.defaultFactionType.techLevel == techLevel);

        private bool FactionProbablyNotGeneratedYet => AllowedThingDefs.Count() == 0 || factionFc.techLevel == TechLevel.Undefined;
        private IEnumerable<PawnKindDef> CheckAndFixWorkList(IEnumerable<PawnKindDef> workList)
        {
            if (FactionProbablyNotGeneratedYet) return DefaultList;

            List<TechLevel> triedLevels = new List<TechLevel>();

            TechLevel tempLevel = factionFc.techLevel;
            while (!workList.Any() && tempLevel > TechLevel.Undefined)
            {
                triedLevels.Add(tempLevel);
                tempLevel -= 1;
                workList = PawnKindDefsForTechLevel(tempLevel);
            }

            if (!workList.Any())
            {
                triedLevels.Add(tempLevel);
                tempLevel = factionFc.techLevel + 1;
                workList = PawnKindDefsForTechLevel(tempLevel);
            }

            while (!workList.Any() && tempLevel <= TechLevel.Archotech)
            {
                triedLevels.Add(tempLevel);
                tempLevel += 1;
                workList = PawnKindDefsForTechLevel(tempLevel);
            }

            if (!workList.Any())
            {
                Messages.Message("noPawnKindDefOfRaceError".Translate(string.Join(", ", AllowedThingDefs)), MessageTypeDefOf.RejectInput);
                Log.Error("noPawnKindDefOfRaceError".Translate(string.Join(", ", AllowedThingDefs)));
                workList = DefaultList;
            }
            else if (triedLevels.Count != 0)
            {
                Log.Warning("noPawnKindDefOfRaceWarning".Translate(string.Join(", ", triedLevels), string.Join(", ", AllowedThingDefs), tempLevel.ToString()));
            }
            return workList;
        }

        private IEnumerable<PawnKindDef> GenerateIfMissing(IEnumerable<PawnKindDef> workList, Func<PawnKindDef, bool> predicate, string missingLabel)
        {
            if (FactionProbablyNotGeneratedYet) return DefaultList;

            List<TechLevel> triedLevels = new List<TechLevel>();

            TechLevel tempLevel = factionFc.techLevel;
            while (!workList.Any(predicate) && tempLevel > TechLevel.Undefined)
            {
                triedLevels.Add(tempLevel);
                tempLevel -= 1;
                workList = workList.Concat(PawnKindDefsForTechLevel(tempLevel).Where(predicate));
            }

            if (!workList.Any(predicate))
            {
                triedLevels.Add(tempLevel);
                tempLevel = factionFc.techLevel + 1;
                workList = workList.Concat(PawnKindDefsForTechLevel(tempLevel).Where(predicate));
            }

            while (!workList.Any(predicate) && tempLevel <= TechLevel.Archotech)
            {
                triedLevels.Add(tempLevel);
                tempLevel += 1;
                workList = workList.Concat(PawnKindDefsForTechLevel(tempLevel).Where(predicate));
            }

            if (!workList.Any(predicate))
            {
                Messages.Message("noPawnKindDefOfTypeOfRaceError".Translate(missingLabel, string.Join(", ", AllowedThingDefs)), MessageTypeDefOf.RejectInput);
                Log.Error("noPawnKindDefOfTypeOfRaceError".Translate(missingLabel, string.Join(", ", AllowedThingDefs)));
                workList.Concat(DefaultList.Where(predicate));
            }
            else if (triedLevels.Count != 0)
            {
                Log.Warning("noPawnKindDefOfTypeOfRaceWarning".Translate(missingLabel, string.Join(", ", triedLevels), string.Join(", ", AllowedThingDefs), tempLevel.ToString()));
            }
            return workList;
        }

        private void RefreshPawnGroupMakers()
        {
            IEnumerable<PawnKindDef> workList = PawnKindDefsForTechLevel(factionFc.techLevel);
            workList = CheckAndFixWorkList(workList) ?? new List<PawnKindDef>();
            workList = GenerateIfMissing(workList, def => def.trader, "Trader");
            workList = GenerateIfMissing(workList, def => def.isFighter, "Fighter");
            workList = GenerateIfMissing(workList, def => def.label != "mercenary", "Non Mercenary");

            //0 = combat, 1 = trader, 2 = settlement, 3 = peaceful
            foreach (PawnKindDef def in workList)
            {
                //Log.Message(def.defaultFactionType.techLevel.ToString() + " == " + factionFc.techLevel.ToString() + " = " + (def.defaultFactionType.techLevel == factionFc.techLevel));

                PawnGenOption type = new PawnGenOption { kind = def, selectionWeight = 1 };
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

        public new bool SetAllow(ThingDef thingDef, bool allow)
        {
            base.SetAllow(thingDef, allow);
            if (faction == null)
            {
                faction = DefDatabase<FactionDef>.GetNamed("PColony");
            }

            if (allow)
            {
                RefreshPawnGroupMakers();
            }
            else
            {
                faction.pawnGroupMakers.ForEach(groupMaker =>
                {
                    groupMaker.options.RemoveAll(
                        type => type.kind.race.label.Equals(thingDef.label));
                    groupMaker.traders.RemoveAll(
                        type => type.kind.race.label.Equals(thingDef.label));
                    groupMaker.guards.RemoveAll(
                        type => type.kind.race.label.Equals(thingDef.label));
                });


                WorldSettlementTraderTracker.reloadTraderKind();
                RefreshPawnGroupMakers();

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
            
            return true;
        }
    }
}