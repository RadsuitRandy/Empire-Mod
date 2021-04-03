using Verse;

namespace FactionColonies
{
    public class Mercenary : IExposable, ILoadReferenceable
    {
        //init variables
        public MilUnitFC loadout;
        public MercenarySquadFC squad;
        public SettlementFC settlement;
        public Mercenary handler;
        public Mercenary animal;
        public Pawn pawn;
        public bool deployable = false;
        public int loadID;
        
        public Mercenary()
        {

        }

        public Mercenary(bool blank)
        {
            loadID = Find.World.GetComponent<FactionFC>().GetNextMercenaryID();
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref loadout, "loadout");
            Scribe_References.Look(ref squad, "squad");
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_References.Look(ref handler, "handler");
            Scribe_References.Look(ref animal, "animal");
            Scribe_Deep.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref loadID, "loadID");

        }

        public string GetUniqueLoadID()
        {
            return "Mercenary_" + loadID;
        }


    }


}
