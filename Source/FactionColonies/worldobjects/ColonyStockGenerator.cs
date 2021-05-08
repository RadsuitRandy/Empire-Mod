using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FactionColonies
{
    public class ColonyStockGenerator : StockGenerator
    {
        private StockGenerator parent;
        private FactionFC faction = Find.World.GetComponent<FactionFC>();

        public ColonyStockGenerator(StockGenerator parent)
        {
            this.parent = parent;
        }
        
        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            return parent.GenerateThings(forTile, faction);
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.techLevel <= faction.techLevel && parent.HandlesThingDef(thingDef);
        }


        public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
        {
            return parent.ConfigErrors(parentDef);
        }
        
        public override void ResolveReferences(TraderKindDef trader)
        {
            parent.ResolveReferences(trader);
        }
    }
}