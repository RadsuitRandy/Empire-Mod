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
}
