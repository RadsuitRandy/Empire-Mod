using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace FactionColonies
{
    public class SettlementCustomizeWindowFc : Window
    {
        public override Vector2 InitialSize
        {
            get { return new Vector2(445f, 280f); }
        }

        //declare variables

        //private int xspacing = 60;
        private int yspacing = 30;

        private int yoffset = 50;

        //private int headerSpacing = 30;
        private int length = 400;
        private int xoffset = 0;
        private int height = 200;


        private SettlementFC settlement;

        public string desc;
        public string header;

        private string name;
        private string title;


        public SettlementCustomizeWindowFc(SettlementFC settlement)
        {
            this.forcePause = false;
            this.draggable = true;
            this.doCloseX = true;
            this.preventCameraMotion = false;
            this.settlement = settlement;
            this.header = "CustomizeSettlement".Translate();
            this.name = settlement.name;
            //this.title = faction.title;
        }

        public override void PreOpen()
        {
            base.PreOpen();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();
            //faction.title = title;
            settlement.name = name;
            WorldSettlementFC settlementFc =
                Find.WorldObjects.WorldObjectAt<WorldSettlementFC>(settlement.mapLocation);
            if (settlementFc != null) settlementFc.Name = name;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //grab before anchor/font
            GameFont fontBefore = Text.Font;
            TextAnchor anchorBefore = Text.Anchor;


            //Settlement Tax Collection Header
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(3, 3, 300, 60), header);


            Text.Font = GameFont.Small;
            for (int i = 0; i < 1; i++) //for each field to customize
            {
                switch (i)
                {
                    case 0: //faction name
                        Widgets.Label(new Rect(xoffset + 3, yoffset + yspacing * i, length / 4, yspacing),
                            "SettlementName".Translate() + ": ");
                        name = Widgets.TextField(
                            new Rect(xoffset + 3 + length / 4 + 5, yoffset + yspacing * i, length / 2, yspacing), name);
                        break;

                    case 1: //faction title
                        Widgets.Label(new Rect(xoffset + 3, yoffset + yspacing * i, length / 4, yspacing),
                            "## title: ");
                        title = Widgets.TextField(
                            new Rect(xoffset + 3 + length / 4 + 5, yoffset + yspacing * i, length / 2, yspacing),
                            title);
                        break;

                    case 2: //faction icon
                        Widgets.Label(new Rect(xoffset + 3, yoffset + yspacing * i, length / 4, yspacing), "## Icon: ");
                        if (Widgets.ButtonImage(new Rect(xoffset + 3 + length / 4 + 5, yoffset + yspacing * i, 40, 40),
                            TexLoad.iconUnrest)) //change to faction icon
                        {
                            //Log.Message("Faction icon select pressed");
                            //Open window to select new icon
                        }

                        break;
                }
            }

            if (Widgets.ButtonText(new Rect((InitialSize.x - 120 - 18) / 2, yoffset + InitialSize.y - 120, 120, 30),
                "ConfirmChanges".Translate()))
            {
                settlement.name = name;
                Settlement check = Find.WorldObjects.SettlementAt(settlement.mapLocation);
                if (check != null)
                {
                    check.Name = name;
                }
            }

            //settlement buttons

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Tiny;

            //0 tithe total string
            //1 source - -1
            //2 due/delivery date
            //3 Silver (- || +)
            //4 tithe


            Widgets.Label(new Rect(xoffset + 2, yoffset - yspacing + 2, length - 4, height - 4 + yspacing * 2), desc);
            Widgets.DrawBox(new Rect(xoffset, yoffset - yspacing, length, height - yspacing * 2));

            //reset anchor/font
            Text.Font = fontBefore;
            Text.Anchor = anchorBefore;
        }
    }
}