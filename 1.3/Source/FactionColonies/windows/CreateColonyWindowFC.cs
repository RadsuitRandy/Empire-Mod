using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using FactionColonies.util;

namespace FactionColonies
{
    public class CreateColonyWindowFc : Window
    {
        public sealed override Vector2 InitialSize => new Vector2(300f, 600f);

        public int currentTileSelected = -1;
        public BiomeResourceDef currentBiomeSelected; //DefDatabase<BiomeResourceDef>.GetNamed(this.biome)
        public BiomeResourceDef currentHillinessSelected;
        public bool traitExpansionistReducedFee;
        public int timeToTravel = -1;

        public CreateColonyWindowFc()
        {
            forcePause = false;
            draggable = false;
            preventCameraMotion = false;
            doCloseX = true;
            windowRect = new Rect(UI.screenWidth - InitialSize.x, (UI.screenHeight - InitialSize.y) / 2f - (UI.screenHeight/8f), 
                InitialSize.x, InitialSize.y);
        }



        //Pre-Opening
        public override void PreOpen()
        {

        }

        //Drawing
        public override void DoWindowContents(Rect inRect)
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();

            faction.roadBuilder.DrawPaths();

            if (Find.WorldSelector.selectedTile != -1 && Find.WorldSelector.selectedTile != currentTileSelected)
            {
                currentTileSelected = Find.WorldSelector.selectedTile;
                //Log.Message("Current: " + currentTileSelected + ", Selected: " + Find.WorldSelector.selectedTile);
                currentBiomeSelected = DefDatabase<BiomeResourceDef>.GetNamed(
                    Find.WorldGrid.tiles[currentTileSelected].biome.ToString(), false);
                //default biome
                if (currentBiomeSelected == default(BiomeResourceDef))
                {
                    //Log Modded Biome
                    currentBiomeSelected = BiomeResourceDefOf.defaultBiome;
                }
                currentHillinessSelected = DefDatabase<BiomeResourceDef>.GetNamed(
                    Find.WorldGrid.tiles[currentTileSelected].hilliness.ToString());
                if (currentBiomeSelected.canSettle && currentHillinessSelected.canSettle && currentTileSelected != 1)
                {
                    timeToTravel = FactionColonies.ReturnTicksToArrive(
                        Find.World.GetComponent<FactionFC>().capitalLocation, currentTileSelected);
                }
                else
                {
                    timeToTravel = 0;
                }
            }


            //grab before anchor/font
            GameFont fontBefore = Text.Font;
            TextAnchor anchorBefore = Text.Anchor;


            int silverToCreateSettlement = (int)(TraitUtilsFC.cycleTraits(new double(), "createSettlementMultiplier", Find.World.GetComponent<FactionFC>().traits, Operation.Multiplikation) * (FactionColonies.silverToCreateSettlement + (500 * (Find.World.GetComponent<FactionFC>().settlements.Count() + Find.World.GetComponent<FactionFC>().settlementCaravansList.Count())) + (TraitUtilsFC.cycleTraits(new double(), "createSettlementBaseCost", Find.World.GetComponent<FactionFC>().traits, Operation.Addition))));
            if (faction.hasPolicy(FCPolicyDefOf.isolationist))
                silverToCreateSettlement *= 2;

            if (faction.hasPolicy(FCPolicyDefOf.expansionist)){
                if (!faction.settlements.Any() && !faction.settlementCaravansList.Any())
                {
                    traitExpansionistReducedFee = false;
                    silverToCreateSettlement = 0;
                } else
                {
                    if (faction.traitExpansionistTickLastUsedSettlementFeeReduction == -1 || (faction.traitExpansionistBoolCanUseSettlementFeeReduction))
                    {
                        traitExpansionistReducedFee = true;
                        silverToCreateSettlement /= 2;
                    } else
                    {
                        traitExpansionistReducedFee = false;
                    }
                }
            }


