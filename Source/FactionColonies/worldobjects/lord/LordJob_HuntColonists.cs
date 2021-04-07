using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordJob_HuntColonists : LordJob
    {
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil = new LordToil_HuntColonists();
            stateGraph.AddToil(lordToil);
            return stateGraph;
        }

        public override void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(pawn.Tile, Find.World.info.name);
            settlement?.worldSettlement.removeAttacker(pawn);
        }
    }
}