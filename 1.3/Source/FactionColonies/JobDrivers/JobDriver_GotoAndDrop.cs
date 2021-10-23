using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace FactionColonies
{
    class JobDriver_GotoAndDrop : JobDriver_Goto
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return base.TryMakePreToilReservations(errorOnFailed);
        }

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

            return toils;
        }
    }
}
