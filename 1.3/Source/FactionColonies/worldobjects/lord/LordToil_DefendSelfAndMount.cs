using System.Collections.Generic;
using System.Linq;
using FactionColonies.util;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordToil_DefendSelfAndMount : LordToil
    {
        private Dictionary<Pawn, Pawn> mounts;

        public LordToil_DefendSelfAndMount(Dictionary<Pawn, Pawn> mounts)
        {
            this.mounts = mounts;
        }

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns.Where(pawn => pawn.mindState.duty == null))
            {
                if (mounts.ContainsKey(pawn))
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.WanderClose, (LocalTargetInfo) pawn);
                    if (pawn.jobs == null) pawn.jobs = new Pawn_JobTracker(pawn);
                    GiddyUpUtil.Mount(pawn, mounts[pawn]);
                }
                else
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend,
                        pawn.Position) {radius = 28f};
                }
            }
        }
    }
}