using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactionColonies.util;
using UnityEngine;
using Verse;

namespace FactionColonies
{
	public static class DebugActionsMisc
	{
		[DebugAction("Mods", "Display Empire patch notes", false, false, allowedGameStates = AllowedGameStates.Entry)]
		public static void PatchNotesDisplayWindow() => Find.WindowStack.Add(new PatchNotesDisplayWindow());
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

		private readonly string title = "FCPatchNotesWindowTitle".Translate();
		private readonly GameFont prevFont;
		private readonly TextAnchor prevAnchor;
		private readonly Color prevColor;
		private readonly Rect patchNotesScrollViewRect;
		private readonly List<PatchNoteDef> patchNoteDefs;

		private Vector2 scrollPos = new Vector2();

		/// <summary>
		/// Constructs a PatchNotesDisplayWindow class and saves the current Text.Font, Text.Anchor and GUI.color
		/// </summary>
		public PatchNotesDisplayWindow()
		{
			prevFont = Text.Font;
			prevAnchor = Text.Anchor;
			prevColor = GUI.color;
			patchNotesScrollViewRect = PatchNotesScrollArea.LeftPartPixels(PatchNotesImageRect.width - 17f);
			patchNoteDefs = DefDatabase<PatchNoteDef>.AllDefsListForReading;
		}

		/// <summary>
		/// Constructs a PatchNotesDisplayWindow class and saves the current Text.Font, Text.Anchor and GUI.color and gives it the given <paramref name="title"/>
		/// </summary>
		/// <param name="title"></param>
		public PatchNotesDisplayWindow(string title) : this() => this.title = title;

		/// <summary>
		/// This function draws the contents of this window class. The Rects were laid out using NesGui
		/// </summary>
		/// <param name="inRect"></param>
		public override void DoWindowContents(Rect inRect)
		{
			DrawTitle();
			DrawDividers();
			DrawPatchNotes();

			Widgets.DrawBox(PatchNotesScrollArea);
			Widgets.DrawBox(PatchNotesImageArea);
			Widgets.DrawBox(PatchNotesImageRect);
			Widgets.DrawBox(LastImageButtonRect);
			Widgets.DrawBox(NextImageButtonRect);
			Widgets.DrawBox(ImageDescRect);

			ResetTextAndColor();
		}

		private void DrawPatchNotes()
		{
			Widgets.BeginScrollView(PatchNotesScrollArea, ref scrollPos, patchNotesScrollViewRect);

			

			Widgets.EndScrollView();
		}

		/// <summary>
		/// Draws the lines that separate the title, patch notes and patch note images
		/// </summary>
		private void DrawDividers()
		{
			GUI.color = Color.gray;
			Widgets.DrawLineHorizontal(HorizontalLineRect.x, HorizontalLineRect.center.y - 1f, HorizontalLineRect.width);
			Widgets.DrawLineVertical(VerticalDeviderRect.center.x, HorizontalLineRect.center.y - 1f, VerticalDeviderRect.height + 45f - HorizontalLineRect.center.y);

			ResetTextAndColor();
		}

		/// <summary>
		/// Draws this windows title and the close button
		/// </summary>
		private void DrawTitle()
		{
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperLeft;

			Widgets.Label(PatchNotesWindowTitleRect, title);
			if (Widgets.ButtonImage(PatchNotesWindowTitleRect.RightPartPixels(PatchNotesWindowTitleRect.height).ContractedBy(6f), TexButton.CloseXSmall)) Close();

			ResetTextAndColor();
		}

		/// <summary>
		/// Resets the Text.Font, Text.Anchor and GUI.color setting
		/// </summary>
		private void ResetTextAndColor()
		{
			Text.Font = prevFont;
			Text.Anchor = prevAnchor;
			GUI.color = prevColor;
		}
	}
}
