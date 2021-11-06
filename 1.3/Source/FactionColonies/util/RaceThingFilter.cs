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

        private bool HasMissingPawnKindDefTypes => !faction.pawnGroupMakers[1].traders.Any() || !faction.pawnGroupMakers[0].options.Any() || !faction.pawnGroupMakers[3].options.Any() || WorldSettlementTraderTracker.BaseTraderKinds == null || !WorldSettlementTraderTracker.BaseTraderKinds.Any();


        private static readonly List<PawnGroupMaker> emptyList = new List<PawnGroupMaker> 
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

        private enum MissingType
        {
            Undefined,
            FCTrader,
            FCFighter,
            FCNonMercenary
        }

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

            faction.pawnGroupMakers = emptyList.ListFullCopy();

            if (AllowedDefCount == 0)
            {
                SetAllow(DefDatabase<PawnKindDef>.AllDefsListForReading.First(def => def.IsHumanlikeWithLabelRace()).race, true);
            }

            foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(kind => kind.RaceProps.packAnimal))
            {
                faction.pawnGroupMakers[1].carriers.Add(new PawnGenOption { kind = pawnKindDef, selectionWeight = 1 });
            }

            List<string> races = new List<string>();
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanlikeWithLabelRace() && !races.Contains(def.race.label) && AllowedThingDefs.Contains(def.race)))
            {
                races.Add(def.race.label);
                SetAllow(def.race, true);
            }

            WorldSettlementTraderTracker.reloadTraderKind();
        }

        private IEnumerable<PawnKindDef> DefaultList => DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanLikeRace() && AllowedThingDefs.Contains(def.race) && def.defaultFactionType != null && def.defaultFactionType.defName != "Empire");
        private IEnumerable<PawnKindDef> PawnKindDefsForTechLevel(TechLevel techLevel) => DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.IsHumanLikeRace() && AllowedThingDefs.Contains(def.race) && def.defaultFactionType != null && def.defaultFactionType.defName != "Empire" && def.defaultFactionType.techLevel == techLevel);

        private bool FactionProbablyNotGeneratedYet => AllowedThingDefs.Count() == 0 || factionFc.techLevel == TechLevel.Undefined;

        private IEnumerable<PawnKindDef> GenerateIfMissing(IEnumerable<PawnKindDef> workList, Func<PawnKindDef, bool> predicate, MissingType type, ThingDef race)
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

            string missingLabel0 = ResolveTypeToLabel0(type);
            if (!workList.Any(predicate))
            {
                Messages.Message("noPawnKindDefOfTypeOfRaceError".Translate(missingLabel0, race.label.CapitalizeFirst()), MessageTypeDefOf.RejectInput);
                Log.Warning("noPawnKindDefOfTypeOfRaceError".Translate(missingLabel0, race.label.CapitalizeFirst()));
                workList.Concat(DefaultList.Where(predicate));
            }
            else if (triedLevels.Count != 0)
            {
                string missingLabel1 = ResolveTypeToLabel1(type);
                Log.Warning("noPawnKindDefOfTypeOfRaceWarning".Translate(missingLabel0, string.Join(", ", triedLevels), race.label.CapitalizeFirst(), missingLabel1, tempLevel.ToString()));
            }
            return workList;
        }

        private string ResolveTypeToLabel0(MissingType type)
        {
            if (type == MissingType.Undefined) return " ";
            return " " + type.ToString().Translate() + " ";
        }

        private string ResolveTypeToLabel1(MissingType type)
        {
            if (type == MissingType.Undefined) return " " + "FCPawns".Translate() + " ";
            return ResolveTypeToLabel0(type);
        }

        private void RefreshPawnGroupMakers()
        {
            IEnumerable<PawnKindDef> workList = PawnKindDefsForTechLevel(factionFc.techLevel);

            foreach (ThingDef race in AllowedThingDefs)
            {
                workList = GenerateIfMissing(workList, def => def.race == race, MissingType.Undefined, race);
                workList = GenerateIfMissing(workList, def => def.trader && def.race == race, MissingType.FCTrader, race);
                workList = GenerateIfMissing(workList, def => def.isFighter && def.race == race, MissingType.FCFighter, race);
                workList = GenerateIfMissing(workList, def => def.label != "mercenary" && def.race == race, MissingType.FCNonMercenary, race);
            }

            GeneratePawnGenOptions(workList);

            if (!HasMissingPawnKindDefTypes) return;

            Messages.Message("missingPawnKindDefsCriticalError".Translate(), MessageTypeDefOf.NegativeEvent);
            Log.Error("missingPawnKindDefsCriticalError".Translate());
            workList = GenerateIfMissing(workList, def => def.race == ThingDefOf.Human, MissingType.Undefined, ThingDefOf.Human);
            faction.pawnGroupMakers = emptyList.ListFullCopy();
            GeneratePawnGenOptions(workList);
        }

        private void GeneratePawnGenOptions(IEnumerable<PawnKindDef> workList)
        {
            foreach (PawnKindDef def in workList)
            {
                //Log.Message(def.defaultFactionType.techLevel.ToString() + " == " + factionFc.techLevel.ToString() + " = " + (def.defaultFactionType.techLevel == factionFc.techLevel));
                //0 = combat, 1 = trader, 2 = settlement, 3 = peaceful
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