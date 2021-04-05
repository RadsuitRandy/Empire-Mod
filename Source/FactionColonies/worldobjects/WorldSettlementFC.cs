using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FactionColonies
{
    public class WorldSettlementFC : MapParent
    {
        public SettlementFC Settlement { get; set; }

        public List<Pawn> Attackers { set; get; }

        public List<Pawn> Defenders { set; get; }

        public militaryForce DefenderForce { set; get; }

        public militaryForce AttackerForce { set; get; }

        public string Name
        {
            get => Settlement.name;
            set => Settlement.name = value;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>();
            if (Settlement.isUnderAttack)
            {
                Command_Action defend = new Command_Action();

                defend.defaultLabel = "DefendColony".Translate();
                defend.defaultDesc = "DefendColonyDesc".Translate();
                defend.icon = TexLoad.iconMilitary;
                defend.action = () =>
                {
                    if (Map == null)
                    {
                        LongEventHandler.QueueLongEvent(() =>
                            {
                                MapGenerator.GenerateMap(new IntVec3(100, 1, 100), this,
                                    MapGeneratorDef, ExtraGenStepDefs).mapDrawer.RegenerateEverythingNow();
                                zoomIntoTile();
                            },
                            "GeneratingMap", false, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
                    }
                    else
                    {
                        zoomIntoTile();
                    }
                };
                gizmos.Add(defend);
            }

            return gizmos;
        }

        private void zoomIntoTile()
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();

            FCEvent evt = MilitaryUtilFC.returnMilitaryEventByLocation(Tile);

            if (Map.mapPawns.ColonistCount == 0)
            {
                militaryForce force = MilitaryUtilFC.returnDefendingMilitaryForce(evt);
                if (force == null)
                {
                    evt.timeTillTrigger = Find.TickManager.TicksGame;
                    return;
                }

                foreach (Building building in Map.listerBuildings.allBuildingsColonist)
                {
                    FloodFillerFog.FloodUnfog(building.InteractionCell, Map);
                }

                List<Pawn> friendlies = generateFriendlies((float) DefenderForce.forceRemaining * 100);

                if (friendlies.Any())
                {
                    CameraJumper.TryJump(new GlobalTargetInfo(friendlies[0]));
                }
            }

            CameraJumper.TryJump(new GlobalTargetInfo(Map.mapPawns.FreeColonists[0]));

            evt.timeTillTrigger = Find.TickManager.TicksGame;
        }

        private List<Pawn> generateFriendlies(float points)
        {
            List<Pawn> friendlies;
            if (DefenderForce.homeSettlement.militarySquad != null)
            {
                friendlies = DefenderForce.homeSettlement.militarySquad.EquippedMercenaryPawns;
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
                    Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamed("PColony"));
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

            foreach (Pawn friendly in friendlies)
            {
                friendly.SetFaction(Faction.OfPlayer);

                IntVec3 loc = new IntVec3(-1000, 0, -1000);
                CellFinder.TryFindRandomCellInsideWith(new CellRect(45, 45, 10, 10),
                    x => x.Standable(Map), out loc);
                if (loc.x == -1000 && loc.z == -1000)
                {
                    Find.WorldPawns.PassToWorld(friendly);
                }
                else
                {
                    GenSpawn.Spawn(friendly, loc, Map, new Rot4());
                }
            }

            Defenders = friendlies;

            return friendlies;
        }

        public void endAttack()
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            if (Defenders.Any())
            {
                faction.addExperienceToFactionLevel(5f);
                //if winner is player
                Find.LetterStack.ReceiveLetter("DefenseSuccessful".Translate(),
                    "DefenseSuccessfulFull".Translate(Settlement.name, AttackerForce.homeFaction),
                    LetterDefOf.PositiveEvent, new LookTargets(this));
            }
            else
            {
                //get multipliers
                double happinessLostMultiplier =
                    (TraitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier",
                        Settlement.traits, "multiply") * TraitUtilsFC.cycleTraits(new double(),
                        "happinessLostMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply"));
                double loyaltyLostMultiplier =
                    (TraitUtilsFC.cycleTraits(new double(), "loyaltyLostMultiplier", Settlement.traits,
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
                Settlement.prosperity -= 20 * traitProsperityMultiplier;
                Settlement.happiness -= 25 * happinessLostMultiplier;
                Settlement.loyalty -= 15 * loyaltyLostMultiplier * traitMuliplier;

                string str = "DefenseFailureFull".Translate(Settlement.name, AttackerForce.homeFaction);


                for (int k = 0; k < 4; k++)
                {
                    int num = new IntRange(0, 10).RandomInRange;
                    if (num >= 7 && Settlement.buildings[k].defName != "Empty" &&
                        Settlement.buildings[k].defName != "Construction" && traitCanDestroyBuildings)
                    {
                        str += "\n" +
                               "BulidingDestroyedInRaid".Translate(Settlement.buildings[k].label);
                        Settlement.deconstructBuilding(k);
                    }
                }

                //level remover checker
                if (Settlement.settlementLevel > 1 && traitCanDestroyBuildings)
                {
                    int num = new IntRange(0, 10).RandomInRange;
                    if (num >= 7)
                    {
                        str += "\n\n" + "SettlementDeleveledRaid".Translate();
                        Settlement.delevelSettlement();
                    }
                }

                Find.LetterStack.ReceiveLetter("DefenseFailure".Translate(), str, LetterDefOf.Death,
                    new LookTargets(Find.WorldObjects.SettlementAt(Settlement.mapLocation)));
            }

            if (DefenderForce.homeSettlement != Settlement)
            {
                //if not the home settlement defending
                if (Defenders.Count >= 7)
                {
                    Find.LetterStack.ReceiveLetter("OverwhelmingVictory".Translate(),
                        "OverwhelmingVictoryDesc".Translate(), LetterDefOf.PositiveEvent);
                    DefenderForce.homeSettlement.returnMilitary(true);
                }
                else
                {
                    DefenderForce.homeSettlement.cooldownMilitary();
                }
            }

            Log.Message("Cleared lords");
            Map.lordManager.lords.Clear();
            Log.Message("Cleared tick manager");
            Current.Game.tickManager.RemoveAllFromMap(Map);
            Settlement.isUnderAttack = false;
            Attackers = null;
            Log.Message("Deiniting map");
            //Prevent player from zooming back into the settlement
            Current.Game.CurrentMap = Find.World.worldObjects.SettlementAt(
                Find.World.GetComponent<FactionFC>().capitalLocation).Map;
            Current.Game.DeinitAndRemoveMap(Map);
        }

        public bool checkDowned(Pawn downed)
        {
            int index = Attackers.IndexOf(downed);
            if (index > 0)
            {
                Attackers.RemoveAt(index);
                if (!Attackers.Any())
                {
                    endAttack();
                    return true;
                }

                return false;
            }

            index = Defenders.IndexOf(downed);
            if (index > 0)
            {
                Defenders.RemoveAt(index);
                if (!Defenders.Any())
                {
                    endAttack();
                    return true;
                }
            }

            return false;
        }

        public override void Notify_MyMapRemoved(Map map)
        {
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    class FighterDied
    {
        static bool Prefix(Pawn __instance)
        {
            Log.Message("Someone down!");
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.Tile, Find.World.info.name);
            return settlement?.WorldSettlement.checkDowned(__instance) ?? false;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    class FighterDowned
    {
        static bool Prefix(Pawn_HealthTracker __instance)
        {
            Log.Message("Someone down!");
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.immunity.pawn.Tile, Find.World.info.name);
            return settlement?.WorldSettlement.checkDowned(__instance.immunity.pawn) ?? false;
        }
    }
}