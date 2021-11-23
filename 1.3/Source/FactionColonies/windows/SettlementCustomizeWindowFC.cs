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
		private readonly int length = 400;
		private readonly int xoffset = 0;
		private readonly int yoffset = 50;
		private readonly int yspacing = 30;
		private readonly int height = 200;

		private readonly SettlementFC settlement;
		public string header;
		private string name;
		private string nameShort;
		public override Vector2 InitialSize => new Vector2(445f, 280f);

		public SettlementCustomizeWindowFc(SettlementFC settlement)
		{
			forcePause = false;
			draggable = true;
			doCloseX = true;
			preventCameraMotion = false;
			this.settlement = settlement;
			header = "CustomizeSettlement".Translate();
			name = settlement.name;
			nameShort = settlement.NameShort;

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
			settlement.name = name;
			settlement.NameShort = nameShort;

		public override void DoWindowContents(Rect inRect)
		{
			Rect fullNameLabelRect = new Rect(xoffset + 3, yoffset + yspacing * 1, length / 4, yspacing);
			Rect shortNameLabelRect = new Rect(xoffset + 3, yoffset + yspacing * 2, length / 4, yspacing);
			Rect fullNameInputRect = new Rect(xoffset + 3 + length / 4 + 5, yoffset + yspacing * 1, length / 2, yspacing);
			Rect shortNameInputRect = new Rect(xoffset + 3 + length / 4 + 5, yoffset + yspacing * 2, length / 2, yspacing);
			Rect confirmChangesRect = new Rect((InitialSize.x - 120 - 18) / 2, yoffset + InitialSize.y - 120, 120, 30);

			//grab before anchor/font
			GameFont fontBefore = Text.Font;
			TextAnchor anchorBefore = Text.Anchor;


			//Settlement Tax Collection Header
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Medium;

			Widgets.Label(new Rect(3, 3, 300, 60), header);

			Text.Font = GameFont.Small;

			Widgets.Label(fullNameLabelRect, "FCSettlementFullName".Translate());
			name = Widgets.TextField(fullNameInputRect, name);

			Widgets.Label(shortNameLabelRect, "FCSettlementShortName".Translate());
			if (Widgets.ButtonText(confirmChangesRect, "ConfirmChanges".Translate()))
			{
				settlement.name = name;
				settlement.NameShort = nameShort;

			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;

			Widgets.DrawBox(new Rect(xoffset, yoffset - yspacing, length, height - yspacing * 2));

			//reset anchor/font
			Text.Font = fontBefore;
			Text.Anchor = anchorBefore;
		}
	}
}
