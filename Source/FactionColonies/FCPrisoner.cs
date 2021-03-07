using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace FactionColonies
{
    public enum FCWorkLoad : Byte
    {
        Light, //Adds to overmax
        Medium, //Adds 1 to max
        Heavy //Adds 2 to max
    }

    public class FCPrisoner : ILoadReferenceable, IExposable
    {
        public Pawn prisoner;
        public SettlementFC settlement;
        public float unrest;
        public float health;
        public bool isReturning;
        public int loadID;
        public FCWorkLoad workload;
        public Pawn_HealthTracker healthTracker;
        

        public FCPrisoner () 
        {
            //this.workload = FCWorkLoad.Light;
            //this.healthTracker = new Pawn_HealthTracker(pawn);
            //this.healthTracker = prisoner.health;
            //HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(this.prisoner);
            
        }

        public FCPrisoner (Pawn pawn, SettlementFC settlement)
        {
            this.prisoner = pawn;
            this.settlement = settlement;
            this.unrest = 0;
            this.healthTracker = new Pawn_HealthTracker(pawn);
            this.healthTracker = pawn.health;
            this.health = (float)Math.Round(this.prisoner.health.summaryHealth.SummaryHealthPercent * 100);
            this.isReturning = false;
            this.loadID = Find.World.GetComponent<FactionFC>().GetNextPrisonerID();
        }

        
        public void ExposeData()
        {
            Scribe_Deep.Look<Pawn>(ref prisoner, "prisoner");
            Scribe_References.Look<SettlementFC>(ref settlement, "settlement");
            Scribe_Values.Look<float>(ref unrest, "unrest");
            Scribe_Values.Look<float>(ref health, "healthy");
            Scribe_Values.Look<bool>(ref isReturning, "isReturning");
            Scribe_Values.Look<int>(ref loadID, "loadID");
            Scribe_Values.Look<FCWorkLoad>(ref workload, "workload");
            Scribe_Deep.Look<Pawn_HealthTracker>(ref healthTracker, "healthTracker", new object[] { this.prisoner });
        }


        public string GetUniqueLoadID()
        {
            return "FCPrisoner_" + this.loadID;
        }



        public bool AdjustHealth(int value)
        {
            this.health += value;
            if (this.health >= 100)
            {
                health = 100;
                if (this.prisoner != null && this.prisoner.health != null)
                    HealthUtility.HealNonPermanentInjuriesAndRestoreLegs(this.prisoner);
            }

            return checkDead();
        }

        public bool checkDead()
        {
            if (this.health <= 0)
            {
                this.settlement.prisonerList.Remove(this);
                Find.LetterStack.ReceiveLetter("PrisonerHasDiedLetter".Translate(), TranslatorFormattedStringExtensions.Translate("PrisonerHasDied", this.prisoner.Name.ToString(), this.settlement.name), LetterDefOf.NeutralEvent);
                return true;
            }
            return false;
        }
    }
}
