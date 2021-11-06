using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Unity;
using UnityEngine;
using System.Reflection;
using FactionColonies.util;

namespace FactionColonies
{
    class FCPrisonerMenu : Window
    {
        public List<FCPrisoner> prisoners;
        public SettlementFC settlement;
        public FactionFC faction;

        public int scroll = 0;
        public int maxScroll;
        public int scrollBoxHeight = 440;

        public int optionHeight = 90;

        //


        public Rect optionBox;
        public Rect optionPawnIcon;
        public Rect optionPawnName;
        public Rect optionPawnHealth;
        public Rect optionPawnWorkload;
        public Rect optionPawnUnrest;
        public Rect optionButtonInfo;
        public Rect optionButtonAction;

        

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(538f, 478f);  //19
            }
        }





        public FCPrisonerMenu(SettlementFC settlement)
        {
            //Window Information
            this.faction = Find.World.GetComponent<FactionFC>();
            this.settlement = settlement;
            this.prisoners = settlement.prisonerList;

            this.scroll = 0;
            this.maxScroll = (prisoners.Count() * optionHeight) - scrollBoxHeight;


            //Window Properties
            this.forcePause = false;
            this.draggable = true;
            this.doCloseX = true;
            this.preventCameraMotion = false;


            optionBox = new Rect(0, 0, 500, optionHeight);
            //rect for pawn image
            optionPawnIcon = new Rect(optionBox.x, optionBox.y + 10, 50, 50);
            //rect for pawn name
            optionPawnName = new Rect(optionBox.x + 50, optionBox.y + 5, 300, 20);
            //rect for pawn health
            optionPawnHealth = new Rect(optionPawnName.x, optionPawnName.y + 20, 300, 20);
            //rect for pawn unrest
            optionPawnUnrest = new Rect(optionPawnHealth.x, optionPawnHealth.y + 20, 300, 20);
            //rect for pawn workload
            optionPawnWorkload = new Rect(optionPawnUnrest.x, optionPawnUnrest.y + 20, 150, 20);
            //rect for info button
            optionButtonInfo = new Rect(optionBox.x + optionBox.width - 150 - 10, optionPawnHealth.y + 20, 150, 20);
            //rect for action button
            optionButtonAction = new Rect(optionBox.x + optionBox.width - 150 - 10, optionPawnUnrest.y + 20, 150, 20);

        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            this.maxScroll = (prisoners.Count() * optionHeight) - scrollBoxHeight;
        }



        public override void DoWindowContents(Rect inRect)
        {
            //grab before anchor/font
            GameFont fontBefore = Text.Font;
            TextAnchor anchorBefore = Text.Anchor;

            //top label
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;



            Text.Anchor = TextAnchor.MiddleLeft;

            int i = 0;
            foreach (FCPrisoner prisoner in prisoners)
            {
                Rect Box;
                Rect PawnIcon;
                Rect PawnName;
                Rect PawnHealth;
                Rect PawnUnrest;
                Rect PawnWorkload;
                Rect ButtonInfo;
                Rect ButtonAction;

                Box = optionBox;
                PawnIcon = optionPawnIcon;
                PawnName = optionPawnName;
                PawnHealth = optionPawnHealth;
                PawnUnrest = optionPawnUnrest;
                PawnWorkload = optionPawnWorkload;
                ButtonInfo = optionButtonInfo;
                ButtonAction = optionButtonAction;

                Box.y += scroll + optionHeight * i;
                PawnIcon.y += scroll + optionHeight * i;
                PawnName.y += scroll + optionHeight * i;
                PawnHealth.y += scroll + optionHeight * i;
                PawnUnrest.y += scroll + optionHeight * i;
                PawnWorkload.y += scroll + optionHeight * i;
                ButtonInfo.y += scroll + optionHeight * i;
                ButtonAction.y += scroll + optionHeight * i;


                //display stuff now
                Widgets.DrawMenuSection(Box);
                //on every other box
                if (i % 2 == 0)
                {
                    Widgets.DrawHighlight(Box);
                }

                //show pawn;
                Widgets.ThingIcon(PawnIcon, prisoner.prisoner);
                //Pawn Name
                Widgets.Label(PawnName, prisoner.prisoner.Name.ToString());
                //Pawn Health
                Widgets.Label(PawnHealth, "Health".Translate().CapitalizeFirst() + " " + prisoner.health);
                //Pawn Unrest
                    //Widgets.Label(PawnUnrest, "Unrest".Translate().CapitalizeFirst() + " " + prisoner.unrest);



                //Pawn Workload
                string workload;
                switch (prisoner.workload)
                {
                    case FCWorkLoad.Heavy:
                        workload = "FCHeavy".Translate().CapitalizeFirst();
                        break;
                    case FCWorkLoad.Medium:
                        workload = "FCMedium".Translate().CapitalizeFirst();
                        break;
                    case FCWorkLoad.Light:
                        workload = "FCLight".Translate().CapitalizeFirst();
                        break;
                    default:
                        workload = "null";
                        break;
                }
                if (Widgets.ButtonText(PawnWorkload, "FCWorkload".Translate().CapitalizeFirst() + ": " + workload))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("FCHeavy".Translate().CapitalizeFirst() + " - " + "FCHeavyExplanation".Translate(), delegate
                    {
                        prisoner.workload = FCWorkLoad.Heavy;
                    }));
                    list.Add(new FloatMenuOption("FCMedium".Translate().CapitalizeFirst() + " - " + "FCMediumExplanation".Translate(), delegate
                    {
                        prisoner.workload = FCWorkLoad.Medium;
                    }));
                    list.Add(new FloatMenuOption("FCLight".Translate().CapitalizeFirst() + " - " + "FCLightExplanation".Translate(), delegate
                    {
                        prisoner.workload = FCWorkLoad.Light;
                    }));
                    FloatMenu menu = new FloatMenu(list);
                    Find.WindowStack.Add(menu);
                }

                //Info Button
                if (Widgets.ButtonTextSubtle(ButtonInfo, "ViewInfo".Translate()))
                {
                    Pawn pawn = new Pawn();
                    pawn = prisoner.prisoner;

                    if (prisoner.healthTracker != null)
                    {
                        prisoner.prisoner.health = prisoner.healthTracker;
                    }
                    else
                    {
                        prisoner.prisoner.health = new Pawn_HealthTracker(prisoner.prisoner);
                        prisoner.healthTracker = new Pawn_HealthTracker(prisoner.prisoner);
                    }

                    pawn.health = prisoner.healthTracker;


                    Find.WindowStack.Add(new Dialog_InfoCard(pawn));
                }

                //Action button
                if (Widgets.ButtonTextSubtle(ButtonAction, "Actions".Translate()))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();

                    list.Add(new FloatMenuOption("SellPawn".Translate() + " $" + prisoner.prisoner.MarketValue + " " + "SellPawnInfo".Translate(), delegate
                    
                    {
                        settlement.addSilverIncome(prisoner.prisoner.MarketValue);

                        //reset window
                        prisoners.Remove(prisoner);
                        WindowUpdate();
                        return;

                    }));

                    list.Add(new FloatMenuOption("ReturnToPlayer".Translate(), delegate
                    {
                        if (prisoner.healthTracker != null)
                        {
                            prisoner.prisoner.health = prisoner.healthTracker;
                        } else
                        {
                            prisoner.prisoner.health = new Pawn_HealthTracker(prisoner.prisoner);
                            prisoner.healthTracker = new Pawn_HealthTracker(prisoner.prisoner);
                        }

                        if (!HealthUtility.TryAnesthetize(prisoner.prisoner)) HealthUtility.DamageUntilDowned(prisoner.prisoner, false);

                        if (prisoner.prisoner.guest == null)
                        {
                            prisoner.prisoner.guest = new Pawn_GuestTracker();
                        }
                        prisoner.prisoner.guest.guestStatusInt = GuestStatus.Prisoner;
                        FieldInfo hostFaction = typeof(Pawn_GuestTracker).GetField("hostFactionInt", BindingFlags.NonPublic | BindingFlags.Instance);
                        hostFaction.SetValue(prisoner.prisoner.guest, Find.FactionManager.OfPlayer);

                        DeliveryEvent.CreateDeliveryEvent(new FCEvent
                        {
                                location = Find.AnyPlayerHomeMap.Tile,
                                source = settlement.mapLocation,
                                planetName = settlement.planetName,
                                goods = new List<Thing> { prisoner.prisoner },
                                customDescription = "aPrisonerIsBeingDeliveredToYou".Translate(),
                                timeTillTrigger = Find.TickManager.TicksGame + FactionColonies.ReturnTicksToArrive(settlement.mapLocation, Find.AnyPlayerHomeMap.Tile)
                        });

                        //reset window
                        prisoners.Remove(prisoner);
                        WindowUpdate();
                        return;
                    }));


                    FloatMenu menu = new FloatMenu(list);
                    Find.WindowStack.Add(menu);
                }
                



                //increment i
                i++;
            }

            Text.Font = fontBefore;
            Text.Anchor = anchorBefore;

            if (Event.current.type == EventType.ScrollWheel)
            {

                scrollWindow(Event.current.delta.y);
            }

        }


        private void scrollWindow(float num)
        {
            if (scroll - num * 5 < -1 * maxScroll)
            {
                scroll = -1 * maxScroll;
            }
            else if (scroll - num * 5 > 0)
            {
                scroll = 0;
            }
            else
            {
                scroll -= (int)Event.current.delta.y * 5;
            }
            Event.current.Use();
        }
    }
}
