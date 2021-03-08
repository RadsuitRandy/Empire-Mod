using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace FactionColonies
{
    public class ResourceFC : IExposable
    {
        public ResourceFC()
        {
        }

        public ResourceFC(string name, string label, double baseProduction, SettlementFC settlement = null, int i = -1)
        {
            this.name = name;
            this.label = label;
            this.baseProduction = baseProduction;
            this.endProduction = baseProduction;
            this.amount = 0;
            this.baseProductionMultiplier = 1;
            this.baseProductionAdditives.Add(new ProductionAdditive("", 0, ""));
            this.baseProductionMultipliers.Add(new ProductionMultiplier("", 0, ""));
            this.settlement = settlement;
            this.filter = new ThingFilter();
            if (settlement != null)
            {
                PaymentUtil.resetThingFilter(settlement, i);
            }
        }
        
        public bool checkMinimum()
        {
            if (taxStock >= taxMinimumToTithe)
            {
                return true;
            } else
            {
                return false;
            }
        }
        public double returnTaxPercentage()
        {
            taxPercentage = Math.Round(taxStock / taxMinimumToTithe, 2)*100 ;
            return taxPercentage;
        }

        public double returnLowestCost()
        {
            double minimum = 999999;
            foreach (ThingDef thing in filter.AllowedThingDefs) 
            {
                minimum = Math.Min(thing.BaseMarketValue, minimum);
            }
            //Log.Message(minimum.ToString());
            taxMinimumToTithe = minimum + (double)LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().productionTitheMod + traitUtilsFC.cycleTraits(0.0, "taxBaseRandomModifier", Find.World.GetComponent<FactionFC>().traits, "add") + traitUtilsFC.cycleTraits(0.0, "taxBaseRandomModifier", settlement.traits, "add");
            return minimum;
        }

        public Texture2D getIcon()
        {
            for(int i = 0; i < texLoad.textures.Count(); i++)
            {
                if (texLoad.textures[i].Key == name)
                {
                    return texLoad.textures[i].Value;
                }
            }
            return null;
        }


        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref name, "name");
            Scribe_Values.Look<string>(ref label, "label");
            Scribe_Values.Look<double>(ref baseProduction, "baseProduction");
            Scribe_Values.Look<double>(ref endProduction, "endProduction");
            Scribe_Values.Look<double>(ref baseProductionMultiplier, "baseProductionMultiplier");
            Scribe_Values.Look<double>(ref endProductionMultiplier, "endProductionMultiplier");
            Scribe_Values.Look<double>(ref amount, "amount");
            Scribe_Collections.Look<ProductionAdditive>(ref baseProductionAdditives, "baseProductionAdditives", LookMode.Deep);
            Scribe_Collections.Look<ProductionMultiplier>(ref baseProductionMultipliers, "baseProductionMultipliers", LookMode.Deep);

            //tithe and income data
            Scribe_Values.Look<bool>(ref isTithe, "isTithe");
            Scribe_Values.Look<bool>(ref isTitheBool, "isTitheBool");
            Scribe_Values.Look<int>(ref assignedWorkers, "assignedWorkers");

            Scribe_Deep.Look<ThingFilter>(ref filter, "filter");
            //Tax Stock
            Scribe_Values.Look<double>(ref taxStock, "taxStock");
            Scribe_Values.Look<double>(ref taxMinimumToTithe, "taxMinimumToTithe");
            Scribe_Values.Look<double>(ref taxPercentage, "taxPercentage");

            Scribe_References.Look<SettlementFC>(ref settlement, "settlement");
        }

        public string name;
        public string label;
        public double baseProduction = 0; //base production for resource
        public double endProduction = 0;  //production after modifiers
        public double baseProductionMultiplier = 1;  //base production modifier for resource
        public double endProductionMultiplier = 1;  //end production modifier for resource
        public List<ProductionAdditive> baseProductionAdditives = new List<ProductionAdditive>();    // {ID, Value, Desc}
        public List<ProductionMultiplier> baseProductionMultipliers = new List<ProductionMultiplier>();  // {ID, Value, Desc}
        public double amount;
        public int assignedWorkers = 0;
        public bool isTithe = false;
        public bool isTitheBool = false; //used to track if isTithe is changed. AGHHH

        public ThingFilter filter = new ThingFilter();
        public double taxStock = 0;
        public double taxMinimumToTithe = 99999;
        public double taxPercentage = 0;
        public SettlementFC settlement;

    }

    public class ProductionAdditive : IExposable
    {
        public ProductionAdditive()
        {

        }

        public ProductionAdditive(string id, double value, string desc)
        {
            this.id = id;
            this.value = value;
            this.desc = desc;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref id, "id");
            Scribe_Values.Look<double>(ref value, "value");
            Scribe_Values.Look<string>(ref desc, "desc");
        }

        public string id;
        public double value;
        public string desc;
    }

    public class ProductionMultiplier : IExposable
    {
        public ProductionMultiplier()
        {

        }
        public ProductionMultiplier(string id, double value, string desc)
        {
            this.id = id;
            this.value = value;
            this.desc = desc;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref id, "id");
            Scribe_Values.Look<double>(ref value, "value");
            Scribe_Values.Look<string>(ref desc, "desc");
        }

        public string id;
        public double value;
        public string desc;
    }
}
