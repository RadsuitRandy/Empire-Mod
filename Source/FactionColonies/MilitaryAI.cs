using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using Verse.AI;
using Verse.AI.Group;
using System.IO;

namespace FactionColonies
{
    public static class MilitaryAI
    {

        public static void SquadAI(ref MercenarySquadFC squad)
        {
            Faction playerFaction = Find.FactionManager.OfPlayer;
            Faction settlementFaction = FactionColonies.getPlayerColonyFaction();

            FactionFC factionfc = Find.World.GetComponent<FactionFC>();
            MilitaryCustomizationUtil militaryCustomizationUtil = factionfc.militaryCustomizationUtil;
            bool deployed = false;


            //Log.Message("1");
            
            foreach (Mercenary merc in squad.DeployedMercenaries)
            { 

                //If pawn is deployed
                    deployed = true;
                    if (!(squad.hitMap))
                    {
                        squad.hitMap = true;
                    }

                    if (merc.pawn.health.State == PawnHealthState.Mobile)
                    {

                        //if pawn is up and moving
                        if (squad.order != null && squad.order != MilitaryOrders.Leave)
                        {
                            if (squad.timeDeployed + 30000 >= Find.TickManager.TicksGame && merc.pawn.Faction != playerFaction)
                                merc.pawn.SetFaction(playerFaction);
                        } else
                        {
                            if (merc.pawn.drafter != null)
                                merc.pawn.drafter.Drafted = false;
                            if (merc.pawn.Faction != settlementFaction)
                                merc.pawn.SetFaction(settlementFaction);
                        }


                        JobGiver_AIFightEnemies jobGiver = new JobGiver_AIFightEnemies();
                        ThinkResult result = jobGiver.TryIssueJobPackage(merc.pawn, new JobIssueParams());
                        bool isValid = result.IsValid;


                        if (isValid)
                        {
                            //Log.Message("Success");
                            if (merc.pawn.jobs.curJob == null || ((merc.pawn.jobs.curJob.def == JobDefOf.Goto || merc.pawn.jobs.curJob.def != result.Job.def) && merc.pawn.jobs.curJob.def.defName != "ReloadWeapon" && merc.pawn.jobs.curJob.def.defName != "ReloadTurret" && !merc.pawn.Drafted))
                            {
                                merc.pawn.jobs.StartJob(result.Job, JobCondition.Ongoing);
                                //Log.Message(result.Job.ToString());
                            }
                        }
                        else
                        {
                            //Log.Message("Fail");
                            if (squad.timeDeployed + 30000 >= Find.TickManager.TicksGame)
                            {
                                if (merc.pawn.drafter == null || merc.pawn.Drafted == false)
                                {
                                    if (squad.order == MilitaryOrders.Standby)
                                    {
                                        //Log.Message("Standby");
                                        merc.pawn.mindState.forcedGotoPosition = squad.orderLocation;
                                        JobGiver_ForcedGoto jobGiver_Standby = new JobGiver_ForcedGoto();
                                        ThinkResult resultStandby = jobGiver_Standby.TryIssueJobPackage(merc.pawn, new JobIssueParams());
                                        bool isValidStandby = resultStandby.IsValid;
                                        if (isValidStandby)
                                        {
                                            //Log.Message("valid");
                                            merc.pawn.jobs.StartJob(resultStandby.Job, JobCondition.InterruptForced);


                                        }
                                    }
                                    else
                                    if (squad.order == MilitaryOrders.Attack)
                                    {
                                        //Log.Message("Attack");
                                        //If time is up, leave, else go home
                                        JobGiver_AIGotoNearestHostile jobGiver_Move = new JobGiver_AIGotoNearestHostile();
                                        ThinkResult resultMove = jobGiver_Move.TryIssueJobPackage(merc.pawn, new JobIssueParams());
                                        bool isValidMove = resultMove.IsValid;
                                        //Log.Message(resultMove.ToString());
                                        if (isValidMove)
                                        {
                                            merc.pawn.jobs.StartJob(resultMove.Job, JobCondition.InterruptForced);
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    if (squad.order == MilitaryOrders.Leave)
                                    {
                                        JobGiver_ExitMapBest jobGiver_Rescue = new JobGiver_ExitMapBest();
                                        ThinkResult resultLeave = jobGiver_Rescue.TryIssueJobPackage(merc.pawn, new JobIssueParams());
                                        bool isValidLeave = resultLeave.IsValid;

                                        if (isValidLeave)
                                        {
                                            merc.pawn.jobs.StartJob(resultLeave.Job, JobCondition.InterruptForced);
                                        }
                                    }
                                    else
                                    if (squad.order == MilitaryOrders.RecoverWounded)
                                    {
                                        JobGiver_RescueNearby jobGiver_Rescue = new JobGiver_RescueNearby();
                                        ThinkResult resultRescue = jobGiver_Rescue.TryIssueJobPackage(merc.pawn, new JobIssueParams());
                                        bool isValidRescue = resultRescue.IsValid;

                                        if (isValidRescue)
                                        {
                                            merc.pawn.jobs.StartJob(resultRescue.Job, JobCondition.InterruptForced);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (merc.pawn.drafter != null)
                                    merc.pawn.drafter.Drafted = false;
                                if (merc.pawn.Faction != settlementFaction)
                                    merc.pawn.SetFaction(settlementFaction);
                            }
                        }



                        //end of if pawn is mobile
                    }
                    else 
                    {
                        //if pawn is down or dead
                        if (merc.pawn.health.Dead) //if pawn is on map and is dead
                        {
                            //squad.removeDroppedEquipment();
                            //squad.PassPawnToDeadMercenaries(merc);
                            //squad.hasDead = true;
                        } else
                        { //If alive but downed
                            if (merc.pawn.drafter != null)
                                merc.pawn.drafter.Drafted = false;
                            if (merc.pawn.Faction != settlementFaction)
                                merc.pawn.SetFaction(settlementFaction);

                        }
                    }

                

                
               
            }


            //Log.Message("2");
            foreach (Mercenary animal in squad.DeployedMercenaryAnimals)
            {
                //if on map
                deployed = true;
                if (animal.pawn.health.State == PawnHealthState.Mobile)
                {
                    animal.pawn.mindState.duty = new PawnDuty();
                    animal.pawn.mindState.duty.def = DutyDefOf.Defend;
                    animal.pawn.mindState.duty.attackDownedIfStarving = false;
                    //animal.pawn.mindState.duty.radius = 2;
                    animal.pawn.mindState.duty.focus = animal.handler.pawn;
                    //If master is not dead
                    JobGiver_AIFightEnemies jobGiver = new JobGiver_AIFightEnemies();
                    ThinkResult result = jobGiver.TryIssueJobPackage(animal.pawn, new JobIssueParams());
                    bool isValid = result.IsValid;
                    if (isValid)
                    {
                        //Log.Message("att");
                        if (animal.pawn.jobs.curJob.def != result.Job.def)
                        {
                            animal.pawn.jobs.StartJob(result.Job, JobCondition.InterruptForced);
                        }
                    }
                    else
                    {
                        animal.pawn.mindState.duty.def = DutyDefOf.Defend;
                        animal.pawn.mindState.duty.radius = 2;
                        animal.pawn.mindState.duty.focus = animal.handler.pawn;
                        //if defend master not valid, follow master
                        JobGiver_AIFollowEscortee jobGiverFollow = new JobGiver_AIFollowEscortee();
                        ThinkResult resultFollow = jobGiverFollow.TryIssueJobPackage(animal.pawn, new JobIssueParams());
                        bool isValidFollow = resultFollow.IsValid;
                        if (isValidFollow)
                        {
                            //Log.Message("foloor");
                            if (animal.pawn.jobs.curJob.def != resultFollow.Job.def)
                            {
                                animal.pawn.jobs.StartJob(resultFollow.Job, JobCondition.Ongoing);
                            }
                        }
                        else
                        {
                            JobGiver_ExitMapBest jobGiver_Rescue = new JobGiver_ExitMapBest();
                            ThinkResult resultLeave = jobGiver_Rescue.TryIssueJobPackage(animal.pawn, new JobIssueParams());
                            bool isValidLeave = resultLeave.IsValid;

                            if (isValidLeave)
                            {
                                animal.pawn.jobs.StartJob(resultLeave.Job, JobCondition.InterruptForced);
                            }
                        }
                    }



                }

            }

            if (deployed == false && squad.hitMap)
            {
                foreach (Mercenary merc in squad.mercenaries)
                {
                    merc.pawn.SetFaction(settlementFaction);
                }
                squad.hasLord = false;
                squad.isDeployed = false;
                squad.removeDroppedEquipment();

                if (squad.map != null)
                {
                    squad.map.lordManager.RemoveLord(squad.lord);
                    squad.lord = null;
                    squad.map = null;
                }
                squad.hitMap = false;

                if (squad.isExtraSquad)
                {
                    militaryCustomizationUtil.mercenarySquads.Remove(squad);
                    //Log.Message("Squad deleted");
                    return;
                } else 
                { 
                    squad.getSettlement.cooldownMilitary(); 
                }

                //Log.Message("Reseting Squad");
                militaryCustomizationUtil.checkMilitaryUtilForErrors();
                squad.OutfitSquad(squad.outfit);
                

            }
        }
    }
}
