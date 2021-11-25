using System.Collections.Generic;
using System.Linq;
using FactionColonies.util;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FactionColonies
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    class PawnDraftGizmos
    {
        static void Postfix(ref Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            List<Gizmo> output = __result.ToList();
            if (__result == null || __instance?.Faction == null || !output.Any() ||
                !(__instance.Map.Parent is WorldSettlementFC))
            {
                return;
            }

            Pawn found = __instance;
            Pawn_DraftController pawnDraftController = __instance.drafter ?? new Pawn_DraftController(__instance);

            WorldSettlementFC settlementFc = (WorldSettlementFC)__instance.Map.Parent;
            if (__instance.Faction.Equals(FactionColonies.getPlayerColonyFaction()))
            {
                Command_Toggle draftColonists = new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_ColonistDraft,
                    isActive = () => false,
                    toggleAction = () =>
                    {
                        if (pawnDraftController.pawn.Faction.Equals(Faction.OfPlayer)) return;
                        pawnDraftController.pawn.SetFaction(Faction.OfPlayer);
                        pawnDraftController.Drafted = true;
                    },
                    defaultDesc = "CommandToggleDraftDesc".Translate(),
                    icon = TexCommand.Draft,
                    turnOnSound = SoundDefOf.DraftOn,
                    groupKey = 81729172,
                    defaultLabel = "CommandDraftLabel".Translate()
                };
                if (pawnDraftController.pawn.Downed) draftColonists.Disable("IsIncapped".Translate(pawnDraftController.pawn.LabelShort, pawnDraftController.pawn));
                draftColonists.tutorTag = "Draft";
                output.Add(draftColonists);
            }
            else if (__instance.Faction.Equals(Faction.OfPlayer) && __instance.Drafted &&
                     !settlementFc.supporting.Any(caravan => caravan.pawns.Any(pawn => pawn.Equals(found))))
            {
                foreach (Command_Toggle action in output.Where(gizmo => gizmo is Command_Toggle))
                {
                    if (action.hotKey != KeyBindingDefOf.Command_ColonistDraft)
                    {
                        continue;
                    }

                    int index = output.IndexOf(action);
                    action.toggleAction = () =>
                    {
                        found.SetFaction(FactionColonies.getPlayerColonyFaction());
                        //settlementFc.worldSettlement.defenderLord.AddPawn(__instance);
                    };
                    output[index] = action;
                    break;
                }
            }

            __result = output;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    class PrisonerGizmosPatch
    {
        static void Postfix(ref Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            Pawn pawn = __instance;
            if (__instance.guest == null || !__instance.guest.IsPrisoner || !__instance.guest.PrisonerIsSecure || !Find.World.GetComponent<FactionFC>().settlements.Any()) return;

            __result = __result.Append(new Command_Action
            {
                defaultLabel = "SendToSettlement".Translate(),
                defaultDesc = "",
                icon = TexLoad.iconMilitary,
                action = delegate
                {
                    if (pawn.Map.dangerWatcher.DangerRating != StoryDanger.None)
                    {
                        Messages.Message("cantSendWithDangerLevel".Translate(pawn.Map.dangerWatcher.DangerRating.ToString()), MessageTypeDefOf.RejectInput);
                        return;
                    }

                    List<FloatMenuOption> settlementList = Find.World.GetComponent<FactionFC>().settlements.Select(settlement => new FloatMenuOption(settlement.name + " - Settlement Level : " + settlement.settlementLevel + " - Prisoners: " + settlement.prisonerList.Count(), delegate
                    {
                        //disappear colonist
                        FactionColonies.sendPrisoner(pawn, settlement);

                        foreach (var bed in Find.Maps.Where(map => map.IsPlayerHome).SelectMany(map => map.listerBuildings.allBuildingsColonist).OfType<Building_Bed>().Where(bed => bed.OwnersForReading.Any(bedPawn => bedPawn == pawn)))
                        {
                            bed.ForPrisoners = false;
                            bed.ForPrisoners = true;
                        }
                    })).ToList();

                    FloatMenu floatMenu2 = new FloatMenu(settlementList)
                    {
                        vanishIfMouseDistant = true
                    };
                    Find.WindowStack.Add(floatMenu2);
                }
            });
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
            if (__instance.def.defName != "Settlement") return;
            int tile = __instance.Tile;
            if (__instance.Faction != fact && __instance.Faction != Find.FactionManager.OfPlayer)
            {
                //if a valid faction to target
                Faction faction = __instance.Faction;

                Command_Action actionHostile = new Command_Action
                {
                    defaultLabel = "AttackSettlement".Translate(),
                    defaultDesc = "",
                    icon = TexLoad.iconMilitary,
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();

                        if (!worldcomp.hasPolicy(FCPolicyDefOf.isolationist))
                            list.Add(new FloatMenuOption("CaptureSettlement".Translate(), delegate
                            {
                                List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                                foreach (SettlementFC settlement in worldcomp.settlements)
                                {
                                    if (settlement.isMilitaryValid())
                                    {
                                        //if military is valid to use.

                                        settlementList.Add(new FloatMenuOption(
                                            settlement.name + " " + "ShortMilitary".Translate() + " " +
                                            settlement.settlementMilitaryLevel + " - " +
                                            "FCAvailable".Translate() + ": " +
                                            (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                            {
                                                if (settlement.isMilitaryBusy())
                                                {
                                                }
                                                else
                                                {
                                                    RelationsUtilFC.attackFaction(faction);

                                                    settlement.sendMilitary(tile, Find.World.info.name,
                                                        MilitaryJob.CaptureEnemySettlement, 60000, faction);


                                                    //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                                }
                                            }
                                        ));
                                    }
                                }

                                if (settlementList.Count == 0)
                                {
                                    settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(),
                                        null));
                                }

                                FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                floatMenu2.vanishIfMouseDistant = true;
                                Find.WindowStack.Add(floatMenu2);
                            }));


                        list.Add(new FloatMenuOption("RaidSettlement".Translate(), delegate
                        {
                            List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                            foreach (SettlementFC settlement in worldcomp.settlements)
                            {
                                if (settlement.isMilitaryValid())
                                {
                                    //if military is valid to use.

                                    settlementList.Add(new FloatMenuOption(
                                        settlement.name + " " + "ShortMilitary".Translate() + " " +
                                        settlement.settlementMilitaryLevel + " - " + "FCAvailable".Translate() +
                                        ": " + (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                        {
                                            if (settlement.isMilitaryBusy())
                                            {
                                                //military is busy
                                            }
                                            else
                                            {
                                                RelationsUtilFC.attackFaction(faction);

                                                settlement.sendMilitary(tile, Find.World.info.name,
                                                    MilitaryJob.RaidEnemySettlement, 60000, faction);


                                                //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                            }
                                        }
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

                        if (worldcomp.hasPolicy(FCPolicyDefOf.authoritarian) &&
                            faction.def.defName != "VFEI_Insect")
                        {
                            list.Add(new FloatMenuOption("EnslavePopulation".Translate(), delegate
                            {
                                List<FloatMenuOption> settlementList = new List<FloatMenuOption>();

                                foreach (SettlementFC settlement in worldcomp.settlements)
                                {
                                    if (settlement.isMilitaryValid())
                                    {
                                        //if military is valid to use.

                                        settlementList.Add(new FloatMenuOption(
                                            settlement.name + " " + "ShortMilitary".Translate() + " " +
                                            settlement.settlementMilitaryLevel + " - " +
                                            "FCAvailable".Translate() + ": " +
                                            (!settlement.isMilitaryBusySilent()).ToString(), delegate
                                            {
                                                if (settlement.isMilitaryBusy())
                                                {
                                                    //military is busy
                                                }
                                                else
                                                {
                                                    RelationsUtilFC.attackFaction(faction);

                                                    settlement.sendMilitary(tile, Find.World.info.name,
                                                        MilitaryJob.EnslaveEnemySettlement, 60000, faction);


                                                    //simulateBattleFC.FightBattle(militaryForce.createMilitaryForceFromSettlement(settlement), militaryForce.createMilitaryForceFromEnemySettlement(faction));
                                                }
                                            }
                                        ));
                                    }
                                }

                                if (settlementList.Count == 0)
                                {
                                    settlementList.Add(new FloatMenuOption("NoValidMilitaries".Translate(),
                                        null));
                                }

                                FloatMenu floatMenu2 = new FloatMenu(settlementList);
                                floatMenu2.vanishIfMouseDistant = true;
                                Find.WindowStack.Add(floatMenu2);


                                //set to raid settlement here
                            }));
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
                    icon = TexLoad.iconProsperity,
                    action = delegate { worldcomp.sendDiplomaticEnvoy(faction); }
                };

                if (worldcomp.hasPolicy(FCPolicyDefOf.pacifist))
                {
                    __result = __result.Concat(new[] { actionPeaceful });
                }
                else
                {
                    __result = __result.Concat(new[] { actionHostile });
                }
            }
        }
    }
}
