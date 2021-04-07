using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordJob_DefendColony : LordJob
    {
        public override bool AddFleeToil { get; } = false;
        
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendSelf defendSelf = new LordToil_DefendSelf();
            stateGraph.AddToil(defendSelf);
            LordToil defendColony = new LordToil_DefendColony();
            stateGraph.AddToil(defendColony);
            Transition startDefending = new Transition(defendSelf, defendColony);
            startDefending.AddTrigger(new Trigger_Signal("startAssault"));
            stateGraph.AddTransition(startDefending);
            return stateGraph;
        }

        public override void Notify_PawnAdded(Pawn pawn)
        {
            lord.CurLordToil.UpdateAllDuties();
        }
        
        public override void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
        {
            if (condition == PawnLostCondition.ChangedFaction)
            {
                return;
            }
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(pawn.Tile, Find.World.info.name);
            Log.Message("Removing " + pawn.Name + " for " + condition);
            settlement?.worldSettlement.removeDefender(pawn);
        }
    }
}