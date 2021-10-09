using Verse;

namespace FactionColonies.util
{
	static class Extensions
	{
		public static bool IsAnimalAndAllowed(this PawnKindDef pawnKindDef)
		{
			return pawnKindDef.race.race.Animal && pawnKindDef.RaceProps.IsFlesh && pawnKindDef.race.tradeTags != null &&
									!pawnKindDef.race.tradeTags.Contains("AnimalDryad") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalMonster") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalGenetic") &&
									!pawnKindDef.race.tradeTags.Contains("AnimalAlpha");
		}
	}
}
