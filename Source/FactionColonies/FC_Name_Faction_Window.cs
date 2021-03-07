using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace FactionColonies
{
    public class FC_Dialogue_Request : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400f, 400f);
            }
        }

        public FC_Dialogue_Request()
        {
            //init variables here I guess
            this.forcePause = true;

            this.preventCameraMotion = true;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.closeOnClickedOutside = false;
            
        }

        public FC_Dialogue_Request(string label)
        {
            this.label = label;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //set text anchor and font
            GameFont fontBefore = Text.Font;
            TextAnchor anchorBefore = Text.Anchor;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;


            Widgets.Label(new Rect(0,0,250,25), label);
           text = Widgets.TextField(new Rect(0,35,150,25), text);
            

            //Reset Text anchor and font
            Text.Font = fontBefore;
            Text.Anchor = anchorBefore;
        }

        public string text;
        public string label;

    }
}
