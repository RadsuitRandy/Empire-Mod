using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace FactionColonies
{

    public static class FCEventMaker
    {
        public static void calculateSuccess(FCOptionDef option, FCEvent parentEvent)
        {
            float baseChance = option.baseChanceOfSuccess;
            int roll = Rand.Range(1, 100);
            //Log.Message(roll.ToString());

            FCEvent tempEvent = new FCEvent(true);


            if (roll <= baseChance)
            { //if success
                if (option.parentEvent.settlementsCarryOver == true)
                {
                    tempEvent = FCEventMaker.MakeRandomEvent(option.successEvent, parentEvent.settlementTraitLocations);
                } else
                {
                    tempEvent = FCEventMaker.MakeRandomEvent(option.successEvent, null);
                }
            } else
            {
                if (option.parentEvent.settlementsCarryOver == true)
                {
                    tempEvent = FCEventMaker.MakeRandomEvent(option.failEvent, parentEvent.settlementTraitLocations);
                }
                else
                {
                    tempEvent = FCEventMaker.MakeRandomEvent(option.failEvent, null);
                }
            }
            if (tempEvent.def != FCEventDefOf.Null)
            {
                Find.World.GetComponent<FactionFC>().addEvent(tempEvent);

                //letter

                string settlementString = "";
                foreach (SettlementFC loc in tempEvent.settlementTraitLocations)
                {
                    if (settlementString == "")
                    {
                        settlementString = settlementString + loc.name;
                    }
                    else
                    {
                        settlementString = settlementString + ", " + loc.name;
                    }
                }
                if (settlementString != "")
                {
                    Find.LetterStack.ReceiveLetter(tempEvent.def.label, tempEvent.def.desc + "\n This event is affecting the following settlements: " + settlementString, LetterDefOf.NeutralEvent);
                }
                else
                {
                    Find.LetterStack.ReceiveLetter(tempEvent.def.label, tempEvent.def.desc, LetterDefOf.NeutralEvent);
                }
            }
        }

        public static bool isValidRandomEvent(FCEventDef cEvent)
        {
        
                FactionFC tmp = Find.World.GetComponent<FactionFC>();

                if (cEvent.isRandomEvent == true && Find.World.PlayerWealthForStoryteller >= cEvent.requiredWealth)    
                {
                    //If meets happiness requirement
                    if (cEvent.minimumHappiness <= tmp.averageHappiness && tmp.averageHappiness <= cEvent.maximumHappiness)
                    {
                    //If meets loyalty requirement
                    if (cEvent.minimumLoyalty <= tmp.averageLoyalty && tmp.averageLoyalty <= cEvent.maximumLoyalty)
                        {
                        //If meets unrest requirement
                        if (cEvent.minimumUnrest <= tmp.averageUnrest && tmp.averageUnrest <= cEvent.maximumUnrest)
                            {
                            //If meets prosperity requirement
                            if (cEvent.minimumProsperity <= tmp.averageProsperity && tmp.averageProsperity <= cEvent.maximumProsperity)
                                {
                                if (cEvent.rangeSettlementsAffected.min == 0 && cEvent.rangeSettlementsAffected.max == 0 || Find.World.GetComponent<FactionFC>().settlements.Count() >= cEvent.rangeSettlementsAffected.min)
                                {
                                    //if doesn't require resource or if required resource has more than 1 production
                                    if (cEvent.requiredResource == null ? Find.World.GetComponent<FactionFC>().returnResource(cEvent.requiredResource).assignedWorkers > 0 : true || (cEvent.requiredResource == "research" && traitUtilsFC.returnResearchAmount() > 0))
                                    {
                                        //if event is not incompatible with any currently-running events
                                        foreach (FCEvent evt in Find.World.GetComponent<FactionFC>().events)
                                        {
                                            if (evt.def != null)
                                            {
                                                if (cEvent == evt.def)
                                                { //if there's already the same event running
                                                    return false;
                                                }
                                                foreach (FCEventDef inEvt in evt.def.incompatibleEvents)
                                                {
                                                    if (cEvent == inEvt)
                                                    { //if not compatible
                                                        return false;
                                                    }
                                                }
                                            }
                                        }

                                        //If there are no required biomes
                                        if (cEvent.applicableBiomes.Count() != 0)
                                        {
                                            foreach (string biome in cEvent.applicableBiomes)
                                            {
                                               // if(cEvent.)
                                            }
                                        }

                                        //else if compatible
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                } 
                                else
                                {
                                    return false;
                                }
                            }
                            else //if not meet prosperity requirement
                            {
                                return false;
                            }
                        }
                        else //if not meet unrest requirement
                        {
                            return false;
                        }
                    }
                    else //if not meet loyalty requirement
                    {
                        return false;
                    }
                }
                else //if not meet happiness requirement
                {
                    return false;
                }
            }
            else //if not a random event or enough player wealth
            {
                return false;
            }
        }
        public static FCEventDef returnRandomEvent()
        {
            //create new list
            List<FCEventDef>tmpEventList = new List<FCEventDef>();

            foreach (FCEventDef eventDef in DefDatabase<FCEventDef>.AllDefsListForReading)
            {
                if (isValidRandomEvent(eventDef))
                {
                    for (int i = 0; i < eventDef.weight; i++)
                    {
                        tmpEventList.Add(eventDef);
                        //Log.Message(eventDef.label);
                    }
                }

            }

            if (tmpEventList.Count() != 0)
            {
                return tmpEventList.RandomElement();
            }
            else
            {
                return null;
            }
        }
        public static FCEvent MakeEvent(FCEventDef def)
        {
            if(def == null) { return null; }
            FCEvent tempEvent = new FCEvent(true);
            tempEvent.def = def;
            tempEvent.timeTillTrigger = def.timeTillTrigger + Find.TickManager.TicksGame;
            return tempEvent;
        }

        public static FCEvent MakeRandomEvent(FCEventDef def, List<SettlementFC> SettlementTraitLocations)
        {
            FCEvent tempEvent = new FCEvent(true);
            tempEvent.def = def;
            tempEvent.timeTillTrigger = def.timeTillTrigger + Find.TickManager.TicksGame;
            tempEvent.traits = def.traits;
            tempEvent.settlementTraitLocations = new List<SettlementFC>();



            //if affects specific settlement(s) then get settlements.
            if (tempEvent.def.rangeSettlementsAffected.max != 0)
            {
                int numSettlements = tempEvent.def.rangeSettlementsAffected.RandomInRange;

                //Log.Message("Pre- " + numSettlements);
                //if random number of settlements more than total settlements, reset number settlements.
                if (numSettlements > Find.World.GetComponent<FactionFC>().settlements.Count())
                {
                    numSettlements = Find.World.GetComponent<FactionFC>().settlements.Count();
                    //Log.Message("Post- " + numSettlements);
                }

                //List of map locations
                List<SettlementFC> settlements = new List<SettlementFC>();
                //temporary list of settlemnts.
                List<SettlementFC> tmp = new List<SettlementFC>();

                if (SettlementTraitLocations == null || SettlementTraitLocations.Count == 0)
                {
                    foreach (SettlementFC settlement in Find.World.GetComponent<FactionFC>().settlements.InRandomOrder())
                    {
                        if (tempEvent.def.requiredResource != "")
                        { //if there is a required resource
                            if (settlement.returnResource(tempEvent.def.requiredResource).assignedWorkers > 0)
                            { //if have someone working on that resource
                                tmp.Add(settlement);
                            }
                        }
                        else
                        {
                            tmp.Add(settlement);
                        }
                    }

                    while (tmp.Count() > 0)
                    {
                        SettlementFC cSettlement = tmp.RandomElement();
                        if (settlements.Count() < numSettlements)
                        {
                            settlements.Add(cSettlement);
                        }
                        tmp.Remove(cSettlement);
                    }

                    tempEvent.settlementTraitLocations.AddRange(settlements);
                } else
                {
                    tempEvent.settlementTraitLocations.AddRange(SettlementTraitLocations);
                    //Log.Message(tempEvent.settlementTraitLocations.Count().ToString());
                }
                //foreach(int loc in tempEvent.settlementTraitLocations)
                //{
                    //Log.Message("Location: " + loc);
                //}
            }
            //if event has options
            //open event option window
            //Log.Message("option count: " + tempEvent.def.options.Count().ToString());
            if (tempEvent.def.options.Count > 0 && tempEvent.def.activateAtStart == true)
            {
                Find.WindowStack.Add(new FCOptionWindow(tempEvent.def, tempEvent));
                return null;
            } else
            {
                return tempEvent;
            }

            //

            
            
        }






        public static void ProcessEvents(in List<FCEvent> events)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            for (int i = 0; i < events.Count(); i++)
            {
                //Log.Message(events[i].timeTillTrigger.ToString());
                if (events[i].timeTillTrigger <= Find.TickManager.TicksGame)  //if due time past game time, do stuff
                { //Make custom event functions here

                    FCEvent temp = new FCEvent(true);
                    temp = events[i];
                    //remove event (stop spam?)
                    faction.events.RemoveAt(i);

                    if (temp.def.defName == "settleNewColony")
                    {  //Settle new colony event
                       //Log.Message(events[i].def.defName + " event triggered" + Find.TickManager.TicksGame)
                        faction.addExperienceToFactionLevel(10f);
                        if (Find.World.info.name == temp.planetName)
                        {
                            Settlement settlement = FactionColonies.createPlayerColonySettlement(temp.location, true, temp.planetName);
                        } else
                        {
                            Settlement settlement = FactionColonies.createPlayerColonySettlement(temp.location, false, temp.planetName);
                            faction.createSettlementQueue.Add(new SettlementSoS2Info(temp.planetName, temp.location));
                        }
                        faction.settlementCaravansList.Remove(temp.location.ToString());
                    }
                    else
                    if (temp.def.defName == "taxColony")
                    {
                        Messages.Message("TaxesFrom".Translate() + " " + faction.getSettlementName(temp.source, temp.planetName) + " " + "HaveBeenDelivered".Translate() + "!", MessageTypeDefOf.PositiveEvent);
                        string str = "TaxesFrom".Translate() + " " + faction.getSettlementName(temp.source, temp.planetName) + " " + "HaveBeenDelivered".Translate() + "!";
                        
                        foreach (Thing thing in temp.goods)
                        {
                            str = str + "\n" + thing.LabelCap;
                        }
                        Find.LetterStack.ReceiveLetter("TaxesHaveArrived".Translate(), str, LetterDefOf.PositiveEvent);
                        PaymentUtil.deliverThings(temp);
                    }
                    else
                    if (temp.def.defName == "constructBuilding")
                    { //Create building
                        faction.settlements[faction.returnSettlementFCIDByLocation(temp.source, temp.planetName)].constructBuilding(temp.building, temp.buildingSlot);
                        Messages.Message(temp.building.label + " " + "HasBeenConstructedAt".Translate() + " " + faction.settlements[faction.returnSettlementFCIDByLocation(temp.source, temp.planetName)].name + "!", MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    if (temp.def.defName == "upgradeSettlement")
                    {
                        if (faction.returnSettlementByLocation(temp.location, temp.planetName) != null) 
                        { //if settlement is not null
                            SettlementFC settlement = faction.returnSettlementByLocation(temp.location, temp.planetName);
                            settlement.upgradeSettlement();
                            Find.LetterStack.ReceiveLetter("Settlement Upgrade", settlement.name + " " + "HasBeenUpgraded".Translate() + " " + settlement.settlementLevel + "!", LetterDefOf.PositiveEvent);
                        }
                    }
                    else
                    if( temp.def.defName == "captureEnemySettlement" || temp.def.defName == "raidEnemySettlement" || temp.def.defName == "enslaveEnemySettlement")
                    {
                        //Process military event
                        faction.returnSettlementByLocation(temp.location, temp.planetName).processMilitaryEvent();


                    }
                    else
                    if (temp.def.defName == "cooldownMilitary")
                    {
                        //Process military event
                        if (temp.planetName == null)
                        {
                            Log.Message("temp.planetName null in FCEvent.ProcessEvents. Please report to Empire Mod with what you used this settlement for.");
                            Messages.Message("temp.planetName null in FCEvent.ProcessEvents. Please report to Empire Mod with what you used this settlement for.", MessageTypeDefOf.NegativeEvent);
                        }
                        faction.returnSettlementByLocation(temp.location, temp.planetName).returnMilitary(true);
                        


                    }
                    if (temp.def.defName == "settlementBeingAttacked")
                    {
                        //Log.Message("Process Event - SettlementBeingAttacked");
                        //Process military event
                        temp.settlementFCDefending.isUnderAttack = false;


                        int winner = simulateBattleFC.FightBattle(temp.militaryForceAttacking, temp.militaryForceDefending);
                        if (winner == 1)
                        {
                            faction.addExperienceToFactionLevel(5f);
                            //if winner is player
                            Find.LetterStack.ReceiveLetter("DefenseSuccessful".Translate(), TranslatorFormattedStringExtensions.Translate("DefenseSuccessfulFull", temp.settlementFCDefending.name, temp.militaryForceAttackingFaction.Name), LetterDefOf.PositiveEvent, new LookTargets(Find.WorldObjects.SettlementAt(temp.settlementFCDefending.mapLocation)));
                        } else
                        {
                            //get multipliers
                            double happinessLostMultiplier = (traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", temp.settlementFCDefending.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply"));
                            double loyaltyLostMultiplier = (traitUtilsFC.cycleTraits(new double(), "loyaltyLostMultiplier", temp.settlementFCDefending.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "loyaltyLostMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply"));

                            int trait_Muliplier = 1;
                            if (faction.hasPolicy(FCPolicyDefOf.feudal))
                                trait_Muliplier = 2;
                            float trait_Prosperity_Multiplier = 1;
                            bool trait_CanDestroyBuildings = true;
                            if (faction.hasTrait(FCPolicyDefOf.resilient))
                            {
                                trait_Prosperity_Multiplier = .5f;
                                trait_CanDestroyBuildings = false;
                            }
                            //if winner are enemies
                            temp.settlementFCDefending.prosperity -= (20 * trait_Prosperity_Multiplier);
                            temp.settlementFCDefending.happiness -= (25 * happinessLostMultiplier);
                            temp.settlementFCDefending.loyalty -= (15 * loyaltyLostMultiplier * trait_Muliplier);

                            string str = TranslatorFormattedStringExtensions.Translate("DefenseFailureFull", temp.settlementFCDefending.name,temp.militaryForceAttackingFaction.Name);


                            for (int k = 0; k < 4; k++)
                            {
                                int num = new IntRange(0, 10).RandomInRange;
                                if (num >= 7 && temp.settlementFCDefending.buildings[k].defName != "Empty" && temp.settlementFCDefending.buildings[k].defName != "Construction" && trait_CanDestroyBuildings)
                                {
                                    str += "\n" + TranslatorFormattedStringExtensions.Translate("BulidingDestroyedInRaid", temp.settlementFCDefending.buildings[k].label);
                                    temp.settlementFCDefending.deconstructBuilding(k);
                                   
                                }
                            }

                            //level remover checker
                            if (temp.settlementFCDefending.settlementLevel > 1 && trait_CanDestroyBuildings)
                            {
                                int num = new IntRange(0, 10).RandomInRange;
                                if (num >= 7)
                                {
                                    str += "\n\n" + "SettlementDeleveledRaid".Translate();
                                    temp.settlementFCDefending.delevelSettlement();

                                }
                            }

                            Find.LetterStack.ReceiveLetter("DefenseFailure".Translate(), str, LetterDefOf.Death, new LookTargets(Find.WorldObjects.SettlementAt(temp.settlementFCDefending.mapLocation)));
                        }

                        if(temp.militaryForceDefending.homeSettlement != temp.settlementFCDefending)
                        {
                            //if not the home settlement defending
                            if (temp.militaryForceDefending.forceRemaining >= 7)
                            {
                                Find.LetterStack.ReceiveLetter("OverwhelmingVictory".Translate(), "OverwhelmingVictoryDesc".Translate(), LetterDefOf.PositiveEvent);
                                temp.militaryForceDefending.homeSettlement.returnMilitary(true);
                            } else 
                            { 
                                temp.militaryForceDefending.homeSettlement.cooldownMilitary();
                            }

                            //log
                            //Log.Message("Military force is being returned from settlement " + temp.militaryForceDefending.homeSettlement.name);
                        } else
                        {
                            //if home settlement
                            //Log.Message("Defending force was home settlement");
                        }


                    }
                    else //if undefined event
                    {
                      

                        if (temp.def.goods.Count > 0)
                        {
                            PaymentUtil.deliverThings(temp);
                        }

                       // Log.Message(temp.def.label + " " + temp.def.randomThingValue + " value:thing " + temp.def.randomThingType);
                        if(temp.def.randomThingValue > 0 && temp.def.randomThingType != "")
                        {
                            List<Thing> list = PaymentUtil.generateThing(temp.def.randomThingValue, temp.def.randomThingType);

                            string str;
                            str = TranslatorFormattedStringExtensions.Translate("GoodsReceivedFollowing", temp.def.label);
                            foreach (Thing thing in list)
                            {
                                str = str + "\n" + thing.LabelCap;
                            }
                            Find.LetterStack.ReceiveLetter("GoodsReceived".Translate(), str, LetterDefOf.PositiveEvent);
                            PaymentUtil.deliverThings(list);
                        }
                    }

                    //If has loot to give
                    if (temp.def.loot.Count() > 0)
                    {
                        List<Thing> list = new List<Thing>();

                        foreach (ThingDef thing in temp.def.loot)
                        {
                            list.Add(ThingMaker.MakeThing(thing));
                        }

                        PaymentUtil.deliverThings(list);
                    }

                    if (temp.loot.Count() > 0)
                    {
                        List<Thing> list = new List<Thing>();

                        foreach (ThingDef thing in temp.loot)
                        {
                            //list.Add(ThingMaker.MakeThing(thing));
                        }

                        PaymentUtil.deliverThings(list);
                    }


                    //check if event has a location, if does, add traits to that specific location;
                    if (temp.settlementTraitLocations.Count() > 0) //if has specific locations
                    {
                        //Remove null settlements
                        ResetClear:
                        foreach (SettlementFC settlement in temp.settlementTraitLocations)
                        {
                            if (settlement == null)
                            {
                                temp.settlementTraitLocations.Remove(settlement);
                                goto ResetClear;
                            }
                        }

                        foreach (SettlementFC location in temp.settlementTraitLocations)
                        {
                            if (location != null)
                            {
                                foreach (FCTraitEffectDef trait in temp.traits)
                                {

                                    //Log.Message(trait.label);
                                    while (location.traits.Contains(trait) == true)
                                    {
                                        location.traits.Remove(trait);
                                    }
                                }

                                //prosperity loss calculation
                                location.prosperity -= temp.prosperityLost;
                            }
                        }
                    }
                    else
                    { //if no specific location then faction wide

                        foreach (FCTraitEffectDef trait in temp.traits)
                        {
                            while(faction.traits.Contains(trait) == true)
                            {
                                faction.traits.Remove(trait);
                            }
                        }

                        foreach (SettlementFC settlement in faction.settlements)
                        {
                            settlement.prosperity -= temp.prosperityLost;
                        }
                    }

                    //if have options
                    if (temp.def != null && temp.def.options.Count > 0 && temp.def.activateAtStart == false)
                    {
                        Find.WindowStack.Add(new FCOptionWindow(temp.def, temp));
                    }

                    //if has following event
                    if (temp.def.eventFollows == true)
                    {
             
                        FCEvent tempEvent = new FCEvent(true);
                        if (temp.def.splitEventFollows == true) //if a split event
                        {
                            //remove null settlement references
                            float baseChance = temp.def.splitEventChance;
                            int roll = Rand.Range(1, 100);
                            if (temp.def.settlementsCarryOver == true)
                            { //if settlements carry
                                if (roll <= baseChance)
                                { //first event
                                    tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent, temp.settlementTraitLocations);
                                }
                                else
                                { //if second event
                                    tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent2, temp.settlementTraitLocations);
                                }
                            } else
                            {
                                if (roll <= baseChance)
                                { //first event
                                    tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent, null);
                                }
                                else
                                { //if second event
                                    tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent2, null);
                                }
                            }
                        }
                        else
                        {
                            if (temp.def.settlementsCarryOver == true)
                            { //if settlements carry
                                tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent, temp.settlementTraitLocations);
                            } else
                            {
                                tempEvent = FCEventMaker.MakeRandomEvent(temp.def.followingEvent, null);
                            }
                        }




                        if (tempEvent != null)
                        {
                            faction.addEvent(tempEvent);
                        


                        //letter

                        string settlementString = "";
                        foreach (SettlementFC loc in tempEvent.settlementTraitLocations)
                        {
                            //Log.Message(loc.ToString());
                            if (settlementString == "")
                            {
                                settlementString = settlementString + " " + loc.name;
                            }
                            else
                            {
                                settlementString = settlementString + ", " + loc.name;
                            }
                        }
                        if (settlementString != "")
                        {
                            Find.LetterStack.ReceiveLetter(tempEvent.def.label, tempEvent.def.desc + "\n" + "EventAffectingSettlements".Translate() + settlementString, LetterDefOf.NeutralEvent);
                        }
                        else
                        {
                            Find.LetterStack.ReceiveLetter(tempEvent.def.label, tempEvent.def.desc, LetterDefOf.NeutralEvent);
                        }
                        }
                    }

                    temp.runAction();


                }
            }
        }





        public static void createTaxEvent(BillFC bill)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();

            FCEvent tmp = FCEventMaker.MakeEvent(FCEventDefOf.taxColony);

            if (bill.settlement != null && faction.settlements.Contains(bill.settlement))
            {
                tmp.source = bill.settlement.mapLocation; //source location
                tmp.customDescription = TranslatorFormattedStringExtensions.Translate("TaxesFromSettlementAreBeingDelivered", bill.settlement.name);
                tmp.planetName = bill.settlement.planetName;
            } else
            {
                tmp.source = -1;
                tmp.planetName = Find.World.info.name;
                tmp.customDescription = TranslatorFormattedStringExtensions.Translate("TaxesFromSettlementAreBeingDelivered", "Null");
            }
            tmp.location = faction.capitalLocation;
            tmp.timeTillTrigger = Find.TickManager.TicksGame + FactionColonies.ReturnTicksToArrive(tmp.source, tmp.location);
            tmp.hasCustomDescription = true;
            //add tithe
            tmp.goods = bill.taxes.itemTithes;

            if (bill.taxes.silverAmount > 0) //if getting paid, add silver to tithe
            {//add to tithe
             //tmp.goods.Add()
                int silverTotal = (int)bill.taxes.silverAmount;
             while (silverTotal > 0)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver, null);

                    if (silverTotal > thing.def.stackLimit)
                    {
                        thing.stackCount = thing.def.stackLimit;
                        silverTotal -= thing.def.stackLimit;
                    } else
                    {
                        //if not above stack limit
                        thing.stackCount = silverTotal;
                        silverTotal -= silverTotal;

                    }
                    tmp.goods.Add(thing);
                }
            }
            else
            if (bill.taxes.silverAmount < 0)   //if paying money
            {//remove money from colony
                PaymentUtil.paySilver((int)(-1 * (bill.taxes.silverAmount)));
            }





            // add event to queue and remove bill
            if (tmp.goods.Count > 0) //if any silver or tithe in bill create event. else, well, don't
            {
                faction.addEvent(tmp);
            }
            faction.Bills.Remove(bill);
        }



    }

    public class FCEvent : IExposable, ILoadReferenceable
    {

        public FCEvent()
        {
            //Constructor
        }

        public FCEvent(bool New)
        {
            this.loadID = Find.World.GetComponent<FactionFC>().GetNextEventID();
        }

        public void ExposeData()
        {
            //Ref
            Scribe_Defs.Look<FCEventDef>(ref def, "def");
            Scribe_Values.Look<int>(ref location, "location");
            Scribe_Values.Look<string>(ref planetName, "planetName");
            Scribe_Values.Look<int>(ref timeTillTrigger, "timeTillTrigger");
            Scribe_Values.Look<int>(ref source, "source");
            Scribe_Values.Look<bool>(ref hasDestination, "hasDestination");
            Scribe_Collections.Look<SettlementFC>(ref settlementTraitLocations, "settlementTraitLocations", LookMode.Reference);
            Scribe_Collections.Look<Thing>(ref goods, "goods", LookMode.Deep);
            Scribe_Collections.Look<FCTraitEffectDef>(ref traits, "traits", LookMode.Def);
            Scribe_Values.Look<int>(ref loadID, "loadID");

            Scribe_Values.Look<int>(ref buildingSlot, "buildingSlot");

            Scribe_Defs.Look<BuildingFCDef>(ref building, "building");


            Scribe_Values.Look<bool>(ref hasCustomDescription, "hasCustomDescription");
            Scribe_Values.Look<string>(ref customDescription, "customDescription");

            //Random Events
            Scribe_Values.Look<bool>(ref isRandomEvent, "isRandomEvent");
            Scribe_Values.Look<bool>(ref perpetual, "perpetual");
            Scribe_Values.Look<bool>(ref activateAtStart, "activateatStart");
            Scribe_Collections.Look<Pawn>(ref pawnSpawn, "pawnSpawn", LookMode.Deep);
            Scribe_Values.Look<int>(ref requiredWealth, "requiredWealth");
            Scribe_Values.Look<IntRange>(ref rangeSettlementsAffected, "rangeSettlementsAffected");
            Scribe_Values.Look<bool>(ref settlementsCarryOver, "settlementsCarryOver");
            Scribe_Values.Look<int>(ref weight, "eventValue");
            Scribe_Values.Look<int>(ref minimumHappiness, "minimumHappiness");
            Scribe_Values.Look<int>(ref maximumHappiness, "maximumHappiness");
            Scribe_Values.Look<int>(ref minimumLoyalty, "minimumLoyalty");
            Scribe_Values.Look<int>(ref maximumLoyalty, "maximumLoyalty");
            Scribe_Values.Look<int>(ref minimumUnrest, "minimumUnrest");
            Scribe_Values.Look<int>(ref maximumUnrest, "maximumUnrest");
            Scribe_Values.Look<int>(ref minimumProsperity, "minimumProsperity");
            Scribe_Values.Look<int>(ref maximumProsperity, "maximumProsperity");
            Scribe_Values.Look<string>(ref requiredResource, "requiredResource");
            Scribe_Values.Look<int>(ref randomThingValue, "randomThingValue");
            Scribe_Values.Look<string>(ref randomThingType, "randomThingType");
            Scribe_Collections.Look<FCOptionDef>(ref options, "options", LookMode.Def);
            Scribe_Collections.Look<FCEventDef>(ref incompatibleEvents, "incompatibleEvents", LookMode.Def);
            Scribe_Values.Look<int>(ref prosperityLost, "prosperityLost");
            Scribe_Values.Look<bool>(ref eventFollows, "eventFollows");
            Scribe_Defs.Look<FCEventDef>(ref followingEvent, "followingEvent");
            Scribe_Defs.Look<FCEventDef>(ref followingEvent2, "followingEvent2");
            Scribe_Values.Look<bool>(ref splitEventFollows, "splitEventFol*lows");
            Scribe_Values.Look<int>(ref splitEventChance, "splitEventChance");
            Scribe_Values.Look<string>(ref optionDescription, "optionDescription");
            Scribe_Collections.Look<string>(ref applicableBiomes, "applicableBiomes", LookMode.Value);
            Scribe_Collections.Look<ThingDef>(ref loot, "loot", LookMode.Def);
            Scribe_Values.Look<string>(ref classToRun, "classToRun");
            Scribe_Values.Look<string>(ref classMethodToRun, "classMethodToRun");
            Scribe_Values.Look<bool>(ref passEventToClassMethodToRun, "passEventToClassMethodToRun");

            //Military stuff
            Scribe_Deep.Look<militaryForce>(ref militaryForceAttacking, "militaryForceAttacking");
            Scribe_References.Look<Faction>(ref militaryForceAttackingFaction, "militaryForceAttackingFaction");
            Scribe_Deep.Look<militaryForce>(ref militaryForceDefending, "militaryForceDefending");
            Scribe_References.Look<Faction>(ref militaryForceDefendingFaction, "militaryForceDefendingFaction");
            Scribe_References.Look<SettlementFC>(ref settlementFCDefending, "SettlementFCDefending");
            Scribe_Values.Look<bool>(ref isMilitaryEvent, "isMilitaryEvent");


        }

        public FCEventDef def = new FCEventDef();
        public int location = -1; //destination
        public string planetName;
        public int timeTillTrigger = -1;
        public int loadID = -1;
        public int source = -1; //source location
        public bool hasDestination = false; //if has destination
        public int buildingSlot = -1;
        public BuildingFCDef building = null;
        public List<SettlementFC> settlementTraitLocations = new List<SettlementFC>();
        public List<Thing> goods = new List<Thing>();
        public List<FCTraitEffectDef> traits = new List<FCTraitEffectDef>();
        public bool hasCustomDescription = false;
        public string customDescription = "";
        




        //Random Event Information
        public bool isRandomEvent = false;
        public bool perpetual = false;
        public bool activateAtStart;
        public List<Pawn> pawnSpawn = new List<Pawn>();
        public int requiredWealth = 0;
        public IntRange rangeSettlementsAffected = new IntRange(0, 0);
        public bool settlementsCarryOver = true;
        public int weight = 0;
        public int minimumHappiness = 0;
        public int maximumHappiness = 100;
        public int minimumLoyalty = 0;
        public int maximumLoyalty = 100;
        public int minimumUnrest = 0;
        public int maximumUnrest = 100;
        public int minimumProsperity = 0;
        public int maximumProsperity = 100;
        public List<FCOptionDef> options = new List<FCOptionDef>();
        public string requiredResource = "";
        public int randomThingValue = 0;
        public string randomThingType = "";
        public List<FCEventDef> incompatibleEvents = new List<FCEventDef>();
        public int prosperityLost = 0;
        public bool eventFollows = false;
        public FCEventDef followingEvent = null;
        public FCEventDef followingEvent2 = null;
        public bool splitEventFollows = false;
        public int splitEventChance = 50;
        public string optionDescription = "";
        public List<string> applicableBiomes = new List<string>();
        public List<ThingDef> loot = new List<ThingDef>();
        public string classToRun = "";
        public string classMethodToRun = "";
        public bool passEventToClassMethodToRun = false;

        //Military Force stuff
        public militaryForce militaryForceAttacking = null;
        public Faction militaryForceAttackingFaction = null;
        public militaryForce militaryForceDefending = null;
        public Faction militaryForceDefendingFaction = null;
        public SettlementFC settlementFCDefending = null;
        public bool isMilitaryEvent = false;


        public string GetUniqueLoadID()
        {
            return "FCEvent_" + this.loadID;
        }

        public void runAction()
        {
            if (!this.classToRun.NullOrEmpty() && !this.classMethodToRun.NullOrEmpty())
            {
                Type typ = null;
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type1 = a.GetType(this.classToRun);
                    if (type1 != null)
                        typ = type1;
                }

                var obj = Activator.CreateInstance(typ);
                object[] paramArgu;
                switch (this.passEventToClassMethodToRun)
                {
                    case true:
                        paramArgu = new object[] { this };
                        break;
                    case false:
                    default:
                        paramArgu = new object[] { };
                        break;
                }
                Traverse.Create(obj).Method(this.classMethodToRun, paramArgu).GetValue();
            }
        }
    }




    public class FCEventDef : Def
    {

        public FCEventDef()
        {
            //Constructor
        }

        //Event main info
        public int timeTillTrigger = -1;
        public string desc;

        //Random Event Information
        public bool isRandomEvent = false;
        public bool perpetual = false;
        public bool activateAtStart;
        public List<Pawn> pawnSpawn = new List<Pawn>();
        public int requiredWealth = 0;
        public IntRange rangeSettlementsAffected = new IntRange(0, 0);
        public bool settlementsCarryOver = true;
        public int weight = 0;
        public int minimumHappiness = 0;
        public int maximumHappiness = 100;
        public int minimumLoyalty = 0;
        public int maximumLoyalty = 100;
        public int minimumUnrest = 0;
        public int maximumUnrest = 100;
        public int minimumProsperity = 0;
        public int maximumProsperity = 100;
        public  List<FCOptionDef> options = new List<FCOptionDef>();
        public string requiredResource = "";
        public int randomThingValue = 0;
        public string randomThingType = "";
        public List<FCEventDef> incompatibleEvents = new List<FCEventDef>();
        public int prosperityLost = 0;
        public bool eventFollows = false;
        public FCEventDef followingEvent = null;
        public FCEventDef followingEvent2 = null;
        public bool splitEventFollows = false;
        public int splitEventChance = 50;
        public string optionDescription = "";
        public List<string> applicableBiomes = new List<string>();
        public List<ThingDef> loot = new List<ThingDef>();
        public bool hasCustomDescription = false;
        public string customDescription = "";
        public string classToRun;
        public string classMethodToRun;
        public bool passEventToClassMethodToRun;


        //Map info
        public int location = -1;
        public bool hasDestination = false;


        //Traits during event
        public List<FCTraitEffectDef> traits = new List<FCTraitEffectDef>();
        

        //Benefits after eventtime info
        public List<Thing> goods = new List<Thing>();

        //Military Force stuff
        public militaryForce militaryForceAttacking = null;
        public Faction militaryForceAttackingFaction = null;
        public militaryForce militaryForceDefending = null;
        public Faction militaryForceAttackingDefending = null;
        public SettlementFC settlementFCDefending = null;
        public bool isMilitaryEvent = false;


    }

    [DefOf]
    public class FCEventDefOf
    {
        //List Events here - loads events at start
        //public static FCEventDef settleNewColony;
        public static FCEventDef Null;
        public static FCEventDef settleNewColony;
        public static FCEventDef taxColony;
        public static FCEventDef constructBuilding;
        public static FCEventDef enactSettlementPolicy;
        public static FCEventDef enactFactionPolicy;
        public static FCEventDef upgradeSettlement;
        public static FCEventDef raidEnemySettlement;
        public static FCEventDef enslaveEnemySettlement;
        public static FCEventDef captureEnemySettlement;
        public static FCEventDef cooldownMilitary;
        public static FCEventDef settlementBeingAttacked;

        static FCEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FCEventDefOf));
        }
    }
}
