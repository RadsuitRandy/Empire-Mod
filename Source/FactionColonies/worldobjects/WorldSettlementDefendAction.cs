using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    public class WorldSettlementDefendAction : CaravanArrivalAction
    {
        private WorldSettlementFC settlement;
        
        public override void Arrived(Caravan caravan)
        {
            settlement.startDefense(
                MilitaryUtilFC.returnMilitaryEventByLocation(caravan.Tile), () =>
                {
                    CaravanSupporting caravanSupporting = new CaravanSupporting();
                    List<Pawn> supporting = caravan.pawns.InnerListForReading.ListFullCopy();
                    caravanSupporting.pawns = supporting;
                    settlement.supporting.Add(caravanSupporting);
                    if (!caravan.Destroyed)
                    {
                        caravan.Destroy();
                    }

                    IntVec3 enterCell = WorldSettlementFC.FindNearEdgeCell(settlement.Map);
                    foreach (Pawn pawn in supporting)
                    {
                        IntVec3 loc =
                            CellFinder.RandomSpawnCellForPawnNear(enterCell, settlement.Map);
                        GenSpawn.Spawn(pawn, loc, settlement.Map, Rot4.Random);
                        settlement.defenders.Add(pawn);
                        settlement.defenders[0].GetLord().AddPawn(pawn);
                    }
                });
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
                () => settlement.Spawned &&
                      !settlement.settlement.isUnderAttack && settlement.CanTradeNow,
                () => new WorldSettlementTradeAction(settlement),
                "TradeWith".Translate((NamedArgument) settlement.Label), caravan,
                settlement.Tile, settlement);
        }
    }
}