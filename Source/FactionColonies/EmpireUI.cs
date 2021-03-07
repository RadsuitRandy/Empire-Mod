using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.layer = WindowLayer.Super;
            this.closeOnClickedOutside = false;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.doCloseX = false;
            this.draggable = true;
            this.drawShadow = false;
            this.doWindowBackground = false;
            this.preventCameraMotion = false;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(200f, 200f);
            }
        }


        protected override void SetInitialSizeAndPosition()
        {
            this.windowRect = new Rect((float)UI.screenWidth - this.InitialSize.x, 0f, this.InitialSize.x, this.InitialSize.y);
        }

        public override void DoWindowContents(Rect rect) 
        {
            FactionFC faction = Find.World.GetComponent<FactionFC>();
            if(faction.militaryCustomizationUtil.DeployedSquads.Count() == 0)
            {
                this.Close();
            }

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            Rect selectSquad = new Rect(0, 0, 160, 40);
            Rect commandAttack = new Rect(0, 40, 160, 40);
            Rect commandMove = new Rect(0, 80, 160, 40);
            Rect commandHeal = new Rect(0, 120, 160, 40);

            
            if (selectedSquad == null)
            {
                squadText = "Select Deployed Squad";
            } else
            {
                squadText = selectedSquad.getSettlement.name + "'s " + selectedSquad.outfit.name;
            }

            //Select a squad
            if(Widgets.ButtonText(selectSquad, squadText))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (MercenarySquadFC squad in faction.militaryCustomizationUtil.DeployedSquads)
                {
                    if (squad.getSettlement != null)
                    {
                        list.Add(new FloatMenuOption(squad.getSettlement.name + "'s " + squad.outfit.name, delegate
                        {
                            selectedSquad = squad;
                        }));
                    }
                }
                if (list.Count() == 0)
                {
                    list.Add(new FloatMenuOption("No squads available", null));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }



            if(Widgets.ButtonTextSubtle(commandAttack, "Attack"))
            {
                if (selectedSquad != null)
                {
                    selectedSquad.order = MilitaryOrders.Attack;
                    Messages.Message(selectedSquad.outfit.name + " are now charging the enemy.", MessageTypeDefOf.NeutralEvent);
                    //selectedSquad.orderLocation = Position;
                }
            }
            if (Widgets.ButtonTextSubtle(commandMove, "Move"))
            {
                if (selectedSquad != null)
                {
                    DebugTool tool = null;
                    IntVec3 Position;
                    tool = new DebugTool("Select Move Position", delegate ()
                    {
                        Position = UI.MouseCell();

                        selectedSquad.order = MilitaryOrders.Standby;
                        selectedSquad.orderLocation = Position;
                        Messages.Message(selectedSquad.outfit.name + " are moving to the position and standing by.", MessageTypeDefOf.NeutralEvent);

                        DebugTools.curTool = null;
                    });
                    DebugTools.curTool = tool;
                }
            }
            if (Widgets.ButtonTextSubtle(commandHeal, "Leave"))
            {
                if (selectedSquad != null)
                {
                    Messages.Message(selectedSquad.outfit.name + " are now leaving the map.", MessageTypeDefOf.NeutralEvent);
                    selectedSquad.order = MilitaryOrders.Leave;
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
