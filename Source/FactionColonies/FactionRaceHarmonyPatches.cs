using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FactionColonies
{


    //
    //[HarmonyPatch(typeof(Faction), "TryGenerateNewLeader")] 
    class ReplaceMe
    {
        static bool Prefix(ref Faction __instance)
        {
            //return true to continue
            return false;

           //return false to skip original
        }
    }
	//


	//Note - these classes basically just copy vanilla but with a couple changes

	//
	[HarmonyPatch(typeof(Faction), "TryGenerateNewLeader")] 
	class FactionTryGenerateNewLeader
    {
        static bool Prefix(ref Faction __instance, ref bool __result)
        {
			if (__instance.def.defName == "PColony")
			{
				//Log.Message("gen leader - is colony");
				Pawn pawn = __instance.leader;
				__instance.leader = null;
				if (__instance.def.generateNewLeaderFromMapMembersOnly)
				{
					for (int i = 0; i < Find.Maps.Count; i++)
					{
						Map map = Find.Maps[i];
						for (int j = 0; j < map.mapPawns.AllPawnsCount; j++)
						{
							if (map.mapPawns.AllPawns[j] != pawn && !map.mapPawns.AllPawns[j].Destroyed && map.mapPawns.AllPawns[j].FactionOrExtraMiniOrHomeFaction == __instance)
							{
								__instance.leader = map.mapPawns.AllPawns[j];
							}
						}
					}
				}
				else if (__instance.def.pawnGroupMakers != null)
				{
					List<PawnKindDef> list = new List<PawnKindDef>();

					foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs) {
						list.Add(def.race.AnyPawnKind);
					}

					if (__instance.def.fixedLeaderKinds != null)
					{
						list.AddRange(__instance.def.fixedLeaderKinds);
					}
					PawnKindDef kind;
					if (list.TryRandomElement(out kind))
					{
						PawnGenerationRequest request = new PawnGenerationRequest(kind, __instance, PawnGenerationContext.NonPlayer, -1, __instance.def.leaderForceGenerateNewPawn, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null);
						__instance.leader = PawnGenerator.GeneratePawn(request);
						if (__instance.leader.RaceProps.IsFlesh)
						{
							__instance.leader.relations.everSeenByPlayer = true;
						}
						if (!Find.WorldPawns.Contains(__instance.leader))
						{
							Find.WorldPawns.PassToWorld(__instance.leader, PawnDiscardDecideMode.KeepForever);
						}
					}
				}
				__result = __instance.leader != null;
				return false;
			} else
			{
				//Log.Message("gen leader - not colony");
				return true;
			}
		}
    }
	//

	//
	[HarmonyPatch(typeof(PawnGroupKindWorker_Normal), "GeneratePawns")]
	class PawnGroupKindWorker_NormalGeneratePawns
	{
		static bool Prefix(ref PawnGroupKindWorker_Normal __instance, PawnGroupMakerParms parms, PawnGroupMaker groupMaker, List<Pawn> outPawns, bool errorOnZeroResults = true)
		{
			if (parms.faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}

				



				if (!__instance.CanGenerateFrom(parms, groupMaker))
				{
					if (errorOnZeroResults)
					{
						Log.Error(string.Concat(new object[]
						{
						"Cannot generate pawns for ",
						parms.faction,
						" with ",
						parms.points,
						". Defaulting to a single random cheap group."
						}), false);
					}
					return false;
				}
				bool allowFood = parms.raidStrategy == null || parms.raidStrategy.pawnsCanBringFood || (parms.faction != null && !parms.faction.HostileTo(Faction.OfPlayer));
				Predicate<Pawn> validatorPostGear = (parms.raidStrategy != null) ? ((Pawn p) => parms.raidStrategy.Worker.CanUsePawn(p, outPawns)) : (Predicate<Pawn>)null;
				bool flag = false;
				foreach (PawnGenOption pawnGenOption in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.options, parms))
				{

					PawnKindDef pawnkind = new PawnKindDef();
					pawnkind = list.RandomElement();
					pawnkind.techHediffsTags = pawnGenOption.kind.techHediffsTags;
					pawnkind.apparelTags = pawnGenOption.kind.apparelTags;
					pawnkind.isFighter = pawnGenOption.kind.isFighter;
					pawnkind.combatPower = pawnGenOption.kind.combatPower;
					pawnkind.gearHealthRange = pawnGenOption.kind.gearHealthRange;
					pawnkind.weaponTags = pawnGenOption.kind.weaponTags;
					pawnkind.apparelMoney = pawnGenOption.kind.apparelMoney;
					pawnkind.weaponMoney = pawnGenOption.kind.weaponMoney;
					pawnkind.apparelAllowHeadgearChance = pawnGenOption.kind.apparelAllowHeadgearChance;
					pawnkind.techHediffsMoney = pawnGenOption.kind.techHediffsMoney;
					pawnkind.label = pawnGenOption.kind.label;


					Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind , parms.faction, PawnGenerationContext.NonPlayer, parms.tile, false, false, false, false, true, true, 1f, false, true, allowFood, true, parms.inhabitants, false, false, false, 0f, null, 1f, null, validatorPostGear, null, null, null, null, null, null, null, null, null, null));
					if (parms.forceOneIncap && !flag)
					{
						pawn.health.forceIncap = true;
						pawn.mindState.canFleeIndividual = false;
						flag = true;
					}
					outPawns.Add(pawn);
				}
				return false;
			} else
			{
				return true;
			}
		}
	}
	//


	//
	[HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GenerateGuards")]
	class PawnGroupKindWorker_TraderGenerateGuards
	{
		static bool Prefix(ref PawnGroupKindWorker_Trader __instance, PawnGroupMakerParms parms, PawnGroupMaker groupMaker, Pawn trader, List<Thing> wares, List<Pawn> outPawns)
		{

			if (parms.faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}

				if (!groupMaker.guards.Any<PawnGenOption>())
				{
					return false;
				}
				foreach (PawnGenOption pawnGenOption in PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints(parms.points, groupMaker.guards, parms))
				{
					PawnKindDef pawnkind = new PawnKindDef();
					pawnkind = list.RandomElement();
					pawnkind.techHediffsTags = pawnGenOption.kind.techHediffsTags;
					pawnkind.apparelTags = pawnGenOption.kind.apparelTags;
					pawnkind.isFighter = pawnGenOption.kind.isFighter;
					pawnkind.combatPower = pawnGenOption.kind.combatPower;
					pawnkind.gearHealthRange = pawnGenOption.kind.gearHealthRange;
					pawnkind.weaponTags = pawnGenOption.kind.weaponTags;
					pawnkind.apparelMoney = pawnGenOption.kind.apparelMoney;
					pawnkind.weaponMoney = pawnGenOption.kind.weaponMoney;
					pawnkind.apparelAllowHeadgearChance = pawnGenOption.kind.apparelAllowHeadgearChance;
					pawnkind.techHediffsMoney = pawnGenOption.kind.techHediffsMoney;
					pawnkind.label = pawnGenOption.kind.label;

					PawnGenerationRequest request = PawnGenerationRequest.MakeDefault();
					request.KindDef = pawnkind;
					//Log.Message(request.KindDef.ToString());
					request.Faction = parms.faction;
					request.Tile = parms.tile;
					request.MustBeCapableOfViolence = true;
					request.Inhabitant = parms.inhabitants;
					request.RedressValidator = ((Pawn x) => x.royalty == null || !x.royalty.AllTitlesForReading.Any<RoyalTitle>());
					Pawn item = PawnGenerator.GeneratePawn(request);
					outPawns.Add(item);
				}

				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//



	//
	[HarmonyPatch(typeof(PawnGroupKindWorker_Trader), "GenerateTrader")]
	class PawnGroupKindWorker_TraderGenerateTrader
	{
		static bool Prefix(ref PawnGroupKindWorker_Trader __instance, ref Pawn __result, PawnGroupMakerParms parms, PawnGroupMaker groupMaker, TraderKindDef traderKind)
		{

			if (parms.faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}

				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(list.RandomElement(), parms.faction, PawnGenerationContext.NonPlayer, parms.tile, false, false, false, false, true, false, 1f, false, true, true, true, parms.inhabitants, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null));
				pawn.mindState.wantsToTradeWithColony = true;
				PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
				pawn.trader.traderKind = traderKind;
				parms.points -= pawn.kindDef.combatPower;
				__result = pawn;


				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//


	//
	//[HarmonyPatch(typeof(QuestGen_Pawns), "GeneratePawn")] --Disabled Currently
	class QuestGen_PawnsQuestGeneratePawn
	{
		static bool Prefix(ref Pawn __result, Quest quest, PawnKindDef kindDef, Faction faction, bool allowAddictions = true, IEnumerable<TraitDef> forcedTraits = null, float biocodeWeaponChance = 0f, bool mustBeCapableOfViolence = true, Pawn extraPawnForExtraRelationChance = null, float relationWithExtraPawnChanceFactor = 0f, float biocodeApparelChance = 0f, bool ensureNonNumericName = false, bool forceGenerateNewPawn = false)
		{

			if (faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}



				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(list.RandomElement(), faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn, false, false, false, true, mustBeCapableOfViolence, 1f, false, true, true, allowAddictions, false, false, false, false, biocodeWeaponChance, extraPawnForExtraRelationChance, relationWithExtraPawnChanceFactor, null, null, forcedTraits, null, null, null, null, null, null, null, null, null)
				{
					BiocodeApparelChance = biocodeApparelChance
				});
				if (ensureNonNumericName && (pawn.Name == null || pawn.Name.Numerical))
				{
					pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, null);
				}
				QuestGen.AddToGeneratedPawns(pawn);
				if (!pawn.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				}
				__result = pawn;


				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//


	//
	//[HarmonyPatch(typeof(QuestGen_Pawns), "GeneratePawn")] --Disabled currently
	class QuestGen_PawnsQuestGeneratePawn2
	{
		static bool Prefix(ref Pawn __result, QuestGen_Pawns.GetPawnParms parms, Faction faction = null)
		{

			if (faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}

				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(list.RandomElement(), faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null));
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any<RoyalTitle>())
				{
					QuestPart_Hyperlinks questPart_Hyperlinks = new QuestPart_Hyperlinks();
					questPart_Hyperlinks.pawns.Add(pawn);
					QuestGen.quest.AddPart(questPart_Hyperlinks);
				}
				__result = pawn;


				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//


	//
	[HarmonyPatch(typeof(QuestNode_GetPawn), "GeneratePawn")]
	class QuestNode_GetPawnGeneratePawn
	{
		static bool Prefix(ref QuestNode_GetPawn __instance, ref Pawn __result, Slate slate, Faction faction = null)
		{

			if (faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}


				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(list.RandomElement(), faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null));
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
				if (pawn.royalty != null && pawn.royalty.AllTitlesForReading.Any<RoyalTitle>())
				{
					QuestPart_Hyperlinks questPart_Hyperlinks = new QuestPart_Hyperlinks();
					questPart_Hyperlinks.pawns.Add(pawn);
					QuestGen.quest.AddPart(questPart_Hyperlinks);
				}
				__result = pawn;



				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//

	//
	[HarmonyPatch(typeof(RaidStrategyWorker), "SpawnThreats")]
	class RaidStrategyWorkerSpawnThreats
	{
		static bool Prefix(ref RaidStrategyWorker __instance, ref List<Pawn> __result, IncidentParms parms)
		{

			if (parms.faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list2 = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list2.Add(def.race.AnyPawnKind);
				}


				if (parms.pawnKind != null)
				{
					List<Pawn> list = new List<Pawn>();
					for (int i = 0; i < parms.pawnCount; i++)
					{
						PawnKindDef pawnKind = parms.pawnKind;
						Faction faction = parms.faction;
						PawnGenerationContext context = PawnGenerationContext.NonPlayer;
						int tile = -1;
						bool forceGenerateNewPawn = false;
						bool newborn = false;
						bool allowDead = false;
						bool allowDowned = false;
						bool canGeneratePawnRelations = true;
						bool mustBeCapableOfViolence = true;
						float colonistRelationChanceFactor = 1f;
						bool forceAddFreeWarmLayerIfNeeded = false;
						bool allowGay = true;
						float biocodeWeaponsChance = parms.biocodeWeaponsChance;

						PawnKindDef pawnkind = new PawnKindDef();
						pawnkind = list2.RandomElement();
						pawnkind.techHediffsTags = parms.pawnKind.techHediffsTags;
						pawnkind.apparelTags = parms.pawnKind.apparelTags;
						pawnkind.isFighter = parms.pawnKind.isFighter;
						pawnkind.combatPower = parms.pawnKind.combatPower;
						pawnkind.gearHealthRange = parms.pawnKind.gearHealthRange;
						pawnkind.weaponTags = parms.pawnKind.weaponTags;
						pawnkind.apparelMoney = parms.pawnKind.apparelMoney;
						pawnkind.weaponMoney = parms.pawnKind.weaponMoney;
						pawnkind.apparelAllowHeadgearChance = parms.pawnKind.apparelAllowHeadgearChance;
						pawnkind.techHediffsMoney = parms.pawnKind.techHediffsMoney;
						pawnkind.label = parms.pawnKind.label;
						Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnkind, faction, context, tile, forceGenerateNewPawn, newborn, allowDead, allowDowned, canGeneratePawnRelations, mustBeCapableOfViolence, colonistRelationChanceFactor, forceAddFreeWarmLayerIfNeeded, allowGay, __instance.def.pawnsCanBringFood, true, false, false, false, false, biocodeWeaponsChance, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null)
						{
							BiocodeApparelChance = 1f
						});
						if (pawn != null)
						{
							list.Add(pawn);
						}
					}
					if (list.Any<Pawn>())
					{
						parms.raidArrivalMode.Worker.Arrive(list, parms);
						__result = list;
						return false;
					}
				}
				__result = null;



				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//


	//
	[HarmonyPatch(typeof(ThingSetMaker_RefugeePod), "Generate")]
	class ThingSetMaker_RefugeePodGenerate
	{
		static bool Prefix(ref ThingSetMaker_RefugeePod __instance, ThingSetMakerParams parms, List<Thing> outThings)
		{
			Faction faction = DownedRefugeeQuestUtility.GetRandomFactionForRefugee();


			if (faction == FactionColonies.getPlayerColonyFaction())
			{
				List<PawnKindDef> list = new List<PawnKindDef>();
				foreach (ThingDef def in Find.World.GetComponent<FactionFC>().raceFilter.AllowedThingDefs)
				{
					list.Add(def.race.AnyPawnKind);
				}


				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, DownedRefugeeQuestUtility.GetRandomFactionForRefugee(), PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 20f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null));
				outThings.Add(pawn);
				HealthUtility.DamageUntilDowned(pawn, true);



				return false;
			}
			else
			{
				return true;
			}
		}
	}
	//






}
