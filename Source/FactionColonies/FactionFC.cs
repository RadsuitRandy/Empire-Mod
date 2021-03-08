using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using Verse.AI;
using Verse.AI.Group;
using System.IO;
using System.Reflection;


namespace FactionColonies
{

    public class FactionFC : WorldComponent
    {
        public int eventTimeDue = 0;
        public int taxTimeDue = Find.TickManager.TicksGame;
        public int timeStart = Find.TickManager.TicksGame;
        public int uiTimeUpdate = 0;
        public int dailyTimer = Find.TickManager.TicksGame;
        public int militaryTimeDue = 0;
        public int mercenaryTick = 0;
        public bool factionCreated = false;
       

        public List<SettlementFC> settlements = new List<SettlementFC>();
        public string name = "PlayerFaction".Translate();
        public string title = "Bastion".Translate();
        public double averageHappiness = 100;
        public double averageLoyalty = 100;
        public double averageUnrest = 0;
        public double averageProsperity = 100;
        public double income = 0;
        public double upkeep = 0;
        public double profit = 0;
        public int capitalLocation = -1;
        public string capitalPlanet;
        public Map taxMap;
        public TechLevel techLevel = TechLevel.Undefined;
        private bool firstTick = true;
        public Texture2D factionIcon = texLoad.factionIcons.ElementAt(0);
        public string factionIconPath = texLoad.factionIcons.ElementAt(0).name;


        //New Types of PRoductions
        public float researchPointPool = 0;
        public float powerPool = 0;
        public ThingWithComps powerOutput;

        public List<FCEvent> events = new List<FCEvent>();
        public List<String> settlementCaravansList = new List<string>(); //list of locations caravans already sent to

        public List<BillFC> OldBills = new List<BillFC>();
        public List<BillFC> Bills = new List<BillFC>();
        public bool autoResolveBills = false;
        public bool autoResolveBillsChanged = false;

        public List<FCPolicy> policies = new List<FCPolicy>();
        public List<FCTraitEffectDef> traits = new List<FCTraitEffectDef>();
        public List<int> militaryTargets = new List<int>();
        public ThingFilter raceFilter = new ThingFilter();


        //Faction resources
        public ResourceFC food = new ResourceFC("food", "Food", 1);
        public ResourceFC weapons = new ResourceFC("weapons", "Weapons", 1);
        public ResourceFC apparel = new ResourceFC("apparel", "Apparel", 1);
        public ResourceFC animals = new ResourceFC("animals", "Animals", 1);
        public ResourceFC logging = new ResourceFC("logging", "Logging", 1);
        public ResourceFC mining = new ResourceFC("mining", "Mining", 1);
        //public ResourceFC research = new ResourceFC("researching", "Researching", 1);
        public ResourceFC power = new ResourceFC("power", "Power", 1);
        public ResourceFC medicine = new ResourceFC("medicine", "Medicine", 1);
        public ResourceFC research = new ResourceFC("research", "Research", 1);

        //Faction Def
        public FactionFCDef factionDef = new FactionFCDef();

        //Update
        public double updateVersion = 0;
        public int nextSettlementFCID = 1;
        public int nextMilitaryForceID = 1;
        public int nextMercenarySquadID = 1;
        public int nextMercenaryID = 1;
        public int nextTaxID = 1;
        public int nextBillID = 1;
        public int nextEventID = 1;
        public int nextPrisonerID = 1;

        //Military 
        public int nextUnitID = 1;
        public int nextSquadID = 1;
        public int nextMilitaryFireSupportID = 1;

        //Military Customization
        public MilitaryCustomizationUtil militaryCustomizationUtil = new MilitaryCustomizationUtil();

        //Sos2 Compatibility
        public Faction factionBackup = null;
        public int travelTime = 0;
        public string planetName;
        public bool boolChangedPlanet = false;
        public bool factionUpdated = false;
        public bool SoSMoving = false;
        public bool SoSShipTaxMap = false;
        public bool SoSShipCapital = false;
        public bool SoSShipCapitalMoving = false;
        public List<SettlementSoS2Info> createSettlementQueue = new List<SettlementSoS2Info>();
        public List<SettlementSoS2Info> deleteSettlementQueue = new List<SettlementSoS2Info>();

        //Road builder
        public FCRoadBuilder roadBuilder = new FCRoadBuilder();

        public int traitMilitaristicTickLastUsedExtraSquad = -1;
        //Traits
        public int traitPacifistTickLastUsedDiplomat = -1;
        public int traitExpansionistTickLastUsedSettlementFeeReduction = -1;
        public bool traitExpansionistBoolCanUseSettlementFeeReduction = true;
        public int traitFeudalTickLastUsedMercenary = -1;
        public bool traitFeudalBoolCanUseMercenary = true;
        public int traitMercantileTradeCaravanTickDue = -1;

        //Settlement Leveling
        public int factionLevel = 1;
        public float factionXPCurrent = 0;
        public float factionXPGoal = 100;
        public List<FCPolicy> factionTraits = new List<FCPolicy>() { new FCPolicy(FCPolicyDefOf.empty) , new FCPolicy(FCPolicyDefOf.empty) , new FCPolicy(FCPolicyDefOf.empty) , new FCPolicy(FCPolicyDefOf.empty) , new FCPolicy(FCPolicyDefOf.empty) };

        //Research Trading
        public float tradedAmount = 0;

        //Call for aid
        // [HarmonyPatch(typeof(FactionDialogMaker), "CallForAid")]
        // class WorldObjectGizmos
        //{
        //    static void Prefix(Map map, Faction faction)
        //    {

        //    }
        // }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref name, "name");
            Scribe_Values.Look<string>(ref title, "title");
            Scribe_Values.Look<int>(ref capitalLocation, "capitalLocation");
            Scribe_Values.Look<string>(ref capitalPlanet, "capitalPlanet");
            Scribe_References.Look<Map>(ref taxMap, "taxMap");
            Scribe_Values.Look<bool>(ref factionCreated, "factionCreated", false);

            Scribe_Values.Look<double>(ref averageHappiness, "averageHappiness");
            Scribe_Values.Look<double>(ref averageLoyalty, "averageLoyalty");
            Scribe_Values.Look<double>(ref averageUnrest, "averageUnrest");
            Scribe_Values.Look<double>(ref averageProsperity, "averageProsperity");

            Scribe_Values.Look<double>(ref income, "income");
            Scribe_Values.Look<double>(ref upkeep, "upkeep");
            Scribe_Values.Look<double>(ref profit, "profit");

            Scribe_Values.Look<int>(ref eventTimeDue, "eventTimeDue");
            Scribe_Values.Look<int>(ref taxTimeDue, "taxTimeDue");
            Scribe_Values.Look<int>(ref timeStart, "timeStart", -1);
            Scribe_Values.Look<int>(ref uiTimeUpdate, "uiTimeUpdate");
            Scribe_Values.Look<int>(ref militaryTimeDue, "militaryTimeDue", -1);
            Scribe_Values.Look<int>(ref dailyTimer, "dailyTimer");
            Scribe_Values.Look<TechLevel>(ref techLevel, "techLevel");
            Scribe_Values.Look<string>(ref factionIconPath, "factionIconPath", "Base");

            Scribe_Collections.Look<SettlementFC>(ref settlements, "settlements", LookMode.Deep);
            Scribe_Collections.Look<FCPolicy>(ref policies, "factionPolicies", LookMode.Deep);
            Scribe_Collections.Look<FCEvent>(ref events, "events", LookMode.Deep);
            Scribe_Collections.Look<string>(ref settlementCaravansList, "settlementCaravansList", LookMode.Value);
            Scribe_Collections.Look<FCTraitEffectDef>(ref traits, "traits", LookMode.Def);
            Scribe_Collections.Look<int>(ref militaryTargets, "militaryTargets", LookMode.Value);

            //New Producitons types
            Scribe_Values.Look<float>(ref researchPointPool, "researchPointPool", 0);
            Scribe_Values.Look<float>(ref powerPool, "powerPool", 0);
            Scribe_References.Look<ThingWithComps>(ref powerOutput, "powerOutput");



            //save resources
            Scribe_Deep.Look<ResourceFC>(ref food, "food");
            Scribe_Deep.Look<ResourceFC>(ref weapons, "weapons");
            Scribe_Deep.Look<ResourceFC>(ref apparel, "apparel");
            Scribe_Deep.Look<ResourceFC>(ref animals, "animals");
            Scribe_Deep.Look<ResourceFC>(ref logging, "logging");
            Scribe_Deep.Look<ResourceFC>(ref mining, "mining");
            Scribe_Deep.Look<ResourceFC>(ref research, "research");
            Scribe_Deep.Look<ResourceFC>(ref power, "power");
            Scribe_Deep.Look<ResourceFC>(ref medicine, "medicine");

            //Faction Def
            Scribe_Deep.Look<FactionFCDef>(ref factionDef, "factionDef");
            Scribe_Deep.Look<ThingFilter>(ref raceFilter, "raceFilter");

            //Update
            Scribe_Values.Look<double>(ref updateVersion, "updateVersion");
            Scribe_Values.Look<int>(ref nextSettlementFCID, "nextSettlementFCID", 0);

            //Military Customization Util
            Scribe_Deep.Look<MilitaryCustomizationUtil>(ref militaryCustomizationUtil, "militaryCustomizationUtil");
            Scribe_Values.Look<int>(ref nextSquadID, "nextSquadID", 1);
            Scribe_Values.Look<int>(ref nextUnitID, "nextUnitID", 1);
            Scribe_Values.Look<int>(ref nextMilitaryFireSupportID, "nextMilitaryFireSupportID", 1);
            Scribe_Values.Look<int>(ref nextMercenaryID, "nextMercenaryID", 1);
            Scribe_Values.Look<int>(ref nextMercenarySquadID, "nextMercenarySquadID", 1);
            Scribe_Values.Look<int>(ref mercenaryTick, "mercenaryTick", -1);
            Scribe_Values.Look<int>(ref nextPrisonerID, "nextPrisonerID", 1);

