using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FactionColonies.util
{
    class ShuttleEvent
    {
        public static void Action(FCEvent evt)
        {
            if (ModsConfig.RoyaltyActive)
            {
                Map playerHomeMap = Find.AnyPlayerHomeMap;
                Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
                TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, evt.goods, shuttle);
                transportShip.ArriveAt(ValidLandingCell(shuttle.def.size, playerHomeMap), playerHomeMap.Parent);
                transportShip.AddJobs(new ShipJobDef[]
                {
                            ShipJobDefOf.Unload,
                            ShipJobDefOf.FlyAway
                });
            }
        }

        public static void CreateShuttleEvent(FCEventParams evtParams)
        {
            FCEvent evt = FCEventMaker.MakeEvent(FCEventDefOf.transportShipArrival);
            evt.source = evtParams.Source;
            evt.goods = evtParams.Contents.ToList();
            evt.classToRun = "FactionColonies.util.ShuttleEvent";
            evt.classMethodToRun = "Action";
            evt.passEventToClassMethodToRun = true;
            evt.customDescription = "transportingInjuredShuttle".Translate();
            evt.hasCustomDescription = true;

            Find.World.GetComponent<FactionFC>().addEvent(evt);
        }

        private static IntVec3 ValidLandingCell(IntVec2 requiredSpace, Map map, bool canLandRoofed = false, Area inArea = null)
        {
            IEnumerable<IntVec3> validCells = map.areaManager.Home.ActiveCells.Where(cell => (!map.roofGrid.Roofed(cell) || canLandRoofed) && cell.CellFullFillsSpaceRequirement(requiredSpace, map));

            if (validCells.Count() == 0)
            {
                validCells = map.areaManager.Home.ActiveCells.Where(cell => cell.CellFullFillsSpaceRequirement(requiredSpace, map));
            }

            if (validCells.Count() == 0)
            {
                validCells = map.AllCells.Where(cell => !map.areaManager.Home.ActiveCells.Contains(cell));
            }

            if (validCells.Count() == 0)
            {
                validCells = map.AllCells;
            }

            return validCells.RandomElement();
        }

    }
}
