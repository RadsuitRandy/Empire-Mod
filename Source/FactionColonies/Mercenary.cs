using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using RimWorld.Planet;

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
            this.loadID = Find.World.GetComponent<FactionFC>().GetNextMercenaryID();
        }

        public void ExposeData()
        {
            Scribe_References.Look<MilUnitFC>(ref loadout, "loadout");
            Scribe_References.Look<MercenarySquadFC>(ref squad, "squad");
            Scribe_References.Look<SettlementFC>(ref settlement, "settlement");
            Scribe_References.Look<Mercenary>(ref handler, "handler");
            Scribe_References.Look<Mercenary>(ref animal, "animal");
            Scribe_Deep.Look<Pawn>(ref pawn, "pawn");
            Scribe_Values.Look<int>(ref loadID, "loadID");

        }

        public string GetUniqueLoadID()
        {
            return "Mercenary_" + this.loadID;
        }


    }


}
