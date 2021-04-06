using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace FactionColonies
{
    public class WorldSettlementFC : MapParent
    {
        public SettlementFC settlement;

        public List<Pawn> attackers;

        public List<Pawn> defenders;

        public List<CaravanSupporting> supporting = new List<CaravanSupporting>();

        public militaryForce defenderForce;

        public militaryForce attackerForce;

        public string Name
        {
            get
            {
                Log.Message("Got name " + settlement.name);
                return settlement.name;
            }
            set
            {
                Log.Message("Set name " + value);
                settlement.name = value;
            }
        }
        
        public override string Label => Name;

        public override void PostMake()
        {
            def.expandingIconTexture = "FactionIcons/" + Find.World.GetComponent<FactionFC>().factionIconPath;
            base.PostMake();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Collections.Look(ref attackers, "attackers", LookMode.Reference);
            Scribe_Collections.Look(ref defenders, "defenders", LookMode.Reference);
            Scribe_Collections.Look(ref supporting, "supporting", LookMode.Reference);
            Scribe_Deep.Look(ref defenderForce, "defenderForce");
            Scribe_Deep.Look(ref attackerForce, "attackerForce");
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            if (settlement.isUnderAttack)
            {
                Command_Action defend = new Command_Action();

                defend.defaultLabel = "DefendColony".Translate();
                defend.defaultDesc = "DefendColonyDesc".Translate();
                defend.icon = TexLoad.iconMilitary;
                defend.action = () => { startDefense(MilitaryUtilFC.returnMilitaryEventByLocation(Tile), 
                    () => { }); };
                gizmos.Add(defend);
            }

            return gizmos;
        }

        public void startDefense(FCEvent evt, Action after)
        {
            LongEventHandler.QueueLongEvent(() =>
                {
                    if (Map == null)
                    {
                        MapGenerator.GenerateMap(new IntVec3(70 + settlement.settlementLevel * 10,
                                1, 70 + settlement.settlementLevel * 10), this,
                            MapGeneratorDef, ExtraGenStepDefs).mapDrawer.RegenerateEverythingNow();
                    }

                    zoomIntoTile(evt);
                    after.Invoke();
                },
                "GeneratingMap", false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
        }

        public void zoomIntoTile(FCEvent evt)
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();

            Log.Message("Event: " + evt);
            if (Map.mapPawns.ColonistCount == 0)
            {
                evt.timeTillTrigger = Find.TickManager.TicksGame;
                militaryForce force = MilitaryUtilFC.returnDefendingMilitaryForce(evt);
                if (force == null)
                {
                    return;
                }

                foreach (Building building in Map.listerBuildings.allBuildingsColonist)
                {
                    FloodFillerFog.FloodUnfog(building.InteractionCell, Map);
                }

                List<Pawn> friendlies = generateFriendlies((float) defenderForce.forceRemaining * 100);

                if (friendlies.Any())
                {
                    CameraJumper.TryJump(new GlobalTargetInfo(friendlies[0]));
                }
            }

            if (Map.mapPawns.FreeColonists.Any())
            {
                CameraJumper.TryJump(new GlobalTargetInfo(Map.mapPawns.FreeColonists[0]));
            }
            else
            {
                CameraJumper.TryJump(new IntVec3(50, 0, 50), Map);
            }
        }

        public override void Notify_CaravanFormed(Caravan caravan)
        {
            List<Pawn> toFind = caravan.pawns.InnerListForReading.ListFullCopy();

            List<CaravanSupporting> foundCaravan = new List<CaravanSupporting>();
            foreach (Pawn found in caravan.pawns)
            {
                if (found.GetLord() != null)
                {
                    found.GetLord().ownedPawns.Remove(found);
                }

                foreach (CaravanSupporting caravanSupporting in
                    supporting.Where(caravanSupporting => caravanSupporting.pawns.Contains(found)))
                {
                    foundCaravan.Add(caravanSupporting);
                    toFind.Remove(found);
                    caravanSupporting.pawns.Remove(found);
                    break;
                }
            }

            foreach (CaravanSupporting caravanSupporting in foundCaravan.Where(caravanSupporting =>
                caravanSupporting.pawns.Find(pawn => !pawn.Downed &&
                                                          !pawn.Dead && !pawn.AnimalOrWildMan()) == null))
            {
                //Prevent removing while creating end battle caravans
                if (settlement.isUnderAttack)
                {
                    supporting.Remove(caravanSupporting);
                }
                /*It appears vanilla handles this automatically
                foreach (Pawn animal in caravanSupporting.supporting.FindAll(pawn => pawn.AnimalOrWildMan()))
                {
                    animal.holdingOwner = null;
                    animal.DeSpawn();
                    Find.WorldPawns.PassToWorld(animal);
                    caravan.pawns.TryAdd(animal);
                }*/
            }

            caravan.pawns.RemoveAll(pawn => toFind.Contains(pawn));
            IntVec3 enterCell = FindNearEdgeCell(Map);
            foreach (Pawn pawn in toFind)
            {
                IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(enterCell, Map);
                GenSpawn.Spawn(pawn, loc, Map, Rot4.Random);
            }

            if (!caravan.pawns.Any)
            {
                caravan.Destroy();
            }
            else
            {
                foreach (Pawn pawn in caravan.pawns)
                {
                    Map.reservationManager.ReleaseAllClaimedBy(pawn);
                }

                base.Notify_CaravanFormed(caravan);
            }
        }

        public static IntVec3 FindNearEdgeCell(Map map)
        {
            bool BaseValidator(IntVec3 x) => x.Standable(map) && !x.Fogged(map);
            Faction hostFaction = map.ParentFaction;
            if (CellFinder.TryFindRandomEdgeCellWith(x =>
            {
                if (!BaseValidator(x))
                    return false;
                if (hostFaction != null && map.reachability.CanReachFactionBase(x, hostFaction))
                    return true;
                return hostFaction == null && map.reachability.CanReachBiggestMapEdgeRoom(x);
            }, map, CellFinder.EdgeRoadChance_Neutral, out var result))
                return CellFinder.RandomClosewalkCellNear(result, map, 5);
            if (CellFinder.TryFindRandomEdgeCellWith(BaseValidator, map, CellFinder.EdgeRoadChance_Neutral, out result))
                return CellFinder.RandomClosewalkCellNear(result, map, 5);
            Log.Warning("Could not find any valid edge cell.");
            return CellFinder.RandomCell(map);
        }

        private List<Pawn> generateFriendlies(float points)
        {
            List<Pawn> friendlies;
            if (defenderForce.homeSettlement.militarySquad != null)
            {
                friendlies = defenderForce.homeSettlement.militarySquad.EquippedMercenaryPawns;
                foreach (Pawn friendly in friendlies)
                {
                    Map.mapPawns.RegisterPawn(friendly);
                }
            }
            else
            {
                IncidentParms parms = new IncidentParms();
                parms.target = Map;
                parms.faction =
                    FactionColonies.getPlayerColonyFaction();
                parms.generateFightersOnly = true;
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly;
                parms.points = IncidentWorker_Raid.AdjustedRaidPoints(points,
                    PawnsArrivalModeDefOf.EdgeWalkIn, parms.raidStrategy,
                    parms.faction, PawnGroupKindDefOf.Combat);
                friendlies = PawnGroupMakerUtility.GeneratePawns(
                    IncidentParmsUtility.GetDefaultPawnGroupMakerParms(
                        PawnGroupKindDefOf.Combat, parms, true)).ToList();
                if (!friendlies.Any())
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                }
            }

            int min = (70 + settlement.settlementLevel * 10) / 2;
            
            LordMaker.MakeNewLord(
                    FactionColonies.getPlayerColonyFaction(), new LordJob_DefendBase(FactionColonies.getPlayerColonyFaction(),
                        new IntVec3(min, 0, min)), Map, friendlies);
            
            foreach (Pawn friendly in friendlies)
            {
                IntVec3 loc;
                min -= 5 + settlement.settlementLevel;
                int size = 10 + settlement.settlementLevel * 2;
                CellFinder.TryFindRandomCellInsideWith(new CellRect(min, min, size, size),
                    x => x.Standable(Map), out loc);
                GenSpawn.Spawn(friendly, loc, Map, new Rot4());
                if (friendly.drafter == null)
                {
                    friendly.drafter = new Pawn_DraftController(friendly);
                }
                friendly.drafter.Drafted = true;
            }

            defenders = friendlies;

            return friendlies;
        }

        public void endAttack()
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            if (defenders.Any())
            {
                faction.addExperienceToFactionLevel(5f);
                //if winner is player
                Find.LetterStack.ReceiveLetter("DefenseSuccessful".Translate(),
                    "DefenseSuccessfulFull".Translate(settlement.name, attackerForce.homeFaction),
                    LetterDefOf.PositiveEvent, new LookTargets(this));
            }
            else
            {
                //get multipliers
                double happinessLostMultiplier =
                    (TraitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier",
                        settlement.traits, "multiply") * TraitUtilsFC.cycleTraits(new double(),
                        "happinessLostMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply"));
                double loyaltyLostMultiplier =
                    (TraitUtilsFC.cycleTraits(new double(), "loyaltyLostMultiplier", settlement.traits,
                        "multiply") * TraitUtilsFC.cycleTraits(new double(), "loyaltyLostMultiplier",
                        Find.World.GetComponent<FactionFC>().traits, "multiply"));

                int traitMuliplier = 1;
                if (faction.hasPolicy(FCPolicyDefOf.feudal))
                    traitMuliplier = 2;
                float traitProsperityMultiplier = 1;
                bool traitCanDestroyBuildings = true;
                if (faction.hasTrait(FCPolicyDefOf.resilient))
                {
                    traitProsperityMultiplier = .5f;
                    traitCanDestroyBuildings = false;
                }

                //if winner are enemies
                settlement.prosperity -= 20 * traitProsperityMultiplier;
                settlement.happiness -= 25 * happinessLostMultiplier;
                settlement.loyalty -= 15 * loyaltyLostMultiplier * traitMuliplier;

                string str = "DefenseFailureFull".Translate(settlement.name, attackerForce.homeFaction);


                for (int k = 0; k < 4; k++)
                {
                    int num = new IntRange(0, 10).RandomInRange;
                    if (num >= 7 && settlement.buildings[k].defName != "Empty" &&
                        settlement.buildings[k].defName != "Construction" && traitCanDestroyBuildings)
                    {
                        str += "\n" +
                               "BulidingDestroyedInRaid".Translate(settlement.buildings[k].label);
                        settlement.deconstructBuilding(k);
                    }
                }

                //level remover checker
                if (settlement.settlementLevel > 1 && traitCanDestroyBuildings)
                {
                    int num = new IntRange(0, 10).RandomInRange;
                    if (num >= 7)
                    {
                        str += "\n\n" + "SettlementDeleveledRaid".Translate();
                        settlement.delevelSettlement();
                    }
                }

                Find.LetterStack.ReceiveLetter("DefenseFailure".Translate(), str, LetterDefOf.Death,
                    new LookTargets(Find.WorldObjects.SettlementAt(settlement.mapLocation)));
            }

            if (defenderForce.homeSettlement != settlement)
            {
                //if not the home settlement defending
                if (defenders.Count >= 7)
                {
                    Find.LetterStack.ReceiveLetter("OverwhelmingVictory".Translate(),
                        "OverwhelmingVictoryDesc".Translate(), LetterDefOf.PositiveEvent);
                    defenderForce.homeSettlement.returnMilitary(true);
                }
                else
                {
                    defenderForce.homeSettlement.cooldownMilitary();
                }
            }

            //Must be done before creating caravans and deiniting map to prevent AI bugging out or
            //caravans being removed mid-loop
            settlement.isUnderAttack = false;
            Map.lordManager.lords.Clear();
            
            //Ignore any empty caravans
            foreach (CaravanSupporting caravanSupporting in supporting.Where(supporting => supporting.pawns.Find(
                pawn => !pawn.Downed && !pawn.Dead) != null))
            {
                CaravanFormingUtility.FormAndCreateCaravan(caravanSupporting.pawns.Where(pawn => pawn.Spawned), 
                    Faction.OfPlayer, settlement.mapLocation, settlement.mapLocation, -1);
            }

            supporting.Clear();

            while (Map.mapPawns.AllPawnsSpawned.Any())
            {
                Map.mapPawns.AllPawnsSpawned[0].DeSpawn();
            }
            
            Current.Game.tickManager.RemoveAllFromMap(Map);
            attackers = null;
            CameraJumper.TryJump(settlement.mapLocation);
            //Prevent player from zooming back into the settlement
            Current.Game.CurrentMap = Find.World.worldObjects.SettlementAt(
                Find.World.GetComponent<FactionFC>().capitalLocation).Map;
            Current.Game.DeinitAndRemoveMap(Map);
        }

        public bool checkDowned(Pawn downed)
        {
            int index = attackers.IndexOf(downed);
            if (index >= 0)
            {
                attackers.RemoveAt(index);
                if (!attackers.Any())
                {
                    endAttack();
                    return false;
                }

                return true;
            }

            index = defenders.IndexOf(downed);
            if (index >= 0)
            {
                defenders.RemoveAt(index);
                if (!defenders.Any())
                {
                    endAttack();
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    class FighterDied
    {
        static bool Prefix(Pawn __instance)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.Tile, Find.World.info.name);
            bool result = settlement?.worldSettlement.checkDowned(__instance) ?? true;
            if (!result && __instance.IsColonist)
            {
                __instance.SetFaction(FactionColonies.getPlayerColonyFaction());
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    class FighterDowned
    {
        static bool Prefix(Pawn_HealthTracker __instance)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.immunity.pawn.Tile, Find.World.info.name);
            return settlement?.worldSettlement.checkDowned(__instance.immunity.pawn) ?? true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    class PawnGizmos
    {
        static void Postfix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            List<Gizmo> output = __result.ToList();
            if (__instance.Faction.Equals(FactionColonies.getPlayerColonyFaction()))
            {
                Pawn_DraftController pawnDraftController = __instance.drafter;
                if (pawnDraftController == null)
                {
                    pawnDraftController = new Pawn_DraftController(__instance);
                    __instance.drafter = pawnDraftController;
                }
                Command_Toggle draftColonists = new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_ColonistDraft,
                    isActive = () => false,
                    toggleAction = () =>
                    {
                        if (pawnDraftController.pawn.Faction.Equals(Faction.OfPlayer)) return;
                        pawnDraftController.pawn.SetFaction(Faction.OfPlayer);
                        pawnDraftController.Drafted = true;
                    },
                    defaultDesc = "CommandToggleDraftDesc".Translate(),
                    icon = TexCommand.Draft,
                    turnOnSound = SoundDefOf.DraftOn,
                    groupKey = 81729172,
                    defaultLabel = "CommandDraftLabel".Translate()
                };
                if (pawnDraftController.pawn.Downed)
                    draftColonists.Disable("IsIncapped".Translate(
                        (NamedArgument) pawnDraftController.pawn.LabelShort,
                        (NamedArgument) pawnDraftController.pawn));
                draftColonists.tutorTag = "Draft";
                output.Add(draftColonists);
            }
            else if(__instance.Faction.Equals(Faction.OfPlayer))
            {
                foreach (Command_Toggle action in output.Where(gizmo => gizmo is Command_Toggle))
                {
                    if (action.hotKey != KeyBindingDefOf.Command_ColonistDraft)
                    {
                        continue;
                    }

                    SettlementFC settlementFc = Find.World.GetComponent<FactionFC>()
                        .getSettlement(__instance.Tile, Find.World.info.name);
                    if (settlementFc == null || !settlementFc.worldSettlement.defenders.Contains(__instance))
                    {
                        continue;
                    }

                    int index = output.IndexOf(action);
                    action.toggleAction = () =>
                    {
                        __instance.SetFaction(FactionColonies.getPlayerColonyFaction());
                    };
                    output[index] = action;
                    break;
                }
            }

            __result = output;
        }
    }

    [HarmonyPatch(typeof(Caravan), "GetGizmos")]
    class CaravanGizmos
    {
        static void Postfix(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.Tile, Find.World.info.name);
            if (settlement != null && settlement.isUnderAttack)
            {
                Command_Action defend = new Command_Action();

                defend.defaultLabel = "DefendColony".Translate();
                defend.defaultDesc = "DefendColonyDesc".Translate();
                defend.icon = TexLoad.iconMilitary;
                defend.action = () =>
                {
                    settlement.worldSettlement.startDefense(
                        MilitaryUtilFC.returnMilitaryEventByLocation(__instance.Tile), () =>
                    {
                        CaravanSupporting caravanSupporting = new CaravanSupporting();
                        List<Pawn> supporting = __instance.pawns.InnerListForReading.ListFullCopy();
                        caravanSupporting.pawns = supporting;
                        settlement.worldSettlement.supporting.Add(caravanSupporting);
                        if (!__instance.Destroyed)
                        {
                            __instance.Destroy();
                        }

                        IntVec3 enterCell = WorldSettlementFC.FindNearEdgeCell(settlement.worldSettlement.Map);
                        foreach (Pawn pawn in supporting)
                        {
                            IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(enterCell, settlement.worldSettlement.Map);
                            GenSpawn.Spawn(pawn, loc, settlement.worldSettlement.Map, Rot4.Random);
                        }
                    });
                };
                __result = __result.Append(defend);
            }
        }
    }
}