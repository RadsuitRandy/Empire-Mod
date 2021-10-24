using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColonies
{
    class LordToil_TakeWoundedAndLeave : LordToil
    {
        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }
        public override bool AllowSelfTend
        {
            get
            {
                return false;
            }
        }
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                lord.ownedPawns[i].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("FCTakeWoundedAndLeave"));
            }
        }
    }
}
