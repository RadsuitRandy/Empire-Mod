using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
using FactionColonies.util;
using System.Linq;
using Verse.AI;

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

		private bool CanNotReach() => lord.ownedPawns.All(pawn => pawn.carryTracker.CarriedThing == null) && lord.ownedPawns[0].mindState?.duty.def != DutyDefOf.ExitMapBestAndDefendSelf && lord.ownedPawns[0].CanReach(lord.ownedPawns[0].CurJob.targetA, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(lord.ownedPawns[0], Danger.Some), false, false, TraverseMode.ByPawn);

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
						Messages.Message("deliveryPawnsEngageEnemy".Translate(), lord.ownedPawns, MessageTypeDefOf.NeutralEvent);
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
						if (Map.HasWoundedForFaction(lord.faction))
						{
							Messages.Message("pawnsLeavingMapWithDowned".Translate(), lord.ownedPawns, MessageTypeDefOf.NeutralEvent);
						}
						else
						{
							Messages.Message("pawnsLeavingMap".Translate(), lord.ownedPawns, MessageTypeDefOf.NeutralEvent);
						}
					})
				}
			});

			stateGraph.AddTransition(new Transition(stateGraph.lordToils[0], stateGraph.lordToils[2])
			{
				triggers = new List<Trigger>
				{
					new Trigger_Custom(delegate(TriggerSignal signal) 
					{
						return CanNotReach();
					})
				},
				preActions = new List<TransitionAction>
				{
					new TransitionAction_Custom(delegate ()
					{
						Messages.Message("pawnsLeavingMapNoPath".Translate(), lord.ownedPawns, MessageTypeDefOf.NeutralEvent);
					})
				}
			});

			return stateGraph;
		}
	}
}
