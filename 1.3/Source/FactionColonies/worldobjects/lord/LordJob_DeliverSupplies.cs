using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FactionColonies
{
    class LordJob_DeliverSupplies : LordJob
    {
        private IntVec3 fallbackLocation;

        public LordJob_DeliverSupplies()
        {
        }

        public LordJob_DeliverSupplies(IntVec3 fallbackLocation)
        {
            this.fallbackLocation = fallbackLocation;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref fallbackLocation, "fallbackLocation", default, false);
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph
            {
                StartingToil = new LordToil_DeliverSupplies()
            };

            stateGraph.AddToil(new LordToil_HuntEnemies(fallbackLocation));
            stateGraph.AddToil(new LordToil_TakeWoundedAndLeave());

            
            stateGraph.AddTransition(new Transition(stateGraph.StartingToil, stateGraph.lordToils[1])
            {
                triggers = new List<Trigger>
                {
                    new Trigger_PawnHarmed(),
                    new Trigger_Custom((TriggerSignal s) => Map.dangerWatcher.DangerRating == StoryDanger.High)

                }, preActions = new List<TransitionAction>
                {
                    new TransitionAction_Custom(delegate ()
                    {
                        Messages.Message("deliveryPawnsEngageEnemy".Translate(), MessageTypeDefOf.NeutralEvent);
                    })
                }
            });

            stateGraph.AddTransition(new Transition(stateGraph.lordToils[1], stateGraph.lordToils[1], true)
            {
                triggers = new List<Trigger>
                {
                    new Trigger_PawnHarmed(),
                    new Trigger_Custom((TriggerSignal s) => Map.dangerWatcher.DangerRating == StoryDanger.High)

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
