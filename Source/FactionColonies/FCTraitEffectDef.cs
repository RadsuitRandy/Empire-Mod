using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace FactionColonies
{
    public class FCTraitEffectDef: Def, IExposable
    {
        public FCTraitEffectDef(){}

        public string desc = ""; //Description of trait

        //THING + (Base/Multiplier) + STAT

        //Resource Base Production  = Connected
        public double productionBaseFood = 0;        
        public double productionBaseWeapons = 0;
        public double productionBaseApparel = 0;
        public double productionBaseAnimals = 0;
        public double productionBaseLogging = 0;
        public double productionBaseMining = 0;
        public double productionBaseResearch = 0;
        public double productionBasePower = 0;
        public double productionBaseMedicine = 0;


        //Resource Multiplier Production = Connected
        public double productionMultiplierFood = 1;
        public double productionMultiplierWeapons = 1;
        public double productionMultiplierApparel = 1;
        public double productionMultiplierAnimals = 1;
        public double productionMultiplierLogging = 1;
        public double productionMultiplierMining = 1;
        public double productionMultiplierResearch = 1;
        public double productionMultiplierPower = 1;
        public double productionMultiplierMedicine = 1;

        //Military Stats  = baselevel connected
        public double militaryBaseLevel = 0;
        public double militaryMultiplierCombatEfficiency = 1;                                                                                          //#NEEDS TO BE IMPLEMENTED

        //Economic Stats
        public double taxBasePercentage = 0; //0.01 - 2.00// Affects the base tax percentage        implemented
        public double taxBaseRandomModifier = 0;  //Affects the modifier for tithe income            implemented
        public double prosperityBaseRecovery = 0; //Affects how quickly settlements recover from lost prosperity                                         #NEEDS TO BE IMPLEMENTED
        public double workerBaseCost = 0; //Affects how much a single worker costs                             implemented
        public double workerBaseMax = 0; //Affects how many workers you can have (Max) before worker costs start to rise      Implemented
        public double workerBaseOverMax = 0; //Affects how many workers past the max you can hire       Implemented

        //Social Stats Base
        public double happinessLostBase = 0; //0.0 - 2.0;     Affects how much happiness is lost            Implemented
        public double happinessGainedBase = 0; //0.0 - 2.0    Affects how much happiness is gained            Implemented
        public double loyaltyLostBase = 0; //0.0 - 2.0;         Affects how much loyalty is lost            Implemented
        public double loyaltyGainedBase = 0; //0.0 - 2.0;         Affects how much loyalty is gained            Implemented
        public double unrestLostBase = 0;  //0.0 - 2.0;         Affects how much unrest is lost            Implemented
        public double unrestGainedBase = 0; //0.0 - 2.0;         Affects how much loyalty is gained            Implemented

        //Social Stats Multipliers
        public double happinessLostMultiplier = 1; //0.0 - 2.0;     Affects how much happiness is lost            Implemented
        public double happinessGainedMultiplier = 1; //0.0 - 2.0    Affects how much happiness is gained            Implemented
        public double loyaltyLostMultiplier = 1; //0.0 - 2.0;         Affects how much loyalty is lost            Implemented
        public double loyaltyGainedMultiplier = 1; //0.0 - 2.0;         Affects how much loyalty is gained            Implemented
        public double unrestLostMultiplier = 1;  //0.0 - 2.0;         Affects how much unrest is lost            Implemented
        public double unrestGainedMultiplier = 1; //0.0 - 2.0;         Affects how much loyalty is gained            Implemented
                                              //                                        #NEEDS TO BE IMPLEMENTED

        //Create Settlement Stats
        public double createSettlementBaseCost = 0;  //affects how much it costs to create a settlement          Implemented only in faction
        public double createSettlementMultiplier = 1; //affects how much it costs to create a settlement         Only implemented in faction

        //Faction Pawn Required Traits
        List<TraitDef> forcedFactionPawnTraits = new List<TraitDef>();   //Traits that pawns are required to have                                        #NEEDS TO BE IMPLEMENTED
        List<PawnKindDef> factionAllowedRaces = new List<PawnKindDef>();  //List of races allowed in the faction                                        #NEEDS TO BE IMPLEMENTED
        List<Thing> factionUniform = new List<Thing>(); //List of the things pawns in the faction can wear                                        #NEEDS TO BE IMPLEMENTED

        public void ExposeData()
        {
            //Description
            Scribe_Values.Look<string>(ref desc, "desc");

        //Resource Base Production
        Scribe_Values.Look<double>(ref productionBaseFood, "productionBaseFood");
        Scribe_Values.Look<double>(ref productionBaseWeapons, "productionBaseWeapons");
        Scribe_Values.Look<double>(ref productionBaseApparel, "productionBaseApparel");
        Scribe_Values.Look<double>(ref productionBaseAnimals, "productionBaseAnimals");
        Scribe_Values.Look<double>(ref productionBaseLogging, "productionBaseLogging");
        Scribe_Values.Look<double>(ref productionBaseMining, "productionBaseMining");
        Scribe_Values.Look<double>(ref productionBaseAnimals, "productionBaseResearch");
        Scribe_Values.Look<double>(ref productionBaseLogging, "productionBasePower");
        Scribe_Values.Look<double>(ref productionBaseMining, "productionBaseMedicine");

        //Resource Multiplier Production
        Scribe_Values.Look<double>(ref productionMultiplierFood, "productionMultiplierFood");
        Scribe_Values.Look<double>(ref productionMultiplierWeapons, "productionMiltiplierWeapons");
        Scribe_Values.Look<double>(ref productionMultiplierApparel, "productionMultiplierApparel");
        Scribe_Values.Look<double>(ref productionMultiplierAnimals, "productionMultiplierAnimals");
        Scribe_Values.Look<double>(ref productionMultiplierLogging, "productionMultiplierLogging");
        Scribe_Values.Look<double>(ref productionMultiplierMining, "productionMultiplierMining");
        Scribe_Values.Look<double>(ref productionMultiplierAnimals, "productionMultiplierResearch");
        Scribe_Values.Look<double>(ref productionMultiplierLogging, "productionMultiplierPower");
        Scribe_Values.Look<double>(ref productionMultiplierMining, "productionMultiplierMedicine");

            //Military Stats
            Scribe_Values.Look<double>(ref militaryBaseLevel, "militaryBaseLevel");
        Scribe_Values.Look<double>(ref militaryMultiplierCombatEfficiency, "militaryMultiplierCombatEfficiency");

        //Economic Stats
        Scribe_Values.Look<double>(ref taxBasePercentage, "taxBasePercentage"); //0.01 - 2.00// Affects the base tax percentage
        Scribe_Values.Look<double>(ref taxBaseRandomModifier, "taxBaseRandomModifier");  //Affects the modifier for tithe income
        Scribe_Values.Look<double>(ref prosperityBaseRecovery, "prosperityBaseRecovery"); //Affects how quickly settlements recover from lost prosperity
        Scribe_Values.Look<double>(ref workerBaseCost, "workerBaseCost"); //Affects how much a single worker costs
        Scribe_Values.Look<double>(ref workerBaseMax, "workerBaseMax"); //Affects how many workers you can have (Max) before worker costs start to rise
        Scribe_Values.Look<double>(ref workerBaseOverMax, "workerBaseOverMax"); //Affects how many workers past the max you can hire

            //Social Stats Base
        Scribe_Values.Look<double>(ref happinessLostBase, "happinessLostBase"); //0.0 - 2.0;     Affects how much happiness is lost
        Scribe_Values.Look<double>(ref happinessGainedBase, "happinessGainedBase"); //0.0 - 2.0    Affects how much happiness is gained
        Scribe_Values.Look<double>(ref loyaltyLostBase, "loyaltyLostBase"); //0.0 - 2.0;         Affects how much loyalty is lost
        Scribe_Values.Look<double>(ref loyaltyGainedBase, "loyaltyGainedBase"); //0.0 - 2.0;         Affects how much loyalty is gained
        Scribe_Values.Look<double>(ref unrestLostBase, "unrestLostBase");  //0.0 - 2.0;         Affects how much unrest is lost
        Scribe_Values.Look<double>(ref unrestGainedBase, "unrestGainedBase"); //0.0 - 2.0;         Affects how much loyalty is gained

        //Social Stats Multipliers
        Scribe_Values.Look<double>(ref happinessLostMultiplier, "happinessLostMultiplier"); //0.0 - 2.0;     Affects how much happiness is lost
        Scribe_Values.Look<double>(ref happinessGainedMultiplier, "happinessGainedMultiplier"); //0.0 - 2.0    Affects how much happiness is gained
        Scribe_Values.Look<double>(ref loyaltyLostMultiplier, "loyaltyLostMultiplier"); //0.0 - 2.0;         Affects how much loyalty is lost
        Scribe_Values.Look<double>(ref loyaltyGainedMultiplier, "loyaltyGainedMultiplier"); //0.0 - 2.0;         Affects how much loyalty is gained
        Scribe_Values.Look<double>(ref unrestLostMultiplier, "unrestLostMultiplier");  //0.0 - 2.0;         Affects how much unrest is lost
        Scribe_Values.Look<double>(ref unrestGainedMultiplier, "unrestGainedMultiplier"); //0.0 - 2.0;         Affects how much loyalty is gained



            //Create Settlement Stats
        Scribe_Values.Look<double>(ref createSettlementBaseCost, "createSettlementBaseCost");  //affects how much it costs to create a settlement
        Scribe_Values.Look<double>(ref createSettlementMultiplier, "createSettlementMultiplier");  //affects how much it costs to create a settlement

            //Faction Pawn Required Traits
        Scribe_Collections.Look<TraitDef>(ref forcedFactionPawnTraits, "forcedFactionPawnTraits", LookMode.Deep);   //Traits that pawns are required to have
        Scribe_Collections.Look<PawnKindDef>(ref factionAllowedRaces, "factionAllowedRaces", LookMode.Deep);  //List of races allowed in the faction
        Scribe_Collections.Look<Thing>(ref factionUniform, "factionUniform", LookMode.Deep); //List of uniform items for the faction
        }


    }

    [DefOf]
    public class FCTraitEffectDefOf
    {

        static FCTraitEffectDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FCTraitEffectDefOf));
        }
    }


}
