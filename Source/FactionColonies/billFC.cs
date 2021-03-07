using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace FactionColonies
{

    public class TaxesFC : ILoadReferenceable, IExposable
    {
        //internal variables
        public int loadID;
        public List<Thing> itemTithes;
        public float silverAmount;
        public float electricityAllotted;
        public float researchCompleted;

        //ref
        public SettlementFC settlement;
        public BillFC bill;

        public void ExposeData()
        {
            Scribe_Collections.Look<Thing>(ref itemTithes, "itemTithes", LookMode.Deep);
            Scribe_Values.Look<float>(ref silverAmount, "silverAmount", 0);
            Scribe_Values.Look<float>(ref electricityAllotted, "electricityAllotted", 0);
            Scribe_Values.Look<float>(ref researchCompleted, "researchCompleted", 0);

            Scribe_Values.Look<int>(ref loadID, "loadID", -1);
            Scribe_References.Look<SettlementFC>(ref settlement, "settlement");
            Scribe_References.Look<BillFC>(ref bill, "bill");
        }

        public string GetUniqueLoadID()
        {
            return "Taxes_" + this.loadID;
        }

        public TaxesFC()
        {

        }

        public TaxesFC(BillFC bill)
        {
            SetUniqueLoadID();
            this.bill = bill;
            this.settlement = bill.settlement;
            this.silverAmount = 0;
            this.itemTithes = new List<Thing>();
            this.electricityAllotted = 0;
            this.researchCompleted = 0;

        }

        public void SetUniqueLoadID()
        {
            this.loadID = Find.World.GetComponent<FactionFC>().GetNextTaxID();
        }
    }

    public class BillFC : ILoadReferenceable, IExposable
    {
        //internal variables
        public int loadID;
        public int dueTick;
        

        //ref
        public SettlementFC settlement;
        public TaxesFC taxes;



        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref loadID, "loadID", -1);
            Scribe_Values.Look<int>(ref dueTick, "dueTick", -1);
            

            Scribe_References.Look<SettlementFC>(ref settlement, "settlement");
            Scribe_Deep.Look<TaxesFC>(ref taxes, "taxes");
            
        }

        public string GetUniqueLoadID()
        {
            return "Bill_" + this.loadID;
        }

        public BillFC()
        {

        }

        public BillFC(SettlementFC settlement)
        {
            SetUniqueLoadID();
            this.settlement = settlement;
            this.dueTick = Find.TickManager.TicksGame + 300000;
            this.taxes = new TaxesFC(this);
        }

        public void SetUniqueLoadID()
        {
            this.loadID = Find.World.GetComponent<FactionFC>().GetNextBillID();
        }

        public bool resolve()
        {
            FactionFC factionfc = Find.World.GetComponent<FactionFC>();
            if (PaymentUtil.getSilver() >= -1 * this.taxes.silverAmount || this.taxes.silverAmount >= 0)
            { //if have enough silver on the current map to pay  & map belongs to player

                FCEventMaker.createTaxEvent(this);
                if (this.taxes.researchCompleted != 0)
                {
                    factionfc.researchPointPool += this.taxes.researchCompleted;
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("PointsAddedToResearchPool", this.taxes.researchCompleted), MessageTypeDefOf.PositiveEvent);
                }

                if (this.taxes.electricityAllotted != 0)
                {
                    factionfc.powerPool += this.taxes.electricityAllotted;
                }

                return true;

            }
            else
            {
                Messages.Message("NotEnoughSilverForBill".Translate() + " " + this.settlement.name + ". " + "ConfiscatedTithes".Translate() + "." + " " + "UnpaidTitheEffect".Translate(), MessageTypeDefOf.NegativeEvent);
                this.settlement.unrest += 10 * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", this.settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", factionfc.traits, "multiply");
                this.settlement.happiness -= 10 * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", this.settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", factionfc.traits, "multiply");
                factionfc.Bills.Remove(this);
                return false;
            }
        }

        public bool attemptResolve()
        {
            FactionFC factionfc = Find.World.GetComponent<FactionFC>();
            if (PaymentUtil.getSilver() >= -1 * this.taxes.silverAmount || this.taxes.silverAmount >= 0)
            { //if have enough silver on the current map to pay  & map belongs to player

                FCEventMaker.createTaxEvent(this);
                if (this.taxes.researchCompleted != 0)
                {
                    factionfc.researchPointPool += this.taxes.researchCompleted;
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("PointsAddedToResearchPool", this.taxes.researchCompleted), MessageTypeDefOf.PositiveEvent);
                }

                if (this.taxes.electricityAllotted != 0)
                {
                    factionfc.powerPool += this.taxes.electricityAllotted;
                }

                return true;

            }
            else
            {
                return false;
            }
        }
    }



    public class billUtility
    {
        public static void processBills()
        {
            FactionFC factionfc = Find.World.GetComponent<FactionFC>();
            Reset:
            foreach(BillFC bill in factionfc.Bills)
            {
                if (bill.dueTick < Find.TickManager.TicksGame)
                { //if bill is overdue
                    bill.resolve();
                    goto Reset;
                }
            }
        }
    }
}
