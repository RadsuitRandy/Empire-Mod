using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FactionColonies.util;
using Verse;
using RimWorld;
using UnityEngine;


namespace FactionColonies
{
    public class EmpireUIMercenaryCommandMenu : Window
    {

        public MercenarySquadFC selectedSquad;
        public string squadText;

        public EmpireUIMercenaryCommandMenu()
        {
            layer = WindowLayer.Super;
            closeOnClickedOutside = false;
            closeOnAccept = false;
            closeOnCancel = false;
            doCloseX = false;
            draggable = true;
            drawShadow = false;
            doWindowBackground = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(200f, 240f);
            }
        }


        protected override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect(UI.screenWidth - InitialSize.x, 0f, InitialSize.x, InitialSize.y);
        }

        public override void DoWindowContents(Rect rect) 
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            if(faction.militaryCustomizationUtil.DeployedSquads.Count() == 0)
            {
                Close();
            }

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            float rectBaseHeight = 40f;
            float rectWidth = 160f;

            Rect selectSquad = new Rect(0, 0, rectWidth, rectBaseHeight);
            Rect commandAttack = new Rect(0, rectBaseHeight, rectWidth, rectBaseHeight);
            Rect commandMove = new Rect(0, rectBaseHeight * 2, rectWidth, rectBaseHeight);
            Rect commandHeal = new Rect(0, rectBaseHeight * 3, rectWidth, rectBaseHeight);
            Rect killWindow = new Rect(0, rectBaseHeight * 4, rectWidth, rectBaseHeight);


            if (selectedSquad == null)
            {
                squadText = "selectDeployedSquad".Translate();
            } else
            {
                squadText = "selectedDeployedSquad".Translate(selectedSquad.getSettlement.name, selectedSquad.outfit.name);
            }

            //Select a squad
            if(Widgets.ButtonText(selectSquad, squadText))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (MercenarySquadFC squad in faction.militaryCustomizationUtil.DeployedSquads)
                {
                    if (squad.getSettlement != null)
                    {
                        list.Add(new FloatMenuOption("selectedDeployedSquad".Translate(squad.getSettlement.name, squad.outfit.name), delegate
                        {
                            selectedSquad = squad;
                        }));
                    }
                }
                if (!list.Any())
                {
                    list.Add(new FloatMenuOption("noSquadsAvailable".Translate(), null));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }



            if(Widgets.ButtonTextSubtle(commandAttack, "commandAttack".Translate()))
            {
                if (selectedSquad != null)
                {
                    selectedSquad.order = MilitaryOrders.Attack;
                    Messages.Message("attackSuccess".Translate(selectedSquad.outfit.name), MessageTypeDefOf.NeutralEvent);
                    //selectedSquad.orderLocation = Position;
                }
            }
            if (Widgets.ButtonTextSubtle(commandMove, "commandMove".Translate()))
            {
                if (selectedSquad != null)
                {
                    DebugTool tool;
                    IntVec3 Position;
                    tool = new DebugTool("Select Move Position", delegate ()
                    {
                        Position = UI.MouseCell();

                        selectedSquad.order = MilitaryOrders.Standby;
                        selectedSquad.orderLocation = Position;
                        Messages.Message("moveSuccess".Translate(selectedSquad.outfit.name), MessageTypeDefOf.NeutralEvent);

                        DebugTools.curTool = null;
                    });
                    DebugTools.curTool = tool;
                }
            }
            if (Widgets.ButtonTextSubtle(commandHeal, "commandLeave".Translate()))
            {
                if (selectedSquad != null)
                {
                    selectedSquad.order = MilitaryOrders.Leave;
                    Messages.Message("commandLeave".Translate(selectedSquad.outfit.name, selectedSquad.dead), MessageTypeDefOf.NeutralEvent);
                }
            }


            //Example command:

            //DebugTool tool = null;
            //IntVec3 Position;
            //tool = new DebugTool("Select Drop Position", delegate ()
            //{
            //   Position = UI.MouseCell();

            //    selectedSquad.order = MilitaryOrders.Standby;
            //    selectedSquad.orderLocation = Position;

            //    DebugTools.curTool = null;
            //});
            //DebugTools.curTool = tool;


            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
        }

        


    }

}
