using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    public class WorldSettlementDefendAction : CaravanArrivalAction
    {
        private WorldSettlementFC settlement;

        //For saving
        public WorldSettlementDefendAction()
        {
        }
        
        public WorldSettlementDefendAction(WorldSettlementFC settlement)
        {
            this.settlement = settlement;
        }

        public override void Arrived(Caravan caravan)
        {
            settlement.startDefense(MilitaryUtilFC.returnMilitaryEventByLocation(settlement.settlement.mapLocation),() => settlement.CaravanDefend(caravan));
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlement, "settlement");
        }
        
        public override string Label => "DefendColony".Translate();

        public override string ReportString => "DefendColonyDesc".Translate();
        
        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(
            Caravan caravan,
            WorldSettlementFC settlement)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(
                () => settlement.Spawned && settlement.settlement.isUnderAttack,
                () => new WorldSettlementDefendAction(settlement),
                "DefendColony".Translate(), caravan,
                settlement.Tile, settlement);
        }
    }

    [HarmonyPatch]
    public class WorldSettlementTransportPodDefendAction : TransportPodsArrivalAction_LandInSpecificCell
    {
        private readonly IntVec3 cell;
        private readonly MapParent mapParent;
        private readonly bool landInShuttle;

        public WorldSettlementTransportPodDefendAction(WorldSettlementFC mapParent, IntVec3 cell, bool landInShuttle)
        {
            this.mapParent = mapParent;
            this.cell = cell;
            this.landInShuttle = landInShuttle;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TransportPodsArrivalAction_LandInSpecificCell), "Arrived")]
        private static void ArrivePatch(TransportPodsArrivalAction_LandInSpecificCell __instance, List<ActiveDropPodInfo> pods, int tile)
        {
            if (Traverse.Create(__instance).Field("mapParent").GetValue() is WorldSettlementFC settlement)
            {
                List<Pawn> pawns = new List<Pawn>();
                bool hasAnyPawns = false;

                foreach (ActiveDropPodInfo activeDropPodInfo in pods)
                {
                    foreach (Thing thing in activeDropPodInfo.innerContainer)
                    {
                        if (thing is Pawn pawn)
                        {
                            hasAnyPawns = true;
                            pawns.Add(pawn);
                        }
                    }
                }

                if (hasAnyPawns) settlement.AddToDefenceFromList(pawns, tile);
            }
        }
    }
}