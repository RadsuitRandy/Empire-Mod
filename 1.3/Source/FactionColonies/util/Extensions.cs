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
	}
}
