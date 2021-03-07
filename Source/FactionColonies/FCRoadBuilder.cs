using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace FactionColonies
{
    public class FCRoadBuilder : IExposable
    {

        //Dictionary <
        public List<FCRoadBuilderQueue> roadQueues = new List<FCRoadBuilderQueue>();
        public RoadDef roadDef;
        public int techUpdateDueDate;

        public FCRoadBuilder()
        {
            this.techUpdateDueDate = Find.TickManager.TicksGame;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<RoadDef>(ref roadDef, "roadDef");
            Scribe_Values.Look<int>(ref techUpdateDueDate, "techUpdateDueDate");
            Scribe_Collections.Look<FCRoadBuilderQueue>(ref roadQueues, "roadQueues", LookMode.Deep, new object[] {Find.World.info.name, this.roadDef });
        }



        public void RoadTick()
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            int days = 3;
            if (faction.hasTrait(FCPolicyDefOf.roadBuilders))
                days = 1;

            if (this.techUpdateDueDate <= Find.TickManager.TicksGame)
            {
                updateRoadTech();
                this.techUpdateDueDate += 60000;
            }

            if (roadDef == null)
            {
                return;
            }
            foreach (FCRoadBuilderQueue queue in roadQueues.Where(x => x.planetName == Find.World.info.name))
            {
                if (queue.nextRoadTick <= Find.TickManager.TicksGame)
                {
                    //Calculate roads
                    //if node already has piece of road, go to next.
                    createNextSegment(queue);

                    
                    

                    queue.nextRoadTick += GenDate.TicksPerDay * days;


                    
                }
            }
        }

        public bool isNewRoadBetter(RoadDef old, RoadDef newroad)
        {
            if (old == newroad)
            {
                return false;
            }
            if (newroad == RoadDefOf.AncientAsphaltHighway)
            {
                return true;
            }
            if (newroad == RoadDefOf.AncientAsphaltRoad && old != RoadDefOf.AncientAsphaltHighway)
            {
                return true;
            }
            if (newroad == RoadDefOf.DirtRoad && old == DefDatabase<RoadDef>.GetNamed("DirtPath"))
            {
                return true;
            }
            return false;
        }
        public void createNextSegment(FCRoadBuilderQueue queue)
        {
            WorldGrid grid = Find.WorldGrid;
            int num = 1;
            //Log.Message("Loop initiated =====");
            List<int> tilesConstructed = new List<int>();
            List<WorldPath> removeQueue = new List<WorldPath>();
            foreach (WorldPath path in queue.ToBuild)
            {
                //Log.Message(num + " - start");
                num += 1;
                List<int> nodeList = (List<int>)(typeof(WorldPath).GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(path));
                typeof(WorldPath).GetField("curNodeIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(path, nodeList.Count - 1, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null);
                //Log.Message("New Path - " + nodeList.Count());
                WorldPath newPath = new WorldPath();
                newPath = path;
                int nodes = newPath.NodesLeftCount;
                int prevtile = newPath.FirstNode;
                int tile;

                //create segments
                //check if segment already has road
                //if not, create segment and return

                //do same but backwards
                for (int i = 0; i < nodes-1; i++)
                {
                    
                    if (newPath.NodesLeftCount == 1)
                    {
                        tile = newPath.LastNode;
                    }
                    else
                    {
                        tile = newPath.ConsumeNextNode();
                    }
                    if (tilesConstructed.Contains(prevtile)) 
                    {
                        //Log.Message(num + " tried to make a road from an already constructed tile");
                        goto Next;
                    }

                    //Log.Message(tile + "n - o" + prevtile);
                    RoadDef def = grid.GetRoadDef(prevtile, tile);
                    if (def == roadDef)
                    {
                        prevtile = tile;
                        continue;
                    }
                    if (def != null && isNewRoadBetter(def, roadDef))
                    {
                        grid.tiles[prevtile].potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == tile);
                        grid.tiles[tile].potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == prevtile);

                        grid.tiles[prevtile].potentialRoads.Add(new Tile.RoadLink { neighbor = tile, road = roadDef });
                        grid.tiles[tile].potentialRoads.Add(new Tile.RoadLink { neighbor = prevtile, road = roadDef });
                        tilesConstructed.Add(tile);

                    } else if (def == null)//if null
                    {
                        if (grid.tiles[prevtile].potentialRoads == null)
                        {
                            grid.tiles[prevtile].potentialRoads = new List<Tile.RoadLink>();
                        }
                        if (grid.tiles[tile].potentialRoads == null)
                        {
                            grid.tiles[tile].potentialRoads = new List<Tile.RoadLink>();
                        }
                        grid.tiles[prevtile].potentialRoads.Add(new Tile.RoadLink { neighbor = tile, road = roadDef });
                        grid.tiles[tile].potentialRoads.Add(new Tile.RoadLink { neighbor = prevtile, road = roadDef });
                        tilesConstructed.Add(tile);
                        if (tile == newPath.LastNode)
                        {
                            //Log.Message("Removed " + num + " from queue");
                            removeQueue.Add(path);
                        }
                    } else
                    {
                        prevtile = tile;
                        continue;
                    }

                    //Regen worldmap
                    try
                    {
                        Find.World.renderer.SetDirty<WorldLayer_Roads>();
                        Find.World.renderer.SetDirty<WorldLayer_Paths>();
                        Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(prevtile);
                        Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(tile);
                    }
                    catch (Exception e)
                    {
                    }
                    //Log.Message("Created road step");
                    goto Next;
                }
                Next:
                continue;
            }

            foreach (WorldPath queueRemove in removeQueue)
            {
                //Log.Message("Removing queue");
                queue.ToBuild.Remove(queueRemove);
            }

        }
        public void displayPaths()
        {
            foreach (FCRoadBuilderQueue queue in roadQueues)
            {
                foreach (WorldPath path in queue.ToBuild)
                {
                    path.DrawPath(null);
                }
            }
        }

        public void createRoadQueue(string planetName)
        {
            if (roadQueues.Where(x => x.planetName == planetName).Count() > 0) 
            {
                Log.Message("Empire - Road queue for " + planetName + " already exists.");
                return;
            }
            FCRoadBuilderQueue queue = new FCRoadBuilderQueue(planetName, roadDef);
            roadQueues.Add(queue);
        }

        public void calculateRoadPathForWorld()
        {
            foreach (FCRoadBuilderQueue queue in roadQueues.Where(x => x.planetName == Find.World.info.name))
            {
                //Log.Message("Empire - Calculating road paths for " + queue.planetName);
                calculateRoadPath(queue);
            }
        }
        public void calculateRoadPath(FCRoadBuilderQueue queue)
        {
            Log.Message("Empire - RoadBuilderQueue - Check for null roadDef");
            if (roadDef == null)
            {
                //Log.Message("===== Road def Null =====");
                return;
            }
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            List<WorldPath> buildQueue = new List<WorldPath>();

            Log.Message("Empire - RoadBuilderQueue - Check for null faction");
            if (faction == null)
            {
                Log.Message("Faction returned null - FCRoadBuilder.calculateRoadPath");
                return;
            }

            List<int> settlementLocations = new List<int>();

            if (queue.planetName == null)
            {
                Log.Message("PlanetName null - Reseting to current...");
                queue.planetName = Find.World.info.name;
            }
            Log.Message("Empire - RoadBuilderQueue - Get Faction settlements");
            //get list of settlements
            foreach (SettlementFC settlement in faction.settlements)
            {
                if (settlement.planetName != null && settlement.planetName == queue.planetName)
                {
                    if (settlement.mapLocation == null || settlement.mapLocation == -1)
                    {
                        Log.Message("Could not find proper settlement for tile location");
                        return;
                    }
                    settlementLocations.Add(settlement.mapLocation);
                }
            }
            Log.Message("Empire - RoadBuilderQueue - Add player settlement locations");
            //TO DO -- add player settlement locations here
            foreach (Settlement settlement in Find.World.worldObjects.Settlements)
            {
                if (settlement.Faction != null && settlement.Faction.IsPlayer)
                {
                    settlementLocations.Add(settlement.Tile);
                }
                Log.Message("Empire - RoadBuilderQueue - Add allied settlement locations");

                if (faction.hasTrait(FCPolicyDefOf.roadBuilders))
                {
                    if (settlement.Faction != null && settlement.Faction != Find.FactionManager.OfPlayer && settlement.Faction.PlayerRelationKind != null && settlement.Faction.PlayerRelationKind == FactionRelationKind.Ally && settlement.Tile !=  null)
                    {
                        settlementLocations.Add(settlement.Tile);
                    }
                }
            }
            Log.Message("Empire - RoadBuilderQueue - Pair with every other settlement");
            //from each settlement location, pair up with every other settlement location
            for (int i = settlementLocations.Count() - 1; i > 0; i--)
            {
                for (int k = 0; k < (settlementLocations.Count() - 1); k++)
                {

                    //Log.Message("3 - " + i + "i = k" + k);
                    WorldPath path = Find.WorldPathFinder.FindPath(settlementLocations[i], settlementLocations[k], null, null);
                    if (path != null && path != WorldPath.NotFound)
                    {
                        if (!testPath(path))
                        {
                            buildQueue.Add(path);
                        }

                    }
                    
                }
                settlementLocations.RemoveAt(i);
            }
            queue.ToBuild = buildQueue;
            //Log.Message(queue.ToBuild.Count().ToString() + " number of road paths calculated");
        }

        public bool testPath(WorldPath path)
        {
            WorldGrid grid = Find.WorldGrid;
            WorldPath testpath = new WorldPath();
            testpath = path;
            List<int> nodeList = (List<int>)(typeof(WorldPath).GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(testpath));
            typeof(WorldPath).GetField("curNodeIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(testpath, nodeList.Count - 1, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null);

            int nodes = testpath.NodesLeftCount;
            int prevtile = testpath.FirstNode;
            int tile;
            bool hasRoad = true;

            for (int i = 0; i < nodes - 1; i++)
            {
                if (testpath.NodesLeftCount == 1)
                {
                    tile = testpath.LastNode;
                }
                else
                {
                    tile = testpath.ConsumeNextNode();
                }

                RoadDef def = grid.GetRoadDef(prevtile, tile);
                if (def == roadDef)
                {
                    prevtile = tile;
                    continue;
                }
                if (def != null && isNewRoadBetter(def, roadDef))
                {
                    return false; //road creatable
                } else if (def == null)
                {
                    return false; //road creatable
                } else //if road exists, but new road is not better
                {
                    //Make no changes
                }




                prevtile = tile;
                continue;
            }

            return true;
        }

        public void updateRoadTech()
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            RoadDef def = this.roadDef;
            if (def == RoadDefOf.AncientAsphaltHighway)
            {
                return;
            }
            if (Find.ResearchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingHighway", false)) == DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingHighway", false).baseCost)
            {
                def = RoadDefOf.AncientAsphaltHighway;
                //Log.Message("Empire roadDef updated to " + def.ToString());

            }
            else if (Find.ResearchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingRoad", false)) == DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingRoad", false).baseCost)
            {
                def = RoadDefOf.AncientAsphaltRoad;
                //Log.Message("Empire roadDef updated to " + def.ToString());

            }
            else if (def != RoadDefOf.AncientAsphaltHighway && Find.ResearchManager.GetProgress(DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingDirt", false)) == DefDatabase<ResearchProjectDef>.GetNamed("FCRoadBuildingDirt", false).baseCost)
            {
                def = RoadDefOf.DirtRoad;
                //Log.Message("Empire roadDef updated to " + def.ToString());
            }
            foreach (FCRoadBuilderQueue queue in roadQueues)
            {
                queue.roadDef = def;
            }
            this.roadDef = def;
            calculateRoadPathForWorld();
        }
    }



    public class FCRoadBuilderQueue: IExposable
    {
        public string planetName;
        public int nextRoadTick;
        public RoadDef roadDef;
        public List<WorldPath> ToBuild = new List<WorldPath>();

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref planetName, "planetName");
            Scribe_Values.Look<int>(ref nextRoadTick, "nextRoadTick");
            Scribe_Defs.Look<RoadDef>(ref roadDef, "roadDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Find.World.GetComponent<FactionFC>().roadBuilder.calculateRoadPath(this);
            }
        }
        public FCRoadBuilderQueue(string planetName, RoadDef roadDef)
        {
            this.planetName = planetName;
            this.ToBuild = new List<WorldPath>();
            this.roadDef = roadDef;
            this.nextRoadTick = Find.TickManager.TicksGame + 60000;
        }

       
    }
}
