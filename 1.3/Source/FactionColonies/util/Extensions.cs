﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FactionColonies.util
{
	static class Extensions
	{
		/// <summary>
		///		Checks if a given <c>PawnKindDef</c> <paramref name="pawnKindDef"/> is an Animal and if it is not blacklisted by tradeTag 
		/// </summary>
		/// <param name="pawnKindDef"></param>
		/// <returns></returns>
		public static bool IsAnimalAndAllowed(this PawnKindDef pawnKindDef)
		{
			return pawnKindDef.race.race.Animal && pawnKindDef.RaceProps.IsFlesh && 
									pawnKindDef.race.race.animalType != AnimalType.Dryad &&
									pawnKindDef.race.tradeTags != null &&
									!pawnKindDef.race.tradeTags.Contains("AnimalDryad") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalMonster") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalGenetic") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalAlpha");
		}

		public static Rect CopyAndShift(this Rect rect, Vector2 vector2)
		{
			return CopyAndShift(rect, vector2.x, vector2.y);
		}

		public static bool IsMercenary(this Pawn pawn) => Find.World.GetComponent<FactionFC>().militaryCustomizationUtil.AllMercenaryPawns.Contains(pawn);

		public static Rect CopyAndShift(this Rect rect, float x, float y)
		{
			Rect newRect = new Rect(rect);
			newRect.x += x;
			newRect.y += y;

			return newRect;
		}

		public static void ApplyIdeologyRitualWounds(this Pawn pawn)
		{
			if (ModsConfig.IdeologyActive)
			{
				if (pawn.Ideo.HasPrecept(PreceptDefOf.AgeReversal_Demanded))
				{
					pawn.ageTracker.DebugResetAgeReversalDemand();
				}
				if (pawn.ideo.Ideo.BlindPawnChance > 0)
				{
					pawn.Blind();
				}
				if (pawn.ideo.Ideo.RequiredScars > 0 && !pawn.HasTrait(TraitDefOf.Wimp))
				{
					pawn.Scarify();
				}
			}
		}

		public static void Blind(this Pawn pawn)
		{
			foreach (BodyPartRecord part in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.SightSource))
			{
				if (!pawn.health.hediffSet.PartIsMissing(part))
				{
					Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, null);
					hediff_MissingPart.lastInjury = HediffDefOf.Cut;
					hediff_MissingPart.Part = part;
					hediff_MissingPart.IsFresh = false;
					pawn.health.AddHediff(hediff_MissingPart, part, null, null);
				}
			}

		}

		public static void Scarify(this Pawn pawn)
		{
			if (!ModLister.CheckIdeology("Scarification")) return;

			int num = 0;
			using (List<Hediff>.Enumerator enumerator = pawn.health.hediffSet.hediffs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.def == HediffDefOf.Scarification)
					{
						num++;
					}
				}
			}

			for (int i = num; i < pawn.ideo.Ideo.RequiredScars; i++)
			{
				IEnumerable<BodyPartRecord> partsToApplyOn = JobDriver_Scarify.GetPartsToApplyOn(pawn);
				List<BodyPartRecord> list = partsToApplyOn.Where((BodyPartRecord p) => JobDriver_Scarify.AvailableOnNow(pawn, p)).ToList();
				BodyPartRecord part = list.RandomElement();
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Scarification, pawn, part);
				HediffComp_GetsPermanent hediffComp_GetsPermanent = hediff.TryGetComp<HediffComp_GetsPermanent>();
				hediffComp_GetsPermanent.IsPermanent = true;
				hediffComp_GetsPermanent.SetPainCategory(JobDriver_Scarify.InjuryPainCategories.RandomElementByWeight((HealthTuning.PainCategoryWeighted e) => e.weight).category);
				pawn.health.AddHediff(hediff, null, null, null);
			}
		}

		public static bool HasTrait(this Pawn pawn, TraitDef trait) => pawn?.ideo?.Ideo == null || pawn.health == null || (pawn.story?.traits?.HasTrait(trait) ?? false);
	}
}
