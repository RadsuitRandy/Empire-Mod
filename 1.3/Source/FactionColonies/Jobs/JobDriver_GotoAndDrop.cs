using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace FactionColonies
{
	class JobDriver_GotoAndDrop : JobDriver_Goto
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			List<Toil> toils = base.MakeNewToils().ToList();

			toils.Add(new Toil
			{
				initAction = delegate
				{
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out Thing thing);
				}
			});

			foreach(Toil toil in toils)
			{
				toil.AddFailCondition(delegate ()
				{
					 return pawn.Map.dangerWatcher.DangerRating != RimWorld.StoryDanger.None;
				});
			}

			return toils;
		}
	}
}
