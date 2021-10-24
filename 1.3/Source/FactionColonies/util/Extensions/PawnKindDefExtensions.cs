using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FactionColonies
{
	static class PawnKindDefExtensions
	{
		public static bool IsHumanLikeRace(this PawnKindDef pawnKindDef)
        {
			return pawnKindDef.race.race?.intelligence == Intelligence.Humanlike && pawnKindDef.race.BaseMarketValue != 0;
		}

		public static bool IsHumanlikeWithLabelRace(this PawnKindDef pawnKindDef)
        {
			return pawnKindDef?.race?.label != null && pawnKindDef.IsHumanLikeRace();
		}

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
