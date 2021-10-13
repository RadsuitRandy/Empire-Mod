using System.Linq;
using System.Text;
using Verse;

namespace FactionColonies.util
{
    public static class WorldTileChecker
    {
        public static bool AnyWorldSettlementFCAtOrAdjacent(int tile, StringBuilder reason = null)
        {
            foreach (WorldSettlementFC settlement in Find.WorldObjects.AllWorldObjects.Where(obj => obj.GetType() == typeof(WorldSettlementFC)))
            {
                Log.Message("Type: " + settlement.GetType().Name);
                if (Find.WorldGrid.IsNeighborOrSame(settlement.Tile, tile))
                {
                    reason?.Append("FactionBaseAdjacent".Translate());
                    return true;
                }
            }

            return false;
        }
    }
}
