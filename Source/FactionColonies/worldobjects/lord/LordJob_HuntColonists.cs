using RimWorld;
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
    }
}