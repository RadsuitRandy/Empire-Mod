using System.Collections.Generic;
using FactionColonies.util;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    class LordJob_DeliverSupplies : LordJob
    {
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph
            {
                StartingToil = new LordToil_DeliverSupplies()
            };

            TraverseParms traverseParms = DeliveryEvent.DeliveryTraverseParms;
            if (!CellFinder.TryFindRandomReachableCellNear(DeliveryEvent.GetDeliveryCell(traverseParms, Map), Map, 5, traverseParms, null, null, out IntVec3 result))
            {
                result = DeliveryEvent.GetDeliveryCell(traverseParms, Map);
            }
            stateGraph.AddToil(new LordToil_HuntEnemies(result));
            stateGraph.AddToil(new LordToil_TakeWoundedAndLeave());

            stateGraph.AddTransition(new Transition(stateGraph.StartingToil, stateGraph.lordToils[1])
            {
                triggers = new List<Trigger>
                {
                    new Trigger_PawnHarmed(),

                }, preActions = new List<TransitionAction>
                {
                    new TransitionAction_Custom(delegate ()
                    {
                        Messages.Message("deliveryPawnsInjured".Translate(), MessageTypeDefOf.NeutralEvent);
                    })
                }
            });


            stateGraph.AddTransition(new Transition(stateGraph.lordToils[1], stateGraph.lordToils[2])
            {
                triggers = new List<Trigger>
                {
                    new Trigger_TicksPassedAndNoRecentHarm(1000)
                },
                preActions = new List<TransitionAction>
                {
                    new TransitionAction_Custom(delegate ()
                    {
                        Messages.Message("pawnsLeavingMap".Translate(), MessageTypeDefOf.NeutralEvent);
                    })
                }
            });

            return stateGraph;
        }
    }
}
