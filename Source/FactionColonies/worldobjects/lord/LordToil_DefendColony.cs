using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColonies
{
    public class LordToil_DefendColony : LordToil
    {
        public override bool ForceHighStoryDanger => true;

        public override bool AllowSatisfyLongNeeds => false;

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns.Where(pawn => pawn.mindState.duty != null && 
                                                                pawn.mindState.duty.def.defName == "DefendColony"))
            {
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("DefendColony"),
                    pawn.Position);
                pawn.mindState.canFleeIndividual = false;
                pawn.jobs.ClearQueuedJobs();
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.def.IsMeleeWeapon)
                {
                    pawn.jobs.StartJob(new Job(JobDefOf.AttackMelee), JobCondition.InterruptForced);
                }
                else
                {
                    pawn.jobs.StartJob(new Job(JobDefOf.AttackStatic), JobCondition.InterruptForced);
                }
            }
        }
    }
}