            //New Tax Stuff
            Scribe_Values.Look<int>(ref nextTaxID, "nextTaxID", 1);
            Scribe_Values.Look<int>(ref nextBillID, "nextBillID", 1);
            Scribe_Values.Look<int>(ref nextEventID, "nextEventID", 1);


            Scribe_Collections.Look<BillFC>(ref Bills, "Bills", LookMode.Deep);
            Scribe_Collections.Look<BillFC>(ref OldBills, "OldBills", LookMode.Deep);
            Scribe_Values.Look<bool>(ref autoResolveBills, "autoResolveBills", false);

            //Sos2 compatibility
            //Scribe_Deep.Look<Faction>(ref factionBackup, "factionBackup");
            Scribe_Values.Look<bool>(ref SoSShipCapital, "SoSShipCapital");
            Scribe_Values.Look<bool>(ref SoSShipTaxMap, "SoSShipTaxMap");
            Scribe_Values.Look<string>(ref planetName, "planetName");
            Scribe_Collections.Look<SettlementSoS2Info>(ref createSettlementQueue, "createSettlementQueue", LookMode.Deep);
            Scribe_Collections.Look<SettlementSoS2Info>(ref deleteSettlementQueue, "deleteSettlementQueue", LookMode.Deep);

            //Road builder
            Scribe_Deep.Look<FCRoadBuilder>(ref roadBuilder, "roadBuilder");

            //Traits
            Scribe_Values.Look<int>(ref traitMilitaristicTickLastUsedExtraSquad, "traitMilitaristicTickLastUsedExtraSquad");
            Scribe_Values.Look<int>(ref traitPacifistTickLastUsedDiplomat, "traitPacifistTickLastUsedDiplomat");
            Scribe_Values.Look<int>(ref traitExpansionistTickLastUsedSettlementFeeReduction, "traitExpansionistTickLastUsedSettlementFeeReduction");
            Scribe_Values.Look<bool>(ref traitExpansionistBoolCanUseSettlementFeeReduction, "traitExpansionistBoolCanUseSettlementReduction");
            Scribe_Values.Look<int>(ref traitFeudalTickLastUsedMercenary, "traitFeudalTickLastUsedMercenary");
            Scribe_Values.Look<bool>(ref traitFeudalBoolCanUseMercenary, "traitFeudalBoolCanUseMercenary");
            Scribe_Values.Look<int>(ref traitMercantileTradeCaravanTickDue, "traitMercantileTradeCaravanTickDue");

            //Settlement Leveling
            Scribe_Values.Look<int>(ref factionLevel, "factionLevel");
            Scribe_Values.Look<float>(ref factionXPCurrent, "factionXPCurrent");
            Scribe_Values.Look<float>(ref factionXPGoal, "factionXPGoal");
            Scribe_Collections.Look<FCPolicy>(ref factionTraits, "factionTraits", LookMode.Deep);

            //Research Trading
            Scribe_Values.Look<float>(ref tradedAmount, "tradedAmount");
        }

        [HarmonyPatch(typeof(FactionDialogMaker), "RequestMilitaryAidOption")]
        class disableMilitaryAid
        {
            static void Postfix(Map map, Faction faction, Pawn negotiator, ref DiaOption __result)
            {
                if (faction.def.defName == "PColony")
                {
                    __result = new DiaOption("RequestMilitaryAid".Translate(25));
                    __result.Disable("Disabled. Use the settlements military tab.");
                }
            }
        }



        [HarmonyPatch(typeof(WorldPawns), "PassToWorld")]
        class MercenaryPassToWorld
        {
           
