using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FactionColonies
{
    public static class DebugActionsMisc
    {
        [DebugAction("Mods", "Display Empire patch notes", false, false, allowedGameStates = AllowedGameStates.Entry)]
        private static void PatchNotesDisplayWindow() => Find.WindowStack.Add(new PatchNotesDisplayWindow());
    }

    class PatchNotesDisplayWindow : Window
    {
        public override Vector2 InitialSize => new Vector2(1200f + (StandardMargin * 2), 595f + (StandardMargin * 2));

        private readonly Rect PatchNotesWindowTitleRect = new Rect(5f, 0f, 1190f, 30f);
        private readonly Rect HorizontalLineRect = new Rect(5f, 30f, 1190f, 15f);
        private readonly Rect PatchNotesScrollArea = new Rect(5f, 45f, 655f, 545f);
        private readonly Rect PatchNotesImageArea = new Rect(675f, 45f, 520f, 545f);
        private readonly Rect PatchNotesImageRect = new Rect(685f, 55f, 500f, 280f);
        private readonly Rect LastImageButtonRect = new Rect(685f, 55f, 50f, 280f);
        private readonly Rect NextImageButtonRect = new Rect(1135f, 55f, 50f, 280f);
        private readonly Rect ImageDescRect = new Rect(685f, 345f, 500f, 235f);
        private readonly Rect VerticalDeviderRect = new Rect(660f, 30f, 15f, 545f);

        public override void DoWindowContents(Rect inRect)
        {
            //COMPILED BY NESGUI
            //Prepare variables

            GameFont prevFont = Text.Font;
            TextAnchor textAnchor = Text.Anchor;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.Label(PatchNotesWindowTitleRect, "Empire Patch Notes");
            Widgets.DrawBox(PatchNotesWindowTitleRect);
            Widgets.DrawLineHorizontal(HorizontalLineRect.x, HorizontalLineRect.center.y, HorizontalLineRect.width);
            Widgets.DrawLineVertical(VerticalDeviderRect.center.x, HorizontalLineRect.center.y, VerticalDeviderRect.height + (45f - HorizontalLineRect.center.y));
            
            if (Widgets.ButtonImage(PatchNotesWindowTitleRect.RightPartPixels(PatchNotesWindowTitleRect.height).ContractedBy(6f), TexButton.CloseXSmall)) Close();
            Widgets.DrawBox(PatchNotesScrollArea);
            Widgets.DrawBox(PatchNotesImageArea);
            Widgets.DrawBox(PatchNotesImageRect);
            Widgets.DrawBox(LastImageButtonRect);
            Widgets.DrawBox(NextImageButtonRect);
            Widgets.DrawBox(ImageDescRect);

            Text.Font = prevFont;
            Text.Anchor = textAnchor;
            //END NESGUI CODE
        }
    }
}
