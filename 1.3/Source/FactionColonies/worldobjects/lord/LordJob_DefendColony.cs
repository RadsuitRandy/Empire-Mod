using System.Collections.Generic;
using System.Linq;
using FactionColonies.util;
using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordJob_DefendColony : LordJob
    {
        private readonly Dictionary<Pawn, Pawn> mounts;
        private readonly HashSet<Pawn> readded = new HashSet<Pawn>();

        public LordJob_DefendColony(Dictionary<Pawn, Pawn> mounts)
        {
            this.mounts = mounts;
        }
        
        public override bool AddFleeToil => false;
        public override bool AllowStartNewGatherings => false;
        public override bool AlwaysShowWeapon => true;

        public override void LordJobTick()
        {
            base.LordJobTick();
            if (readded.Any(pawn => pawn?.mindState?.duty == null))
            {
                lord.CurLordToil.UpdateAllDuties();
                readded.Clear();
            }
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendSelfAndMount defendSelf = new LordToil_DefendSelfAndMount(mounts);
            stateGraph.AddToil(defendSelf);
            LordToil defendColony = new LordToil_DefendColony();
            stateGraph.AddToil(defendColony);
            Transition startDefending = new Transition(defendSelf, defendColony);
            startDefending.AddTrigger(new Trigger_Signal("startAssault"));
            stateGraph.AddTransition(startDefending);
            return stateGraph;
        }

        public override void Notify_PawnLost(Pawn pawn, PawnLostCondition condition)
        {
            if (condition == PawnLostCondition.ChangedFaction || condition == PawnLostCondition.ExitedMap)
            {
                lord.AddPawn(pawn);
                readded.Add(pawn);
                return;
            }
            if (pawn.IsMercenary() && pawn.Faction != FactionColonies.getPlayerColonyFaction()) pawn.SetFaction(FactionColonies.getPlayerColonyFaction());
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            //Check if a settlement battle ended
            SettlementFC settlement = faction.getSettlement(pawn.Tile, Find.World.info.name);
            settlement?.worldSettlement.removeDefender(pawn);
        }
    }
}