            static bool Prefix(Pawn pawn, PawnDiscardDecideMode discardMode = PawnDiscardDecideMode.Decide)
            {
                FactionFC faction = Find.World.GetComponent<FactionFC>();
                if (faction != null && faction.militaryCustomizationUtil != null)
                {
                    if (faction.militaryCustomizationUtil.AllMercenaryPawns != null && faction.militaryCustomizationUtil.AllMercenaryPawns.Contains(pawn))
                    {
                        //Don't pass
                        //Log.Message("POOf");
                     ///*   MercenarySquadFC squad = faction.militaryCustomizationUtil.returnSquadFromUnit(pawn);
                     //   if (squad.DeployedMercenaries.Count == 0 && squad.DeployedMercenaryAnimals.Count == 0)
                     //   {
                     //       Log.Message("Last pawn, removing Lord");
                     //      squad.hasLord = false;
                     //   }
                        return false;
                      
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Pawn), "GetGizmos")]
        class PawnGizmos
        {
            static void Postfix(ref Pawn __instance, ref IEnumerable<Gizmo> __result)
            {
                Pawn instance = __instance;
                if (__instance.guest != null)
                {
                    if (__instance.guest.IsPrisoner == true && __instance.guest.PrisonerIsSecure == true)
                    {
                        Pawn prisoner = __instance;


                        __result = __result.Concat(new[] { new Command_Action()
                        {
                        defaultLabel = "SendToSettlement".Translate(),
                        defaultDesc = "",
                        icon = texLoad.iconMilitary,
                        action = delegate
                        {
                            List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                            foreach (SettlementFC settlement in Find.World.GetComponent<FactionFC>().settlements)
                            {
                                settlementList.Add(new FloatMenuOption(settlement.name + " - Settlement Level : " + settlement.settlementLevel + " - Prisoners: " + settlement.prisonerList.Count(), delegate
                                {
                                    //disappear colonist
                                    FactionColonies.sendPrisoner(prisoner, settlement);

                                    foreach (Map map in Find.Maps)
                                    {
                                        if (map.IsPlayerHome)
                                        {
                                            foreach (Building building in map.listerBuildings.allBuildingsColonist)
                                            {
                                                if (building is Building_Bed)
                                                {
                                                    Building_Bed bed = (Building_Bed)building;
                                                    foreach (Pawn pawn in bed.OwnersForReading)
                                                    {
                                                        if (pawn == instance)
                                                        {
                                                            bed.ForPrisoners = false;
                                                            bed.ForPrisoners = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }));
                            }

                             FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                        floatMenu2.vanishIfMouseDistant = true;
                                         Find.WindowStack.Add(floatMenu2);
                        }
                    }
                    });
                    }
                }
            }
        }

        //stops friendly faction from being a group source
        [HarmonyPatch(typeof(IncidentWorker_RaidFriendly), "TryResolveRaidFaction")]
        class RaidFriendlyStopSettlementFaction
        {
            static void Postfix(ref IncidentWorker_RaidFriendly __instance, ref bool __result, IncidentParms parms)
            {
                if (parms.faction == FactionColonies.getPlayerColonyFaction())
                {
                    Log.Message("Prevented");
                    parms.faction = null;
                    __result = false;
                    return;
                }
            }
        }


            //Faction worldmap gizmos
            //Goodwill by distance to settlement
            [HarmonyPatch(typeof(WorldObject), "GetGizmos")]
        class WorldObjectGizmos
        {
            static void Postfix(ref WorldObject __instance, ref IEnumerable<Gizmo> __result)
            {
                FactionFC worldcomp = Find.World.GetComponent<FactionFC>();
                Faction fact = FactionColonies.getPlayerColonyFaction();
                if (__instance.def.defName == "Settlement")
                {
                    //if settlement
                    //Log.Message(fact.loadID + " vs " + __instance.Faction.loadID);
                    int tile = __instance.Tile;
                    if (__instance.Faction != fact && __instance.Faction != Find.FactionManager.OfPlayer)
                    {
                        //if a valid faction to target

                        Faction faction = __instance.Faction;
                        
                        string name = __instance.LabelCap;


                        Command_Action actionHostile = new Command_Action
                        {
                            defaultLabel = "AttackSettlement".Translate(),
                            defaultDesc = "",
                            icon = texLoad.iconMilitary,
                            action = delegate
                            {
                                List<FloatMenuOption> list = new List<FloatMenuOption>();

                                if (!worldcomp.hasPolicy(FCPolicyDefOf.isolationist))
                                    list.Add(new FloatMenuOption("CaptureSettlement".Translate(), delegate
                                {
                                    List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                                    foreach (SettlementFC settlement in worldcomp.settlements)
                                    {
                                        if (settlement.isMilitaryValid() == true)
                                        {
                                            //if military is valid to use.

                                            settlementList.Add(new FloatMenuOption(settlement.name + " " + "ShortMilitary".Translate() + " " + settlement.settlementMilitaryLevel + " - " + "FCAvailable".Translate() + ": " + (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                            {
                                                if (settlement.isMilitaryBusy() == true)
                                                {
                                                }
                                                else
                                                {

                                                    RelationsUtilFC.attackFaction(faction);

                                                    settlement.sendMilitary(tile, Find.World.info.name, "captureEnemySettlement", 60000, faction);


                                                    //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                                }
                                            }, MenuOptionPriority.Default, null, null, 0f, null, null
                                            ));
                                        }


                                    }

                                    if (settlementList.Count == 0)
                                    {
                                        settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(), null));
                                    }

                                    FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                    floatMenu2.vanishIfMouseDistant = true;
                                    Find.WindowStack.Add(floatMenu2);
                                }, MenuOptionPriority.Default, null, null, 0f, null, null));


                                list.Add(new FloatMenuOption("RaidSettlement".Translate(), delegate
                                {
                                    List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                                    foreach (SettlementFC settlement in worldcomp.settlements)
                                    {
                                        if (settlement.isMilitaryValid() == true)
                                        {
                                            //if military is valid to use.

                                            settlementList.Add(new FloatMenuOption(settlement.name + " " + "ShortMilitary".Translate() + " " + settlement.settlementMilitaryLevel + " - " + "FCAvailable".Translate() + ": " + (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                            {
                                                if (settlement.isMilitaryBusy() == true)
                                                {
                                                    //military is busy
                                                }
                                                else
                                                {

                                                    RelationsUtilFC.attackFaction(faction);

                                                    settlement.sendMilitary(tile, Find.World.info.name, "raidEnemySettlement", 60000, faction);


                                                    //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                                }
                                            }, MenuOptionPriority.Default, null, null, 0f, null, null
                                            ));
                                        }


                                    }

                                    if (settlementList.Count == 0)
                                    {
                                        settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(), null));
                                    }

                                    FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                    floatMenu2.vanishIfMouseDistant = true;
                                    Find.WindowStack.Add(floatMenu2);


                                    //set to raid settlement here
                                }, MenuOptionPriority.Default, null, null, 0f, null, null));

                                if (worldcomp.hasPolicy(FCPolicyDefOf.authoritarian)  && faction.def.defName != "VFEI_Insect")
                                {
                                        list.Add(new FloatMenuOption("EnslavePopulation".Translate(), delegate
                                    {
                                        List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                                        foreach (SettlementFC settlement in worldcomp.settlements)
                                        {
                                            if (settlement.isMilitaryValid() == true)
                                            {
                                                //if military is valid to use.

                                                settlementList.Add(new FloatMenuOption(settlement.name + " " + "ShortMilitary".Translate() + " " + settlement.settlementMilitaryLevel + " - " + "FCAvailable".Translate() + ": " + (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                                    {
                                                        if (settlement.isMilitaryBusy() == true)
                                                        {
                                                            //military is busy
                                                        }
                                                        else
                                                        {

                                                            RelationsUtilFC.attackFaction(faction);

                                                            settlement.sendMilitary(tile, Find.World.info.name, "enslaveEnemySettlement", 60000, faction);


                                                            //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                                        }
                                                    }, MenuOptionPriority.Default, null, null, 0f, null, null
                                                    ));
                                            }


                                        }

                                        if (settlementList.Count == 0)
                                        {
                                            settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(), null));
                                        }

                                        FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                        floatMenu2.vanishIfMouseDistant = true;
                                        Find.WindowStack.Add(floatMenu2);


                                        //set to raid settlement here
                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                }

                                FloatMenu floatMenu = new FloatMenu(list);
                                floatMenu.vanishIfMouseDistant = true;
                                Find.WindowStack.Add(floatMenu);
                            }
                        };

                        Command_Action actionPeaceful = new Command_Action
                        {
                            defaultLabel = "FCIncreaseRelations".Translate(),
                            defaultDesc = "",
                            icon = texLoad.iconProsperity,
                            action = delegate
                            {
                                worldcomp.sendDiplomaticEnvoy(faction);
                            }
                        };

                        if (worldcomp.hasPolicy(FCPolicyDefOf.pacifist))
                        {
                            __result = __result.Concat(new[] { actionPeaceful });
                        } else
                        {
                            __result = __result.Concat(new[] { actionHostile });
                        }
                       

                        
                    } 
                    
                    
                    else



                    {
                        if (__instance.Faction == fact)
                        {
                            //Log.Message(__instance.Tile.ToString());
                            //is a colony of the player faction
                            //Log.Message(Find.World.info.name);
                            SettlementFC test = worldcomp.returnSettlementByLocation(__instance.Tile, Find.World.info.name);
                            //Log.Message(((bool)(test != null)).ToString());
                            if (worldcomp.returnSettlementByLocation(__instance.Tile, Find.World.info.name).isUnderAttack == true)
                            {
                                //if settlement is under attack
                                __result = __result.Concat(new[] { new Command_Action()
                                {
                                    defaultLabel = "DefendSettlement".Translate(),
                                    defaultDesc = "",
                                    icon = texLoad.iconMilitary,
                                    action = delegate
                                    {
                                        List<FloatMenuOption> list = new List<FloatMenuOption>();

                                        
                                        FCEvent evt = MilitaryUtilFC.returnMilitaryEventByLocation(tile);
                                        list.Add(new FloatMenuOption(TranslatorFormattedStringExtensions.Translate("SettlementDefendingInformation", evt.militaryForceDefending.homeSettlement.name, evt.militaryForceDefending.militaryLevel), null, MenuOptionPriority.High));
                                        list.Add(new FloatMenuOption("ChangeDefendingForce".Translate(), delegate
                                        {
                                            List<FloatMenuOption> settlementList = new List<FloatMenuOption>();
                                            SettlementFC homeSettlement = worldcomp.returnSettlementByLocation(tile, evt.planetName);

                                            settlementList.Add(new FloatMenuOption(TranslatorFormattedStringExtensions.Translate("ResetToHomeSettlement", homeSettlement.settlementMilitaryLevel), delegate
                                            {
                                                MilitaryUtilFC.changeDefendingMilitaryForce(evt, homeSettlement);
                                            }, MenuOptionPriority.High));

                                            foreach (SettlementFC settlement in worldcomp.settlements)
                                            {
                                                if (settlement.isMilitaryValid() == true && settlement != homeSettlement)
                                                {
                                                    //if military is valid to use.
                                        
                                                    settlementList.Add(new FloatMenuOption(settlement.name + " " + "ShortMilitary".Translate() + " " + settlement.settlementMilitaryLevel + " - " + "FCAvailable".Translate() + ": " + (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                                    {
                                                        if(settlement.isMilitaryBusy() == true)
                                                        {
                                                            //military is busy
                                                        } else
                                                        {

                                                            MilitaryUtilFC.changeDefendingMilitaryForce(evt, settlement);

                                                        }
                                                    }, MenuOptionPriority.Default, null, null, 0f, null, null
                                                    ));
                                                }       


                                            }

                                            if (settlementList.Count == 0)
                                            {
                                                settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(), null));
                                            }

                                            FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                            floatMenu2.vanishIfMouseDistant = true;
                                            Find.WindowStack.Add(floatMenu2);


                                            //set to raid settlement here
                                        }));

                                        FloatMenu floatMenu = new FloatMenu(list);
                                        floatMenu.vanishIfMouseDistant = true;
                                        Find.WindowStack.Add(floatMenu);
                                    }



                                }});
                            }


                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), "Kill")]
        class MercenaryDied
        {
            static bool Prefix(Pawn __instance)
            {
                if ( Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.AllMercenaryPawns.Contains(__instance) == true)
                {
                    __instance.SetFaction(FactionColonies.getPlayerColonyFaction());
                    MercenarySquadFC squad = Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.returnSquadFromUnit(__instance);
                    if (squad != null)
                    {
                        Mercenary merc = Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.returnMercenaryFromUnit(__instance, squad);
                        if (merc != null)
                            squad.PassPawnToDeadMercenaries(merc);
                        squad.hasDead = true;
                        squad.removeDroppedEquipment();
                    } else
                    {
                        Log.Message("Mercenary Errored out. Did not find squad.");
                    }
                    __instance.equipment.DestroyAllEquipment();
                    __instance.apparel.DestroyAll();
                    //__instance.Destroy();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DeathActionWorker_Simple), "PawnDied")]
        class MercenaryAnimalDied
        {
            static bool Prefix(Corpse corpse)
            {
                if (Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.AllMercenaryPawns.Contains(corpse.InnerPawn) == true)
                {
                    //corpse.InnerPawn.SetFaction(FactionColonies.getPlayerColonyFaction());
                    corpse.Destroy();
                    return false;
                }
                return true;
            }
        }

       // [HarmonyPatch(typeof(JobGiver_AnimalFlee), "TryGiveJob")]
        class TryGiveJobFleeAnimal
        {
            static bool Prefix(Pawn pawn)
            {
                if (Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.AllMercenaryPawns.Contains(pawn))
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }

        //Goodwill by distance to settlement
        [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), "AppendProximityGoodwillOffsets")]
        class GoodwillPatch
        {
            static void Postfix(int tile, List<Pair<Settlement, int>> outOffsets, bool ignoreIfAlreadyMinGoodwill, bool ignorePermanentlyHostile)
            {
                Pair:
                foreach (Pair<Settlement, int> pair in outOffsets)
                {
                    if(pair.First.Faction.def.defName == "PColony")
                    {
                        outOffsets.Remove(pair);
                        goto Pair;
                    }
                }
            }
        }

        //CheckNaturalTendencyToReachGoodwillThreshold()
        [HarmonyPatch(typeof(Faction), "CheckNaturalTendencyToReachGoodwillThreshold")]
        class GoodwillPatchFunctionsGoodwillTendency
        {
            static bool Prefix(ref Faction __instance)
            {
                if (__instance.def.defName == "PColony")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        //tryAffectGoodwillWith
        [HarmonyPatch(typeof(Faction), "TryAffectGoodwillWith")]
        class GoodwillPatchFunctionsGoodwillAffect
        {
            static bool Prefix(ref Faction __instance, Faction other, int goodwillChange, bool canSendMessage = true, bool canSendHostilityLetter = true, string reason = null, GlobalTargetInfo? lookTarget = null)
            {
                if (__instance.def.defName == "PColony" && other == Find.FactionManager.OfPlayer)
                {
                    if (reason == "GoodwillChangedReason_RequestedTrader".Translate())
                    {
                        return false;
                    } else
                        if (reason == "GoodwillChangedReason_ReceivedGift".Translate())
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }


        //Notify_MemberDied(Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
        [HarmonyPatch(typeof(Faction), "Notify_MemberDied")]
        class GoodwillPatchFunctionsMemberDied
        {
            static bool Prefix(ref Faction __instance, Pawn member, DamageInfo? dinfo, bool wasWorldPawn, Map map)
            {
                if (member.Faction.def.defName == "PColony" && !wasWorldPawn && !PawnGenerator.IsBeingGenerated(member) && map != null && map.IsPlayerHome && !__instance.HostileTo(Faction.OfPlayer))
                {
                    FactionFC faction = Find.World.GetComponent<FactionFC>();
                    if (!faction.hasPolicy(FCPolicyDefOf.pacifist))
                    {
                        if (dinfo != null && (dinfo.Value.Category == DamageInfo.SourceCategory.Collapse))
                        {
                            Messages.Message("DeathOfFactionPawn".Translate(), MessageTypeDefOf.PawnDeath);
                            foreach (SettlementFC settlement in faction.settlements)
                            {
                                settlement.unrest += 5 * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", faction.traits, "multiply");
                                settlement.happiness -= 5 * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", faction.traits, "multiply");
                            }

                        }
                        else if (dinfo != null && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == null))
                        {
                            Pawn pawn = dinfo.Value.Instigator as Pawn;
                            if (pawn == null || !pawn.RaceProps.Animal || pawn.mindState.mentalStateHandler.CurStateDef != MentalStateDefOf.ManhunterPermanent)
                            {
                                Messages.Message("DeathOfFactionPawn".Translate(), MessageTypeDefOf.PawnDeath);
                                foreach (SettlementFC settlement in faction.settlements)
                                {
                                    settlement.unrest += 5 * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", faction.traits, "multiply");
                                    settlement.happiness -= 5 * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", faction.traits, "multiply");
                                }
                            }
                        }
                        else if (dinfo != null && dinfo.Value.Instigator != null && dinfo.Value.Instigator.Faction == Find.FactionManager.OfPlayer)
                        {
                            Messages.Message("DeathOfFactionPawn".Translate(), MessageTypeDefOf.PawnDeath);
                            foreach (SettlementFC settlement in faction.settlements)
                            {
                                settlement.unrest += 5 * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", faction.traits, "multiply");
                                settlement.happiness -= 5 * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", faction.traits, "multiply");
                            }
                        }
                    }

                    //return false to stop from continuing method
                    return false;
                }
               
                else
                {
                    return true;
                }
            }
        }


        //member exit map
        [HarmonyPatch(typeof(Faction), "Notify_MemberExitedMap")]
        class GoodwillPatchFunctionsExitedMap
        {
            static bool Prefix(ref Faction __instance, Pawn member, bool free)
            {
                if (__instance.def.defName == "PColony")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        

        //member took damage
        [HarmonyPatch(typeof(Faction), "Notify_MemberTookDamage")]
        class GoodwillPatchFunctionsTookDamage
        {
            static bool Prefix(ref Faction __instance, Pawn member, DamageInfo dinfo)
            {
                if (__instance.def.defName == "PColony")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        //Player traded
        [HarmonyPatch(typeof(Faction), "Notify_PlayerTraded")]
        class GoodwillPatchFunctionsPlayerTraded
        {
            static bool Prefix(ref Faction __instance, float marketValueSentByPlayer, Pawn playerNegotiator)
            {
                if (__instance.def.defName == "PColony")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        //Player traded
        [HarmonyPatch(typeof(Faction), "Notify_MemberCaptured")]
        class GoodwillPatchFunctionsCapturedPawn
        {
            static bool Prefix(ref Faction __instance, Pawn member, Faction violator)
            {
                if (__instance.def.defName == "PColony" && violator == Find.FactionManager.OfPlayer)
                {
                    Messages.Message("CaptureOfFactionPawn".Translate(), MessageTypeDefOf.NegativeEvent);
                    foreach (SettlementFC settlement in Find.World.GetComponent<FactionFC>().settlements)
                    {
                        settlement.unrest += 15 * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "unrestGainedMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply");
                        settlement.happiness -= 10 * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", settlement.traits, "multiply") * traitUtilsFC.cycleTraits(new double(), "happinessLostMultiplier", Find.World.GetComponent<FactionFC>().traits, "multiply");

                        settlement.unrest = Math.Min(settlement.unrest, 100);
                        settlement.happiness = Math.Max(settlement.happiness, 0);
                    }

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        // Fix a crash related to a harmony bug on Linux
        // This gets all patches Empire makes, gets the ones that would crash on Linux, and fixes them
        static void FixLinuxHarmonyCrash(Harmony harmony)
        {
            bool WouldCrash(MethodInfo method)
            {
                if (method == null || !method.IsVirtual || method.IsAbstract || method.IsFinal)
                {
                    return false;
                }

                byte[] bytes = method.GetMethodBody()?.GetILAsByteArray();
                if (bytes == null || bytes.Length == 0 || (bytes.Length == 1 && bytes.First() == 0x2A))
                {
                    return true;
                }

                return false;
            }

            var methods = typeof(FactionFC).Assembly.GetTypes()
                .Where(t => t.IsClass && !typeof(Delegate).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch)).Any())
                .SelectMany(t => {
                    HarmonyPatch patch = (HarmonyPatch)Attribute.GetCustomAttribute(t, typeof(HarmonyPatch));
                    MethodInfo[] m = patch.info.declaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    return m.Where(met => met.Name == patch.info.methodName);
                })
                .Where(WouldCrash)
                ;

            foreach (MethodInfo i in methods)
            {
                // Patching methods without any Prefixes/Postfixes before actually patching them fixes it. Idk why
                harmony.Patch(i);
            }
        }

        //CallForAid
        //Remove ability to attack colony.




        public FactionFC(World world) : base(world)
        {
            var harmony = new Harmony("com.Saakra.Empire");

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Linux)
            {
                FixLinuxHarmonyCrash(harmony);
            }

            harmony.PatchAll();

            if (FactionColonies.checkForMod("kentington.saveourship2"))
            {
                SoS2HarmonyPatches.Patch(harmony);
            }

            if (FactionColonies.checkForMod("Krkr.AndroidTiers") || FactionColonies.checkForMod("Atlas.AndroidTiers"))
            {
                //Android_Tiers_Patches.Patch(harmony);
            }





            power.isTithe = true;
            power.isTitheBool = true;
            research.isTithe = true;
            research.isTitheBool = true;
            

        }

        public List<SettlementFC> settlementsOnPlanet
        {
            get
            {
                List<SettlementFC> list = new List<SettlementFC>();
                foreach (SettlementFC settlement in settlements)
                {
                    if (settlement.planetName == Find.World.info.name)
                    {
                        list.Add(settlement);
                    }
                }
                return list;
            }
        }
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (firstTick == true)
            {
                //Log.Message("First Tick");
                FactionColonies.updateChanges();
                if (planetName == null || planetName == ""){
                    planetName = Find.World.info.name;
                }
                roadBuilder.updateRoadTech();
                roadBuilder.createRoadQueue(Find.World.info.name);
                roadBuilder.calculateRoadPathForWorld();
                
                firstTick = false;
                Faction FCf = FactionColonies.getPlayerColonyFaction();
                if (FCf != null)
                {
                    FCf.def.techLevel = TechLevel.Undefined;
                    factionIcon = texLoad.factionIcons.FirstOrFallback(obj => obj.name == factionIconPath, texLoad.factionIcons.First());
                    updateFactionIcon(ref FCf, "FactionIcons/" + factionIcon.name);
                    factionIconPath = factionIcon.name;
                }
                factionBackup = FCf;

            }
            FCEventMaker.ProcessEvents(in events);
            billUtility.processBills();

            TickMecernaries();

            

            

            //If Player Colony Faction does exists
            Faction faction = FactionColonies.getPlayerColonyFaction();
            if (faction != null)
            {
                roadBuilder.RoadTick();
                TaxTick();
                UITick();
                StatTick();
                MilitaryTick();
                TickActions();

            } else if (faction == null && settlements.Count() >= 0 && factionBackup != null)
            {
                //Log.Message("Moved to new planet - Adding faction copy");
                //FactionColonies.createPlayerColonyFaction();
                //FactionColonies.copyPlayerColonyFaction();
            }

            if (boolChangedPlanet)
            {
               // if (!factionUpdated)
               // {
                    SoS2HarmonyPatches.updateFactionOnPlanet();
                    factionUpdated = false;
               // }
            Reset:
                //Log.Message("New planet-" + Find.World.info.name);
                foreach (SettlementSoS2Info entry in createSettlementQueue)
                {
                    //Log.Message("key for create-" + entry.Key);
                    if (entry.planetName == Find.World.info.name)
                    {
                        //Log.Message("Match");


                        Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                        settlement.SetFaction(faction);
                        settlement.Tile = entry.location; 
                        settlement.Name = returnSettlementByLocation(settlement.Tile, Find.World.info.name).name;
                        Find.WorldObjects.Add(settlement);

                        createSettlementQueue.Remove(entry);
                        goto Reset;

                    }
                }
                roadBuilder.createRoadQueue(Find.World.info.name);
                roadBuilder.calculateRoadPathForWorld();
            Reset2:
                foreach (SettlementSoS2Info entry in deleteSettlementQueue)
                {
                    //Log.Message("key for destroy-" + entry.Key);
                    if (entry.planetName == Find.World.info.name)
                    {
                        //Log.Message("Match");
                        Find.WorldObjects.Remove(Find.World.worldObjects.SettlementAt(entry.location));
                        deleteSettlementQueue.Remove(entry);
                        goto Reset2;

                    }
                }
                boolChangedPlanet = false;
            }

        }

        public void TickActions()
        {
            int tick = Find.TickManager.TicksGame;
            foreach (SettlementFC settlement in settlements)
            {
                settlement.tickSpecialActions(tick);

            }
            //Feudal
            if (traitFeudalBoolCanUseMercenary == false && (traitFeudalTickLastUsedMercenary + GenDate.TicksPerSeason) <= Find.TickManager.TicksGame)
            {
                traitFeudalBoolCanUseMercenary = true;
                Find.LetterStack.ReceiveLetter("FCActionAvailable".Translate(), "FCActionMercenaryRefreshed".Translate(), LetterDefOf.PositiveEvent);
            }
            //Expansionist
            if (traitExpansionistBoolCanUseSettlementFeeReduction == false && (traitExpansionistTickLastUsedSettlementFeeReduction + GenDate.TicksPerYear) <= Find.TickManager.TicksGame)
            {
                traitExpansionistBoolCanUseSettlementFeeReduction = true;
                Find.LetterStack.ReceiveLetter("FCActionAvailable".Translate(), "FCActionSettlementFeeReduction".Translate(), LetterDefOf.PositiveEvent);
            }

            //Mercantile
            if (hasTrait(FCPolicyDefOf.mercantile) && traitMercantileTradeCaravanTickDue <= Find.TickManager.TicksGame)
            {
                IncidentWorker_TraderCaravanArrival worker = new IncidentWorker_TraderCaravanArrival();
                worker.def = IncidentDefOf.TraderCaravanArrival;
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, this.returnCapitalMap());
                parms.faction = FactionColonies.getPlayerColonyFaction();
                RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, (Map)parms.target, CellFinder.EdgeRoadChance_Friendly);
                parms.spawnRotation = Rot4.FromAngleFlat((((Map)parms.target).Center - parms.spawnCenter).AngleFlat);
                if (parms.spawnCenter.IsValid)
                    worker.TryExecute(parms);
                else
                    Log.Message("Empire - Mercantile - Spawn Center not valid");


                resetTraitMercantileCaravanTime();
            }
        }

        public void TickMecernaries()
        {
            mercenaryTick++;
            if (mercenaryTick > 120)
            {

                for (int i = 0; i < militaryCustomizationUtil.mercenarySquads.Count(); i++)
                //foreach (MercenarySquadFC squad in militaryCustomizationUtil.mercenarySquads)
                {
                    MercenarySquadFC squad = militaryCustomizationUtil.mercenarySquads[i];
                    if (squad.isDeployed)
                    {
                        if (squad.DeployedMercenaries.Count > 0 && squad.isDeployed == true && squad.hasLord == false)
                        {
                            Log.Message("Pawn deployed, creating lord");
                            Faction faction = FactionColonies.getPlayerColonyFaction();
                            List<Pawn> pawns = new List<Pawn>();
                            foreach (Mercenary pawn in squad.DeployedMercenaries)
                            {
                                pawn.pawn.mindState.canFleeIndividual = false;
                                pawns.Add(pawn.pawn);
                            }
                            foreach (Mercenary pawn in squad.DeployedMercenaryAnimals)
                            {
                                pawn.pawn.mindState.canFleeIndividual = false;
                                pawns.Add(pawn.pawn);
                            }
                            
                          

                            Lord lord = LordMaker.MakeNewLord(faction, new LordJob_AssistColony(faction, squad.DeployedMercenaries[0].pawn.DutyLocation()), squad.DeployedMercenaries[0].pawn.Map, pawns);
                            squad.map = squad.DeployedMercenaries[0].pawn.Map;
                            squad.lord = lord;


                            squad.hasLord = true;
                        }
                        if (Find.WindowStack.IsOpen(typeof(EmpireUIMercenaryCommandMenu)) == false)
                        {
                            //Log.Message("Opening Window");
                            // menu.focusWhenOpened = false;
                            
                            Find.WindowStack.Add(new EmpireUIMercenaryCommandMenu());
                           
                        }
                        MilitaryAI.SquadAI(ref squad);

                    
                        
                    } else
                    {

                        //If squads not deployed

                    }
                }

                mercenaryTick = 0;
            }if (militaryCustomizationUtil.fireSupport == null)
            {
                militaryCustomizationUtil.fireSupport = new List<MilitaryFireSupport>();
            }
            
            //Other functions
            ResetFireSupport:
            foreach (MilitaryFireSupport fireSupport in militaryCustomizationUtil.fireSupport)
            {
                if (fireSupport.ticksTillEnd <= Find.TickManager.TicksGame)
                {
                    militaryCustomizationUtil.fireSupport.Remove(fireSupport);
                    goto ResetFireSupport;

                } else
                {
                    //process firesupport
                    if (fireSupport.fireSupportType == "fireSupport")
                    {
                        if ((fireSupport.timeRunning % 15) == 0 && fireSupport.timeRunning >= fireSupport.startupTime) //15
                        {
                            //Log.Message("Boom");
                            IntVec3 spawnCenter = (from x in GenRadial.RadialCellsAround(fireSupport.location, fireSupport.accuracy, true)
                                              where x.InBounds(fireSupport.map)
                                              select x).RandomElementByWeight((IntVec3 x) => new SimpleCurve { { new CurvePoint(0f, 1f), true }, { new CurvePoint(fireSupport.accuracy, 0.1f), true } }.Evaluate(x.DistanceTo(fireSupport.location)));

                            Map map = fireSupport.map;
                            Thing launcher = null;
                            ProjectileHitFlags hitFlags = ProjectileHitFlags.All;
                            LocalTargetInfo info = new LocalTargetInfo(spawnCenter);
                            ThingDef def = new ThingDef();
                            ThingDef tempDef;
                            if (FactionColonies.checkForMod("CETeam.CombatExtended"))
                            {//if CE is on
                                tempDef = fireSupport.expendProjectile();
                                Type typeDef = FactionColonies.returnUnknownTypeFromName("CombatExtended.AmmoDef");
                                var ammoSetDef = typeDef.GetProperty("AmmoSetDefs", BindingFlags.Public | BindingFlags.Instance).GetValue(tempDef);
                                Type ammoLink = FactionColonies.returnUnknownTypeFromName("CombatExtended.AmmoLink");
                                var ammoLinkVar = ammoSetDef.GetType().GetProperty("Item").GetValue(ammoSetDef, new object[] { 0 });
                              //  Log.Message(ammoLinkVar.ToString());
                                var ammoTypes = ammoLinkVar.GetType().GetField("ammoTypes", BindingFlags.Public | BindingFlags.Instance).GetValue(ammoLinkVar);
                                //list of ammotypes
                                int count = (int)ammoTypes.GetType().GetProperty("Count").GetValue(ammoTypes, new object[] { });
                                for (int k = 0; k < count; k++)
                                {
                                    var ammoDefAmmo = ammoTypes.GetType().GetProperty("Item").GetValue(ammoTypes, new object[] { k });
                                    if (ammoDefAmmo.GetType().GetField("ammo", BindingFlags.Public | BindingFlags.Instance).GetValue(ammoDefAmmo).ToString() == tempDef.defName)
                                    {
                                        def = (ThingDef)ammoDefAmmo.GetType().GetField("projectile", BindingFlags.Public | BindingFlags.Instance).GetValue(ammoDefAmmo);
                                        break;
                                    } 
                                }



                                Type type2 = FactionColonies.returnUnknownTypeFromName("CombatExtended.ProjectileCE");
                                MethodInfo launch = type2.GetMethod("Launch", new Type[] { typeof(Thing), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(float), typeof(Thing) });
                                MethodInfo getShotAngle = type2.GetMethod("GetShotAngle", BindingFlags.Public | BindingFlags.Static);
                                Thing thing = GenSpawn.Spawn(def, fireSupport.sourceLocation, map, WipeMode.Vanish);

                                PropertyInfo gravityProperty = type2.GetProperty("GravityFactor", BindingFlags.NonPublic | BindingFlags.Instance);

                                Vector2 sourceVec = new Vector2(fireSupport.sourceLocation.x, fireSupport.sourceLocation.z);
                                Vector2 destVec = new Vector2(spawnCenter.x, spawnCenter.z);
                                Vector3 finalVector = (destVec - sourceVec);
                                float magnitude = finalVector.magnitude;

                                float gravity = (float)gravityProperty.GetValue(thing);      //1.96f * 5;

                                float shotRotation = (-90f + 57.29578f * Mathf.Atan2(finalVector.y, finalVector.x)) % 360;             //Vector2Utility.AngleTo(sourceVec, destVec);
                                float shotHeight = 10f;
                                float shotSpeed = 100f;
                                float shotAngle = (float)getShotAngle.Invoke(null, BindingFlags.Public | BindingFlags.Static, null, new object[] { shotSpeed, magnitude, shotHeight, true, gravity }, null);


                                launch.Invoke(thing, new object[] { (Thing)FactionColonies.getPlayerColonyFaction().leader, sourceVec, shotAngle, shotRotation, shotHeight, shotSpeed, null });


                            }
                            else
                            {
                                def = fireSupport.expendProjectile().projectileWhenLoaded;
                                Projectile projectile = (Projectile)GenSpawn.Spawn(def, fireSupport.sourceLocation, map, WipeMode.Vanish);
                                projectile.Launch(launcher, info, info, hitFlags);
                            }
                            

                            
                            
                        }
                    }


                }
               // Log.Message("tick - " + fireSupport.timeRunning);
                fireSupport.timeRunning++;
            }




        }

       


        public int GetNextSettlementFCID()
        {
            this.nextSettlementFCID++;
            //Log.Message("Returning next settlement FC ID " + nextSettlementFCID);

            return nextSettlementFCID;
        }

        public int GetNextMercenaryID()
        {
            this.nextMercenaryID++;
            //Log.Message("Returning next mercenary ID " + nextMercenaryID);
            return nextMercenaryID;
        }

        public int GetNextUnitID()
        {
            this.nextUnitID++;
            //Log.Message("Returning next Unit ID " + nextUnitID);

            return nextUnitID;
        }

        public int GetNextSquadID()
        {
            this.nextSquadID++;
            //Log.Message("Returning next SquadID " + nextSquadID);

            return nextSquadID;
        }

        public int GetNextMilitaryFireSupportID()
        {
            this.nextMilitaryFireSupportID++;
            //Log.Message("Returning next MilitaryFireSupportID " + nextSquadID);

            return nextMilitaryFireSupportID;
        }

        public int GetNextMercenarySquadID()
        {
            this.nextMercenarySquadID++;
            //Log.Message("Returning next MercenarySquadID " + nextMercenarySquadID);

            return nextMercenarySquadID;
        }

        public int GetNextTaxID()
        {
            this.nextTaxID++;
            return nextTaxID;
        }

        public int GetNextEventID()
        {
            this.nextEventID++;
            return nextEventID;
        }

        public int GetNextBillID()
        {
            this.nextBillID++;
            return nextBillID;
        }

        public int GetNextPrisonerID()
        {
            this.nextPrisonerID++;
            return nextPrisonerID;
        }

        public List<FCTraitEffectDef> returnListFactionTraits()
        {
            List<FCTraitEffectDef> tmpList = new List<FCTraitEffectDef>();
            foreach (FCTraitEffectDef trait in traits)
            {
                tmpList.Add(trait);
            }
            return tmpList;
        }


        public void setStartTime()
        {
            taxTimeDue = Find.TickManager.TicksGame + LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().timeBetweenTaxes;
            dailyTimer = Find.TickManager.TicksGame + 2000;
        }

        //0 defname
        //1 desc
        //2 location
        //3 time till trigger

        public int returnHighestMilitaryLevel()
        {
            int max = 1;
            foreach (SettlementFC settlement in settlements)
            {
                max = Math.Max(max, settlement.settlementMilitaryLevel);
            }
            return max;
        }

       

        public void updateFaction()
        {
            if (Find.World.GetComponent<FactionFC>().factionDef != null)
            {

            }
            else
            {
                Find.World.GetComponent<FactionFC>().factionDef = new FactionFCDef();
            }

            //load factionfcvalues
            //FactionColonies.getPlayerColonyFaction().def.techLevel = factionDef.techLevel;
            //FactionColonies.getPlayerColonyFaction().def.apparelStuffFilter = factionDef.apparelStuffFilter;
        }

        public void updateFactionRaces()
        {
            Faction faction = FactionColonies.getPlayerColonyFaction();

            
        }

        public string returnNextTechToLevel()
        {
            switch (techLevel)
            {
                case TechLevel.Ultra:
                    return "ReachedMaxLevel".Translate();
                case TechLevel.Spacer:
                    return "FCShipBasics".Translate();
                case TechLevel.Industrial:
                    return "FCFabrication".Translate();
                case TechLevel.Medieval:
                    return "FCElectricity".Translate();
                case TechLevel.Neolithic:
                    return "FCSmithing".Translate();
                default:
                    return "N/A";
            }
        }
        public void updateTechLevel(ResearchManager researchManager)
        {
            bool medievalOnly = LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().medievalTechOnly;


            if (!medievalOnly && DefDatabase<ResearchProjectDef>.GetNamed("ShipBasics", false) != null && researchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("ShipBasics", false)) == DefDatabase<ResearchProjectDef>.GetNamed("ShipBasics", false).baseCost && techLevel < TechLevel.Ultra){
                techLevel = TechLevel.Ultra;
                factionDef.techLevel = TechLevel.Ultra;
                Log.Message("Ultra");
            }
            else if (!medievalOnly && DefDatabase<ResearchProjectDef>.GetNamed("Fabrication", false) != null && researchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("Fabrication", false)) == DefDatabase<ResearchProjectDef>.GetNamed("Fabrication", false).baseCost && techLevel < TechLevel.Spacer)
            {
                techLevel = TechLevel.Spacer;
                factionDef.techLevel = TechLevel.Spacer;
                Log.Message("Spacer");
            }
            else if (!medievalOnly && DefDatabase<ResearchProjectDef>.GetNamed("Electricity", false) != null && researchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("Electricity", false)) == DefDatabase<ResearchProjectDef>.GetNamed("Electricity", false).baseCost && techLevel < TechLevel.Industrial)
            {
                techLevel = TechLevel.Industrial;
                factionDef.techLevel = TechLevel.Industrial;
                Log.Message("Industrial");
            }
            else if (DefDatabase<ResearchProjectDef>.GetNamed("Smithing", false) != null && researchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("Smithing", false)) == DefDatabase<ResearchProjectDef>.GetNamed("Smithing", false).baseCost && techLevel < TechLevel.Medieval)
            {
                techLevel = TechLevel.Medieval;
                factionDef.techLevel = TechLevel.Medieval;
                //Log.Message("Medieval");
                Log.Message("Medieval");
            }
            else
            {
                //Log.Message("Neolithic");
                if (techLevel < TechLevel.Neolithic)
                {
                    Log.Message("Neolithic");
                    techLevel = TechLevel.Neolithic;
                }
            }
            //update to player colony faction
            updateFaction();

            Faction playerColonyfaction = FactionColonies.getPlayerColonyFaction();
            if (playerColonyfaction != null && playerColonyfaction.def.techLevel < techLevel)
            {
                Log.Message("Updating Tech Level");
                if (playerColonyfaction != null)
                {
                    updateFactionDef(techLevel, ref playerColonyfaction);
                }
            }
        }

        public void updateFactionIcon(ref Faction faction, string iconPath)
        {
            Log.Message("Updated Icon - " + iconPath);
            faction.def.factionIconPath = iconPath;
        }

        public void updateFactionDef(TechLevel tech, ref Faction faction)
        {
            FactionDef replacingDef;
            ThingFilter apparelStuffFilter = new ThingFilter();
            FactionDef def = faction.def;

            switch (tech)
            {
                case TechLevel.Archotech:
                case TechLevel.Ultra:
                case TechLevel.Spacer:
                    replacingDef = DefDatabase<FactionDef>.GetNamedSilentFail("OutlanderCivil");
      
                    break;
                case TechLevel.Industrial:
                    replacingDef = DefDatabase<FactionDef>.GetNamedSilentFail("OutlanderCivil");
                    break;
                case TechLevel.Medieval:
                    if (FactionColonies.checkForMod("OskarPotocki.VanillaFactionsExpanded.MedievalModule"))
                    {
                        replacingDef = DefDatabase<FactionDef>.GetNamedSilentFail("VFEM_KingdomCivil");
                    } else
                    {
                        replacingDef = DefDatabase<FactionDef>.GetNamedSilentFail("TribeCivil");
                    }
                    break;
                case TechLevel.Neolithic:
                case TechLevel.Animal:
                case TechLevel.Undefined:
                default:
                    replacingDef = DefDatabase<FactionDef>.GetNamedSilentFail("TribeCivil");
                    break;

            }
            //Log.Message("FactionFC.updateFactionDef - switch(tech) passed");

            //Log.Message("1");
            def.pawnGroupMakers = replacingDef.pawnGroupMakers;
            //Log.Message("2");
            def.caravanTraderKinds = replacingDef.caravanTraderKinds;
            //Log.Message("3");
            if (replacingDef.backstoryFilters != null && replacingDef.backstoryFilters.Count != 0)
                def.backstoryFilters = replacingDef.backstoryFilters;
            //Log.Message("4");
            def.techLevel = tech;
            //Log.Message("5");
            def.hairTags = replacingDef.hairTags;
            //Log.Message("6");
            def.visitorTraderKinds = replacingDef.visitorTraderKinds;
            //Log.Message("7");
            def.baseTraderKinds = replacingDef.baseTraderKinds;
            //Log.Message("8");
            if (replacingDef.apparelStuffFilter != null)
            def.apparelStuffFilter = replacingDef.apparelStuffFilter;


            if (tech >= TechLevel.Spacer && def.apparelStuffFilter != null)
            {
                def.apparelStuffFilter.SetAllow(DefDatabase<StuffCategoryDef>.GetNamedSilentFail("Synthread"), true);
                def.apparelStuffFilter.SetAllow(DefDatabase<StuffCategoryDef>.GetNamedSilentFail("Hyperweave"), true);
                def.apparelStuffFilter.SetAllow(DefDatabase<StuffCategoryDef>.GetNamedSilentFail("Plasteel"), true);
            }

            updateFactionIcon(ref faction, "FactionIcons/" + factionIconPath);

            Log.Message("FactionFC.updateFactionDef - Completed tech update");

        }

        public bool hasPolicy(FCPolicyDef def)
        {
            //Don't game the system
            if (policies.Count() < 2)
            {
                return false;
            }

            foreach (FCPolicy policy in this.policies)
            {
                if (policy.def == def)
                    return true;
            }
            return false;
        }

        public bool hasTrait(FCPolicyDef def)
        {
            foreach (FCPolicy trait in this.factionTraits)
            {
                if (trait.def == def)
                    return true;
            }
            return false;
        }

        public bool sendDiplomaticEnvoy(Faction faction)
        {
            FactionFC factionfc = Find.World.GetComponent<FactionFC>();

            if (!faction.def.permanentEnemy)
            {
                if (Find.TickManager.TicksGame >= (factionfc.traitPacifistTickLastUsedDiplomat + GenDate.TicksPerDay * 5))
                {
                    factionfc.traitPacifistTickLastUsedDiplomat = Find.TickManager.TicksGame;
                    int random = Rand.Range(1, 10);
                    if (random > 5)
                    {
                        int relationImprovement = Rand.Range(5, 15);
                        faction.TryAffectGoodwillWith(Find.FactionManager.OfPlayer, relationImprovement);
                        Find.LetterStack.ReceiveLetter("FCRelationImproved".Translate(), TranslatorFormattedStringExtensions.Translate("FCRelationImprovedText", faction.Name, relationImprovement), LetterDefOf.PositiveEvent);
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("FCRelationNotImproved".Translate(), TranslatorFormattedStringExtensions.Translate("FCFailedToImproveRelationship", faction.Name), LetterDefOf.NeutralEvent);
                    }
                    return true;
                }
                else
                {
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("XDaysToSendDiplomat", Math.Round(GenDate.TicksToDays((factionfc.traitPacifistTickLastUsedDiplomat + GenDate.TicksPerDay * 5) - Find.TickManager.TicksGame), 1)), MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            else
            {

                Messages.Message("FCCannotImproveRelationsWithType".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
        }

        public void resetRaceFilter()
        {
            raceFilter = new ThingFilter();

            List<string> races = new List<string>();
            foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (def.race.race.intelligence == Intelligence.Humanlike & races.Contains(def.race.label) == false && def.race.BaseMarketValue != 0)
                {
                    if (def.race.label == "Human" && def.LabelCap != "Colonist")
                    {

                    }
                    else
                    {

                        races.Add(def.race.label);
                        raceFilter.SetAllow(def.race, true);

                    }
                }
            }
        }

        public void updateAverages()
        {
            int averageHappinessTmp = 0;
            int averageLoyaltyTmp = 0;
            int averageUnrestTmp = 0;
            int averageProsperityTmp = 0;
            
            if (settlements.Count() > 0)
            {

                foreach (SettlementFC settlement in settlements)
                {

                    averageHappinessTmp += Convert.ToInt32(settlement.happiness);
                    averageLoyaltyTmp += Convert.ToInt32(settlement.loyalty);
                    averageUnrestTmp += Convert.ToInt32(settlement.unrest);
                    averageProsperityTmp += Convert.ToInt32(settlement.prosperity);
                }
                averageHappinessTmp /= settlements.Count();
                averageLoyaltyTmp /= settlements.Count();
                averageUnrestTmp /= settlements.Count();
                averageProsperityTmp /= settlements.Count();

            }

            averageHappiness = averageHappinessTmp;
            averageLoyalty = averageLoyaltyTmp;
            averageUnrest = averageUnrestTmp;
            averageProsperity = averageProsperityTmp;

            
            if (settlements.Count() > 0 && FactionColonies.getPlayerColonyFaction() != null)
            {
                FactionColonies.getPlayerColonyFaction().TryAffectGoodwillWith(Find.FactionManager.OfPlayer, (Convert.ToInt32(averageHappiness) - FactionColonies.getPlayerColonyFaction().PlayerGoodwill));
            }
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public void addSettlement(SettlementFC settlement)
        {
            settlements.Add(settlement);
            uiUpdate();
        }

        public void uiUpdate()
        {
            //Pop UI updates
            updateTotalResources();
            updateTotalProfit();
            updateTechLevel(Find.ResearchManager);
        }

        public double getTotalIncome() //return total income of settlements       ####MAKE UPDATE PER HOUR TICK
        {
            double income = 0;
            for (int i = 0; i < settlements.Count(); i++)
            {
                income += settlements[i].getTotalIncome();
            }
            return income;
        }


        public double getTotalUpkeep() //returns total upkeep of all settlements
        {
            double upkeep = 0;
            for (int i = 0; i < settlements.Count(); i++)
            {
                upkeep += settlements[i].getTotalUpkeep();
            }
            return upkeep;
        }

        public double getTotalProfit() //returns total profit (income - upkeep) of all settlements
        {
            return (getTotalIncome() - getTotalUpkeep());
        }

        public void updateTotalProfit()
        {
            income = getTotalIncome();
            upkeep = getTotalUpkeep();
            profit = income - upkeep;
        }

        public void updateTotalResources()
        {
            for (int i = 0; i < returnNumberResource(); i++)
            {
                int resource = 0;

                for (int k = 0; k < settlements.Count(); k++)
                {
                    resource += (int)settlements[k].returnResourceByInt(i).endProduction;
                }
                returnResourceByInt(i).amount = resource;
                //Log.Message(i + " " + returnResourceByInt(i).amount);  //display total resources by type
            }
        }

        public void updateDailyResearch()
        {
            //Research adding
            if (Find.ResearchManager.currentProj == null && researchPointPool != 0)
            {
                Messages.Message(TranslatorFormattedStringExtensions.Translate("NoResearchExpended", Math.Round(researchPointPool)), MessageTypeDefOf.NeutralEvent);
            }
            else if (researchPointPool != 0 && Find.ResearchManager.currentProj != null)
            {
                //Log.Message(researchTotal.ToString());
                float neededPoints;
                neededPoints = (float)Math.Ceiling(Find.ResearchManager.currentProj.CostApparent - Find.ResearchManager.currentProj.ProgressApparent);
                Log.Message("Needed points: " + neededPoints.ToString());

                float expendedPoints;
                if (researchPointPool >= neededPoints)
                {
                    researchPointPool -= neededPoints;
                    expendedPoints = neededPoints;
                }
                else
                {
                    expendedPoints = researchPointPool;
                    researchPointPool = 0;
                }
                Log.Message("Expended points: " + expendedPoints.ToString());

                Find.LetterStack.ReceiveLetter("ResearchPointsExpended".Translate(), TranslatorFormattedStringExtensions.Translate("ResearchExpended", Math.Round(expendedPoints), Find.ResearchManager.currentProj.LabelCap, Math.Round(researchPointPool)), LetterDefOf.PositiveEvent);
                if (Find.ColonistBar.GetColonistsInOrder().Count > 0)
                {
                    Pawn pawn = Find.ColonistBar.GetColonistsInOrder()[0];
                    Find.ResearchManager.ResearchPerformed((float)Math.Ceiling(((1 * Find.ResearchManager.currentProj.CostFactor(pawn.Faction.def.techLevel)) / (0.00825 * Find.Storyteller.difficultyValues.researchSpeedFactor)) * expendedPoints), pawn);
                    Log.Message("Passed to function: " + (float)Math.Ceiling(((1 * Find.ResearchManager.currentProj.CostFactor(pawn.Faction.def.techLevel)) / (0.00825 * Find.Storyteller.difficultyValues.researchSpeedFactor)) * expendedPoints));
                } else
                {
                    Log.Message("Could not find colonist to research with");
                    Find.ResearchManager.ResearchPerformed((float)Math.Ceiling((1 / (0.00825 * Find.Storyteller.difficultyValues.researchSpeedFactor)) * expendedPoints), null);
                }

            }
        }



        public void addTax(bool isUpdating)
        {
            //if (capitalLocation == -1)
            //{
            //    setCapital();
            //}
            powerPool = 0;

            if (settlements.Count != 0) //if settlements is not zero
            {
               
                foreach (SettlementFC settlement in settlements)
                {
                    //Start Traits
                    addExperienceToFactionLevel(2f);


                    float trait_Industrious_TaxPercentageBoost = 1;
                    if (hasTrait(FCPolicyDefOf.industrious))
                    {
                        int num = Rand.RangeInclusive(1, 20);
                        if (num == 5)
                        {
                            trait_Industrious_TaxPercentageBoost = 1f + ((float)(Rand.RangeInclusive(20, 50)) / 100f);
                            Find.LetterStack.ReceiveLetter("FCIdustriousTaxBoost".Translate(), TranslatorFormattedStringExtensions.Translate("FCIndustriousPop", settlement.name, ((trait_Industrious_TaxPercentageBoost - 1f) * 100f) + "%"), LetterDefOf.PositiveEvent);
                        }

                    }




                    //End Traits

                    List<Thing> list = new List<Thing>();
                    settlement.updateProfitAndProduction();
                    list = settlement.createTithe(trait_Industrious_TaxPercentageBoost);
                    float researchPool = settlement.createResearchPool();
                    float electricityAllotted = settlement.createPowerPool();

                    BillFC bill = new BillFC(settlement);  //Create new bill connected to settlement
                    bill.taxes.electricityAllotted = electricityAllotted;
                    bill.taxes.researchCompleted = researchPool;
                    bill.taxes.itemTithes.AddRange(list); //Add tithe to bill's tithes
                    bill.taxes.silverAmount = (float)(Convert.ToInt32((settlement.totalIncome * trait_Industrious_TaxPercentageBoost) - settlement.totalUpkeep)) + settlement.returnSilverIncome(true);
                    Bills.Add(bill);

                    FactionColonies.getTownTitle(settlement);
                    TaxTickPrisoner(settlement);
                }


                if (!isUpdating) //if done updating (timeskip) then send goods/silver etc
                {
                    //Messages.Message("TaxesBilled".Translate() + "!", MessageTypeDefOf.PositiveEvent);
                    Find.LetterStack.ReceiveLetter("Taxes Billed", "Taxes from your settlements have been billed", LetterDefOf.PositiveEvent);
                    uiUpdate();
                    
                    //Messages.Message(Find.TickManager.TicksGame.ToString(), MessageTypeDefOf.PositiveEvent);
                }

            }
            else
            {

                Messages.Message("NoSettlementsToTax".Translate(), MessageTypeDefOf.NeutralEvent);
            }


        }
        public float updateFactionLevelGoalXP(int currentLevel)
        {
            float newGoal = 100 + (currentLevel * 150);
            return newGoal;
        }

        public bool addExperienceToFactionLevel(float xp)
        {
            bool leveled = false;
            this.factionXPCurrent += xp;

            while (factionXPCurrent >= factionXPGoal)
            {
                factionXPCurrent -= factionXPGoal;
                factionLevel += 1;
                Find.LetterStack.ReceiveLetter("FCFactionLevelUp".Translate(), TranslatorFormattedStringExtensions.Translate("FCFactionLevelUpDesc", this.name, this.factionLevel), LetterDefOf.PositiveEvent);
                leveled = true;
                factionXPGoal = updateFactionLevelGoalXP(factionLevel);
            }

            return leveled;
        }

        public void addEvent(FCEvent fcevent)
        {   //Add event to events
            events.Add(fcevent);

            //check if event has a location, if does, add traits to that specific location;
            if (fcevent.settlementTraitLocations.Count() > 0) //if has specific locations
            {
                foreach(SettlementFC location in fcevent.settlementTraitLocations)
                {
                    location.traits.AddRange(fcevent.def.traits);
                    foreach( FCTraitEffectDef trait in fcevent.def.traits)
                    {
                        //Log.Message(trait.label);
                    }
                    
                }
            } else 
            { //if no specific location then faction wide
                traits.AddRange(fcevent.traits);
            }
        }

        public bool checkSettlementCaravansList(string location) //list of destinations caravans gone to
        {
            for (int i = 0; i < settlementCaravansList.Count(); i++)
            {
                if(location == settlementCaravansList[i] || Find.WorldGrid.IsNeighbor(Convert.ToInt32(location), Convert.ToInt32(settlementCaravansList[i])))
                {
                    return true; // is on list
                }
            }
            return false; //is not on list
        }

        public ResourceFC returnResource(string name) //used to return the correct resource based on string name
        {
            if (name == "food")
            {
                return food;
            }
            if (name == "weapons")
            {
                return weapons;
            }
            if (name == "apparel")
            {
                return apparel;
            }
            if (name == "animals")
            {
                return animals;
            }
            if (name == "logging")
            {
                return logging;
            }
            if (name == "mining")
            {
                return mining;
            }
            if (name == "research")
            {
                return research;
            }
            if (name == "power")
            {
                return power;
            }
            if (name == "medicine")
            {
                return medicine;
            }
            Log.Message("Unable to find resource - returnResource(string name)");
            return null;
        }
        public ResourceFC returnResourceByInt(int name) //used to return the correct resource based on string name
        {
            if (name == 0)
            {
                return food;
            }
            if (name == 1)
            {
                return weapons;
            }
            if (name == 2)
            {
                return apparel;
            }
            if (name == 3)
            {
                return animals;
            }
            if (name == 4)
            {
                return logging;
            }
            if (name == 5)
            {
                return mining;
            }
            if (name == 6)
            {
                return research;
            }
            if (name == 7)
            {
                return power;
            }
            if (name == 8)
            {
                return medicine;
            }
            Log.Message("Unable to find resource - returnResourceByInt(int name)");
            return null;
        }


        public int returnNumberResource()
        {
            return 9;
        }

        public void setCapital(){
            if (Find.CurrentMap != null && Find.CurrentMap.IsPlayerHome == true)
			{
                capitalLocation = Find.CurrentMap.Parent.Tile;
                capitalPlanet = Find.World.info.name;

                //Log.Message(Find.CurrentMap.Parent.def.defName);
                if (Find.CurrentMap.Parent.def.defName == "ShipOrbiting")
                {
                    SoSShipCapital = true;
                } else
                {
                    SoSShipCapital = false;
                }

                Messages.Message(TranslatorFormattedStringExtensions.Translate("SetAsFactionCapital", Find.CurrentMap.Parent.LabelCap), MessageTypeDefOf.NeutralEvent);
            } else
            {
                Messages.Message("Unable to set faction capital on this map. Please go to your capital map and use the Set Capital button or else you may have some bugs soon.", MessageTypeDefOf.NegativeEvent);
            }
        }

        public int returnCapitalMapId()
        {
            for (int i = 0; i < Find.Maps.Count(); i++)
            {
                if (Find.Maps[i].Tile == capitalLocation)
                {
                    return i;
                }
            }
            Log.Message("CouldNotFindMapOfCapital".Translate());
            return -1;
        }

        public Map returnCapitalMap()
        {
            for (int i = 0; i < Find.Maps.Count(); i++)
            {
                if (Find.Maps[i].Tile == capitalLocation)
                {
                    return Find.Maps[i];
                }
            }
            Log.Message("CouldNotFindMapOfCapital".Translate());
            return null;
        }

        public int returnSettlementFCIDByLocation(int location, string planetName)
        {
            for (int i = 0; i < settlements.Count(); i++)
            {
                if(settlements[i].mapLocation == location && settlements[i].planetName == planetName)
                {
                    return i;
                }
            }
            return -1;
        }

        public SettlementFC returnSettlementByLocation(int location, string planetName)
        {
            if (planetName == null)
            {
                Log.Message("Planet name was null. Please report this as well as the military event that the settlement was used for.");
                planetName = Find.World.info.name;
            }
            for (int i = 0; i < settlements.Count(); i++)
            {
                //Log.Message(settlements[i].planetName);
                if (settlements[i].mapLocation == location && settlements[i].planetName == planetName)
                {
                    return settlements[i];
                }
            }
            return null;
        }

        public string getSettlementName(int location, string planetName)
        {
            int i = returnSettlementFCIDByLocation(location, planetName);
            switch (i)
            {
                case -1:
                    return "Null";

                default:
                    return settlements[returnSettlementFCIDByLocation(location, planetName)].name;
            }
        }


        public void updateSettlementStats()
        {
            foreach (SettlementFC settlement in settlements)
            {
                settlement.updateHappiness();
                settlement.updateLoyalty();
                settlement.updateUnrest();
                settlement.updateProsperity();
                
            }
        }

        public void TaxTick()
        {
            if (Find.TickManager.TicksGame >= taxTimeDue) // taxTimeDue being used as set interval when skipping time
            {
                while (Find.TickManager.TicksGame >= taxTimeDue)   //while updating events
                {//update events in this order: regular events: tax events.


                    if (Find.TickManager.TicksGame > taxTimeDue)
                    {

                        addTax(true);
                    }
                    else
                    {
                        Log.Message("Empire Mod - TaxTick - Catching Up - Did you skip time? Report this if you did not");
                        addTax(false);
                        //NOT WHERE FINAL UPDATE IS. Go to addTax Function

                    }
                    taxTimeDue += LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().timeBetweenTaxes;
                    //Log.Message(Find.TickManager.TicksGame + " vs " + taxTimeDue + " - Taxing");
                }

                //if Autoresolve bills on, attempt to autoresolve
                switch (autoResolveBills)
                {
                    case true:
                        PaymentUtil.autoresolveBills(Bills);
                        break;
                    case false:
                        break;
                }
            }
        }

        public void TaxTickPrisoner(SettlementFC settlement)
        {
            Reset:
            foreach (FCPrisoner prisoner in settlement.prisonerList)
            {
                switch (prisoner.workload)
                {
                    case FCWorkLoad.Heavy:
                        if (prisoner.AdjustHealth(-20))
                            goto Reset;
                        break;
                    case FCWorkLoad.Medium:
                        if(prisoner.AdjustHealth(-10))
                            goto Reset;
                        break;
                    case FCWorkLoad.Light:
                        if(prisoner.AdjustHealth(4))
                            goto Reset;
                        break;
                }
            }
        }

        public void resetTraitMercantileCaravanTime()
        {
            float days = Rand.RangeInclusive(3, 5);
            this.traitMercantileTradeCaravanTickDue = Find.TickManager.TicksGame + (int)(days * GenDate.TicksPerDay);
        }


        public void StatTick()
        {
            if (Find.TickManager.TicksGame >= dailyTimer) // taxTimeDue being used as set interval when skipping time
            {
                while (Find.TickManager.TicksGame >= dailyTimer)   //while updating events
                {//update events in this order: regular events: tax events.

                   // Log.Message("Tick");
                    updateSettlementStats();
                    updateAverages();
                    RelationsUtilFC.resetPlayerColonyRelations();


                    updateDailyResearch();


                    //Random event creation
                    int tmpNum = Rand.Range(1, 100);
                    //Log.Message(tmpNum.ToString());
                    if (tmpNum <= FactionColonies.randomEventChance && FactionColonies.Settings().disableRandomEvents == false)
                    {
                        FCEvent tmpEvt = FCEventMaker.MakeRandomEvent(FCEventMaker.returnRandomEvent(), null);
                        //Log.Message(tmpEvt.def.label);
                        if (tmpEvt != null)
                        {
                            Find.World.GetComponent<FactionFC>().addEvent(tmpEvt);


                            //letter code
                            string settlementString = "";
                            foreach (SettlementFC loc in tmpEvt.settlementTraitLocations)
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
                                Find.LetterStack.ReceiveLetter("Random Event", tmpEvt.def.desc + "\n This event is affecting the following settlements: " + settlementString, LetterDefOf.NeutralEvent);
                            }
                            else
                            {
                                Find.LetterStack.ReceiveLetter("Random Event", tmpEvt.def.desc, LetterDefOf.NeutralEvent);
                            }
                        }
                    }


                    dailyTimer += GenDate.TicksPerDay;
                    //Log.Message(Find.TickManager.TicksGame + " vs " + taxTimeDue + " - Taxing");
                }
            }
        }

        public void MilitaryTick()
        {
            if (Find.TickManager.TicksGame >= militaryTimeDue)
            {
                if (LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().disableHostileMilitaryActions == false & Find.TickManager.TicksGame > (timeStart + GenDate.TicksPerSeason))
                {
                    //if military actions not disabled or game has not passed through the first season
                    //Log.Message("Mil Action debug");




                //if settlements exist

                // get list of settlements

                //if not underattack, add to list

                //create weight list by settlement military level

                //choose random

                    if (settlements.Count() > 0)
                    {
                        //if settlements exist
                        List<SettlementFC> targets = new List<SettlementFC>();
                        foreach (SettlementFC settlement in settlements)
                        {
                            //create weight list of settlements
                            if (settlement.isUnderAttack == false)
                            {
                                //if not underattack, add to list
                                //get weightvalue of target
                                int weightValue;
                                switch (settlement.settlementMilitaryLevel)
                                {
                                    case 0:
                                    case 1:
                                        weightValue = 10;
                                        break;
                                    case 2:
                                    case 3:
                                        weightValue = 7;
                                        break;
                                    case 4:
                                    case 5:
                                        weightValue = 3;
                                        break;
                                    default:
                                        weightValue = 1;
                                        break;
                                }

                                for (int k = 0; k < weightValue; k++)
                                {
                                    targets.Add(settlement);
                                }

                            }
                        }

                        //List created, pick from list
                        Faction enemy = Find.FactionManager.RandomEnemyFaction();
                        if (enemy != null)
                            MilitaryUtilFC.attackPlayerSettlement(militaryForce.createMilitaryForceFromFaction(enemy, true), targets.RandomElement(), enemy);
                    }




                } else
                {
                    //Log.Message("Empire - Debug - Military Actions Disabled. Not processing.");
                }
                militaryTimeDue = Find.TickManager.TicksGame + (60000 * LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().minMaxDaysTillMilitaryAction.RandomInRange);
                //Log.Message(militaryTimeDue + " - " + Find.TickManager.TicksGame);
//Log.Message((militaryTimeDue - Find.TickManager.TicksGame) / 60000 + " days till next military action");
                //militaryTimeDue =
            }
        }


        public void UITick()
        {
            if (uiTimeUpdate <= 0) //update per time?
            {
                uiTimeUpdate = FactionColonies.updateUiTimer;

                //already built in ui update -.-
                Find.WindowStack.WindowsUpdate();

                //Pop UI updates
                uiUpdate();

            }
            else
            {
                uiTimeUpdate -= 1;
            }
        }



    }
}