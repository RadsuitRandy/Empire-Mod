using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FactionColonies.util
{
	class DeliveryEvent
	{
		public static TraverseParms DeliveryTraverseParms => new TraverseParms()
		{
			canBashDoors = false,
			canBashFences = false,
			alwaysUseAvoidGrid = false,
			fenceBlocked = false,
			maxDanger = Danger.Deadly,
			mode = TraverseMode.ByPawn
		};

		public static void Action(FCEvent evt)
		{
			Action(evt, null, null);
		}

		public static void Action(FCEvent evt, Letter let = null, Message msg = null, bool CanUseShuttle = false)
        {
			Action(evt.goods, let, msg, CanUseShuttle || Find.World.GetComponent<FactionFC>().settlements.First(settlement => settlement.mapLocation == evt.source).traits.Contains(FCTraitEffectDefOf.shuttlePort), evt.source);
        }

		private static void MakeDeliveryLetterAndMessage(Letter let, Message msg, List<Thing> things)
		{
			try
			{
				if (let != null)
				{
					let.lookTargets = things;
					Find.LetterStack.ReceiveLetter(let);
				}

				if (msg != null)
				{
					msg.lookTargets = things;
					Messages.Message(msg);
				}
			} 
			catch
            {
				Log.ErrorOnce("MakeDeliveryLetterAndMessage failed to attach targets to the message", 908347458);
            }
		}

		public static void Action(List<Thing> things, Letter let = null, Message msg = null, bool canUseShuttle = false, int source = -1)
		{
			Map playerHomeMap = Find.World.GetComponent<FactionFC>().TaxMap;

			if (DefDatabase<ResearchProjectDef>.GetNamed("TransportPod").IsFinished)
			{
				if (ModsConfig.RoyaltyActive && canUseShuttle)
				{

					List<ShipLandingArea> landingZones = ShipLandingBeaconUtility.GetLandingZones(playerHomeMap);

					IntVec3 landingCell = DropCellFinder.GetBestShuttleLandingSpot(playerHomeMap, Faction.OfPlayer);

					if (!landingZones.Any() || landingZones.Any(zone => zone.Clear))
					{
						MakeDeliveryLetterAndMessage(let, msg, things);
						Thing shuttle = ThingMaker.MakeThing(ThingDefOf.Shuttle);
						TransportShip transportShip = TransportShipMaker.MakeTransportShip(TransportShipDefOf.Ship_Shuttle, things, shuttle);

						transportShip.ArriveAt(landingCell, playerHomeMap.Parent);
						transportShip.AddJobs(new ShipJobDef[]
						{
								ShipJobDefOf.Unload,
								ShipJobDefOf.FlyAway
						});
					} 
					else
                    {
						if (let != null && msg != null)
                        {
							Messages.Message(((string)"shuttleLandingBlockedWithItems".Translate(things.ToLetterString())).Replace("\n", ""), MessageTypeDefOf.RejectInput);
                        }

						if (source == -1) source = playerHomeMap.Tile;

						DeliveryEventParams eventParams = new DeliveryEventParams
						{
							Location = Find.AnyPlayerHomeMap.Tile,
							Source = source,
							PlanetName = Find.World.info.name,
							Contents = things,
							CustomDescription = "shuttleLandingBlocked".Translate(),
							timeTillTriger = Find.TickManager.TicksGame + 1000
						};

						CreateDeliveryEvent(eventParams);
					}
				}
				else
				{
					MakeDeliveryLetterAndMessage(let, msg, things);
					DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(playerHomeMap), playerHomeMap, things, 110, false, false, true, false);
				}
			}
			else
			{
				MakeDeliveryLetterAndMessage(let, msg, things);
				if (FactionColonies.Settings().disableTaxDeliveryCaravan)
                {
					things.ForEach(thing => PaymentUtil.placeThing(thing));
					return;
                }

				List<Pawn> pawns = new List<Pawn>();
				while (things.Count() > 0)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(FCPawnGenerator.WorkerOrMilitaryRequest);
					Thing next = things.First();

					if (pawn.carryTracker.innerContainer.TryAdd(next))
					{
						things.Remove(next);
					}

					pawns.Add(pawn);
				}

				PawnsArrivalModeWorker_EdgeWalkIn pawnsArrivalModeWorker = new PawnsArrivalModeWorker_EdgeWalkIn();
				IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, playerHomeMap);
				parms.spawnRotation = Rot4.FromAngleFlat((((Map)parms.target).Center - parms.spawnCenter).AngleFlat);

				RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, playerHomeMap, CellFinder.EdgeRoadChance_Friendly);

				pawnsArrivalModeWorker.Arrive(pawns, parms);
				LordMaker.MakeNewLord(FCPawnGenerator.WorkerOrMilitaryRequest.Faction, new LordJob_DeliverSupplies(parms.spawnCenter), playerHomeMap, pawns);
			}
		}

		public static void CreateDeliveryEvent(DeliveryEventParams evtParams)
		{
			FCEvent evt = FCEventMaker.MakeEvent(FCEventDefOf.deliveryArrival);
			evt.source = evtParams.Source;
			evt.goods = evtParams.Contents.ToList();
			evt.classToRun = "FactionColonies.util.DeliveryEvent";
			evt.classMethodToRun = "Action";
			evt.passEventToClassMethodToRun = true;
			evt.customDescription = evtParams.CustomDescription;
			evt.hasCustomDescription = true;
			evt.timeTillTrigger = evtParams.timeTillTriger;

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

		public static IntVec3 GetDeliveryCell(TraverseParms traverseParms, Map map)
		{
			if (!PaymentUtil.checkForTaxSpot(map, out IntVec3 intVec3))
			{
				intVec3 = ValidLandingCell(new IntVec2(1, 1), map, true);
			}

			IntVec3 oldVec = intVec3;
			for (int i = 0; i < 10; i++)
			{
				if (CellFinder.TryFindRandomReachableCellNear(intVec3, map, i, traverseParms, cell => map.thingGrid.ThingsAt(cell) == null, null, out intVec3))
				{
					break;
				}

				if (i == 9)
				{
					intVec3 = oldVec;
				}

			}

			return intVec3;
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
