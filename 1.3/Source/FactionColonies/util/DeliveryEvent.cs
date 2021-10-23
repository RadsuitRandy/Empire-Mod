using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace FactionColonies.util
{
	class DeliveryEvent
	{
		public static void Action(FCEvent evt)
		{
			Map playerHomeMap = Find.World.GetComponent<FactionFC>().returnCapitalMap();
			if (DefDatabase<ResearchProjectDef>.GetNamed("TransportPod").IsFinished)
			{
				if (ModsConfig.RoyaltyActive)
				{
					Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
					TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, evt.goods, shuttle);
					transportShip.ArriveAt(ValidLandingCell(shuttle.def.size, playerHomeMap), playerHomeMap.Parent);
					transportShip.AddJobs(new ShipJobDef[]
					{
								ShipJobDefOf.Unload,
								ShipJobDefOf.FlyAway
					});
				}
				else
				{
					DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(playerHomeMap), playerHomeMap, evt.goods);
				}
			}
			else
			{
				List<Pawn> pawns = new List<Pawn>();
				while(evt.goods.Count() > 0)
				{
					PawnGenerationRequest request = new PawnGenerationRequest
					{
						KindDef = FactionColonies.getPlayerColonyFaction()?.RandomPawnKind(),
						Faction = FactionColonies.getPlayerColonyFaction(),
						Context = PawnGenerationContext.NonPlayer,
						Tile = -1,
						ForceGenerateNewPawn = false,
						Newborn = false,
						AllowDead = false,
						AllowDowned = false,
						CanGeneratePawnRelations = true,
						MustBeCapableOfViolence = false,
						ColonistRelationChanceFactor = 0,
						Inhabitant = false,
						CertainlyBeenInCryptosleep = false,
						ForceRedressWorldPawnIfFormerColonist = false,
						WorldPawnFactionDoesntMatter = false,
						BiocodeApparelChance = 0,
						ExtraPawnForExtraRelationChance = null,
						RelationWithExtraPawnChanceFactor = 0
					};

					Pawn pawn = PawnGenerator.GeneratePawn(request);
					Thing next = evt.goods.First();

					if (pawn.carryTracker.innerContainer.TryAdd(next))
					{
						evt.goods.Remove(next);
					}
					pawns.Add(pawn);
				}
				PawnsArrivalModeWorker_EdgeWalkIn pawnsArrivalModeWorker = new PawnsArrivalModeWorker_EdgeWalkIn();
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, playerHomeMap);
				parms.spawnRotation = Rot4.FromAngleFlat((((Map)parms.target).Center - parms.spawnCenter).AngleFlat);

				RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, playerHomeMap, CellFinder.EdgeRoadChance_Friendly);

				pawnsArrivalModeWorker.Arrive(pawns, parms);
				//Thing resultingThing = null;

				foreach(Pawn pawn in pawns)
				{
					if (!PaymentUtil.checkForTaxSpot(playerHomeMap, out IntVec3 intVec3) != true)
					{
						intVec3 = DropCellFinder.TradeDropSpot(playerHomeMap);
					}
					Job job = new Job(DefDatabase<JobDef>.GetNamed("GotoAndDrop"), intVec3);
					pawn.jobs.StartJob(job);
				}
			}
		}

		public static void CreateShuttleEvent(FCEventParams evtParams)
		{
			FCEvent evt = FCEventMaker.MakeEvent(FCEventDefOf.deliveryArrival);
			evt.source = evtParams.Source;
			evt.goods = evtParams.Contents.ToList();
			evt.classToRun = "FactionColonies.util.ShuttleEvent";
			evt.classMethodToRun = "Action";
			evt.passEventToClassMethodToRun = true;
			evt.customDescription = evtParams.CustomDescription;
			evt.hasCustomDescription = true;

			Find.World.GetComponent<FactionFC>().addEvent(evt);
		}

		public static string ShuttleEventInjuredString
		{
			get
			{
				if (DefDatabase<ResearchProjectDef>.GetNamed("TransportPod", false).IsFinished)
				{
					if (ModsConfig.RoyaltyActive)
					{
						return "transportingInjuredShuttle".Translate();
					}
					return "transportingInjuredDropPod".Translate();
				}
				return "transportingInjuredCaravan".Translate();
			}
		}

		private static IntVec3 ValidLandingCell(IntVec2 requiredSpace, Map map, bool canLandRoofed = false)
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