            //Draw Label
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0, 0, 268, 40), "SettleANewColony".Translate());

            //hori line
            Widgets.DrawLineHorizontal(0, 40, 300);


            //Upper menu
            Widgets.DrawMenuSection(new Rect(5, 45, 258, 220));

            DrawLabelBox(10, 50, 100, 100, "TravelTime".Translate(), timeToTravel.ToStringTicksToDays());
            DrawLabelBox(153, 50, 100, 100, "InitialCost".Translate(), silverToCreateSettlement + " " + "Silver".Translate());


            //Lower Menu label
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0, 270, 268, 40), "BaseProductionStats".Translate());


            //Lower menu
            Widgets.DrawMenuSection(new Rect(5, 310, 258, 220));


            //Draw production
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            //Production headers
            Widgets.Label(new Rect(40, 310, 60, 25), "Base".Translate());
            Widgets.Label(new Rect(110, 310, 60, 25), "Modifier".Translate());
            Widgets.Label(new Rect(180, 310, 60, 25), "Final".Translate());

            if (currentTileSelected != -1)
            {
                foreach (ResourceType titheType in ResourceUtils.resourceTypes)
                {
                    int titheTypeInt = (int)titheType;
                    int baseHeight = 15;
                    if(Widgets.ButtonImage(new Rect(20, 335 + titheTypeInt * (5 + baseHeight), baseHeight, baseHeight), faction.returnResource(titheType).getIcon()))
                    {
                        string label = faction.returnResource(titheType).label;

                        Find.WindowStack.Add(new DescWindowFc("SettlementProductionOf".Translate() + ": " + label, label.CapitalizeFirst()));
                    }

                    float xMod = 70f;
                    Rect baseRect = new Rect(40, 335 + titheTypeInt * (5 + baseHeight), 60, baseHeight + 2);

                    double titheAddBaseProductionCurBiome = currentBiomeSelected.BaseProductionAdditive[titheTypeInt];
                    double titheAddBaseProductionCurHilli = currentHillinessSelected.BaseProductionAdditive[titheTypeInt];

                    double titheMultBaseProductionCurBiome = currentBiomeSelected.BaseProductionMultiplicative[titheTypeInt];
                    double titheMultBaseProductionCurHilli = currentHillinessSelected.BaseProductionMultiplicative[titheTypeInt];

                    Widgets.Label(baseRect, (titheAddBaseProductionCurBiome + titheAddBaseProductionCurHilli).ToString());
                    Widgets.Label(baseRect.CopyAndShift(xMod, 0f), (titheMultBaseProductionCurBiome * titheMultBaseProductionCurHilli).ToString());
                    Widgets.Label(baseRect.CopyAndShift(xMod * 2f, 0f), ((titheAddBaseProductionCurBiome + titheAddBaseProductionCurHilli) * (titheMultBaseProductionCurBiome * titheMultBaseProductionCurHilli)).ToString());
                }
            }




            //Settle button
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            int buttonLength = 130;
            if(Widgets.ButtonText(new Rect((InitialSize.x - 32 - buttonLength)/2f, 535, buttonLength, 32), 
                "Settle".Translate() + ": (" + silverToCreateSettlement + ")")) //add inital cost
            { //if click button to settle
                if (PaymentUtil.getSilver() >= silverToCreateSettlement) //if have enough monies to make new settlement
                {
                    StringBuilder reason = new StringBuilder();
                    if (currentTileSelected == -1 || util.WorldTileChecker.AnyWorldSettlementFCAtOrAdjacent(currentTileSelected, reason) || !TileFinder.IsValidTileForNewSettlement(currentTileSelected, reason) ||
                        Find.World.GetComponent<FactionFC>().checkSettlementCaravansList(currentTileSelected.ToString()))
                    {
                        //Alert Error to User
                        Messages.Message(reason.ToString(), MessageTypeDefOf.NegativeEvent);
                    }
                    else
                    {   //Else if valid tile

                        PaymentUtil.paySilver(silverToCreateSettlement);
                        //if PROCESS MONEY HERE

                        if (traitExpansionistReducedFee)
                        {
                            faction.traitExpansionistTickLastUsedSettlementFeeReduction = Find.TickManager.TicksGame;
                            faction.traitExpansionistBoolCanUseSettlementFeeReduction = false;
                        }

                        //create settle event
                        FCEvent tmp = FCEventMaker.MakeEvent(FCEventDefOf.settleNewColony);
                        tmp.location = currentTileSelected;
                        tmp.planetName = Find.World.info.name;
                        tmp.timeTillTrigger = Find.TickManager.TicksGame + timeToTravel;
                        tmp.source = Find.World.GetComponent<FactionFC>().capitalLocation;
                        Find.World.GetComponent<FactionFC>().addEvent(tmp);

                        Find.World.GetComponent<FactionFC>().settlementCaravansList.Add(tmp.location.ToString());
                        Messages.Message("CaravanSentToLocation".Translate() 
                                         + " " + (tmp.timeTillTrigger
                                                  -Find.TickManager.TicksGame).ToStringTicksToDays() + "!", 
                            MessageTypeDefOf.PositiveEvent);
                        // when event activate FactionColonies.createPlayerColonySettlement(currentTileSelected);
                    }
                } else
                {  //if don't have enough monies to make settlement
                    Messages.Message("NotEnoughSilverToSettle".Translate() + "!", MessageTypeDefOf.NeutralEvent);
                }
            }

            //reset anchor/font
            Text.Font = fontBefore;
            Text.Anchor = anchorBefore;
        }

        public void DrawLabelBox(int x, int y, int length, int height, string text1, string text2)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            //Draw highlight
            Widgets.DrawHighlight(new Rect(x, y+height/8, length, height / 4f));
            Widgets.Label(new Rect(x, y, length, height / 2f), text1);

            //divider
            Widgets.DrawLineHorizontal(x + 5, y + height / 2, length - 10);

            //Bottom Text - Gamers Rise Up
            Widgets.Label(new Rect(x, y + height / 2, length, height / 2f), text2);
        }
    }
}
