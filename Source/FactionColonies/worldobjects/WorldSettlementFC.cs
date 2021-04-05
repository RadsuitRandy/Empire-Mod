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

        public militaryForce Defenders { set; get; }

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
                        MapGenerator.GenerateMap(new IntVec3(100, 1, 100), this,
                            MapGeneratorDef, ExtraGenStepDefs);
                        Log.Message("Test 1");
                    }

                    zoomIntoTile();
                };
                gizmos.Add(defend);
            }

            return gizmos;
        }

        private void zoomIntoTile()
        {
            Log.Message("1");
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

                List<Pawn> friendlies = generateFriendlies((float) Defenders.forceRemaining * 100);

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
            if (Defenders.homeSettlement.militarySquad != null)
            {
                friendlies = Defenders.homeSettlement.militarySquad.EquippedMercenaryPawns;
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

            return friendlies;
        }

        public void endAttack(bool defendersWin)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            if (defendersWin)
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

            if (Defenders.homeSettlement != Settlement)
            {
                int remaining = 0;
                foreach (Pawn pawn in Map.mapPawns.FreeColonists)
                {
                    if (!pawn.Dead && !pawn.Downed)
                    {
                        remaining++;
                    }
                }

                //if not the home settlement defending
                if (remaining >= 7)
                {
                    Find.LetterStack.ReceiveLetter("OverwhelmingVictory".Translate(),
                        "OverwhelmingVictoryDesc".Translate(), LetterDefOf.PositiveEvent);
                    Defenders.homeSettlement.returnMilitary(true);
                }
                else
                {
                    Defenders.homeSettlement.cooldownMilitary();
                }
            }

            Map.lordManager.lords.Clear();
            Current.Game.tickManager.RemoveAllFromMap(Map);
            Settlement.isUnderAttack = false;
            Attackers = null;
            //Prevent player from zooming back into the settlement
            Current.Game.CurrentMap = Find.World.worldObjects.SettlementAt(
                Find.World.GetComponent<FactionFC>().capitalLocation).Map;
            Current.Game.DeinitAndRemoveMap(Map);
        }

        public override void Notify_MyMapRemoved(Map map) { }

        public void checkWinners()
        {
            if (Attackers == null)
            {
                return;
            }

            bool lost = true;
            foreach (Pawn pawn in Map.mapPawns.FreeColonists)
            {
                if (!pawn.Dead && !pawn.Downed)
                {
                    Log.Message(pawn.Name + " lives for defenders");
                    lost = false;
                }
            }

            if (lost)
            {
                endAttack(false);
            }

            bool won = true;
            foreach (Pawn pawn in Attackers)
            {
                if (!pawn.Dead && !pawn.Downed)
                {
                    Log.Message(pawn.Name + " lives for attackers");
                    won = false;
                }
            }

            if (won)
            {
                endAttack(true);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    class FighterDied
    {
        static void Postfix(Pawn __instance)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.Tile, Find.World.info.name);
            if (settlement != null)
            {
                settlement.WorldSettlement.checkWinners();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    class FighterDowned
    {
        static void Postfix(Pawn_HealthTracker __instance)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(__instance.immunity.pawn.Tile, Find.World.info.name);
            if (settlement != null)
            {
                settlement.WorldSettlement.checkWinners();
            }
        }
    }
}