using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;


namespace FactionColonies
{
	public class settlementUpgradeWindowFC : Window
	{

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(380f, 300f);
			}
		}

		//declare variables

		//private int xspacing = 60;
		private int yspacing = 30;
		private int yoffset = 90;
		//private int headerSpacing = 30;
		private int length = 335;
		private int xoffset = 0;
		private int height = 200;
		private int settlementUpgradeCost;

		private SettlementFC settlement;
		private FactionFC factionfc;




		public string desc;
		public string header;

		public settlementUpgradeWindowFC(SettlementFC settlement)
		{
			this.forcePause = false;
			this.draggable = true;
			this.doCloseX = true;
			this.preventCameraMotion = false;
			this.header = "UpgradeSettlement".Translate();
			this.settlement = settlement;
			this.settlementUpgradeCost = Convert.ToInt32(LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().settlementBaseUpgradeCost) + (settlement.settlementLevel * 1000);
			this.desc = settlement.name + " " + "CanBeUpgraded".Translate() + " " + this.settlementUpgradeCost + " " + "Silver".Translate().ToLower() + ". " + "UpgradeColonyDesc".Translate();
			this.factionfc = Find.World.GetComponent<FactionFC>();
		}

		public override void PreOpen()
		{
			base.PreOpen();
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
		}

		public override void DoWindowContents(Rect inRect)
		{





			//grab before anchor/font
			GameFont fontBefore = Text.Font;
			TextAnchor anchorBefore = Text.Anchor;



			//Settlement Tax Collection Header
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(2, 0, 300, 60), header);




			//settlement buttons

			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Tiny;

			//0 tithe total string
			//1 source - -1
			//2 due/delivery date
			//3 Silver (- || +)
			//4 tithe

			if (settlement.settlementLevel < LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().settlementMaxLevel) //if settlement is not max level
			{
				if(Widgets.ButtonText(new Rect(xoffset+((335-150)/2), height + 10, 150, 40), "UpgradeSettlement".Translate() + ": " + settlementUpgradeCost))
				{ //if upgrade button clicked
					//if max level
					if(settlement.settlementLevel < LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().settlementMaxLevel) //if below max level
					{
						if(PaymentUtil.getSilver() > settlementUpgradeCost) //if have enough monies to pay
						{
							foreach (FCEvent evt in Find.World.GetComponent<FactionFC>().events)
							{
								if (evt.def == FCEventDefOf.upgradeSettlement && evt.location == settlement.mapLocation)
								{ //if already existing event
									Messages.Message("AlreadyUpgradeSettlement".Translate(), MessageTypeDefOf.RejectInput);
									return;
								}
							}
							PaymentUtil.paySilver(settlementUpgradeCost);
							//settlement.upgradeSettlement();
							FCEvent tmp = new FCEvent(true);
							tmp.def = FCEventDefOf.upgradeSettlement;
							tmp.location = settlement.mapLocation;
							tmp.planetName = settlement.planetName;
							int triggerTime = (settlement.settlementLevel + 1) * 60000 * 2;
							if (factionfc.hasPolicy(FCPolicyDefOf.isolationist))
								triggerTime /= 2;
							tmp.timeTillTrigger = Find.TickManager.TicksGame + triggerTime;
							//Log.Message(list[i].enactDuration.ToString());
							//Log.Message(tmp.timeTillTrigger.ToString());
							Find.World.GetComponent<FactionFC>().addEvent(tmp);
							Find.WindowStack.TryRemove(this);
							Find.WindowStack.WindowOfType<settlementWindowFC>().WindowUpdateFC();
							Messages.Message("StartUpgradeSettlement".Translate(), MessageTypeDefOf.NeutralEvent);
							
						} else
						{ //if don't have enough monies
							Messages.Message("NotEnoughSilverUpgrade".Translate(), MessageTypeDefOf.RejectInput);
						}
					} else
					{
						Messages.Message(settlement.name + " " + "AlreadyMaxLevel".Translate() + "!", MessageTypeDefOf.RejectInput);
					}
				}
			} else //if settlement is max level
			{
				desc = "CannotBeUpgradedPastMax".Translate() + ": " + LoadedModManager.GetMod<FactionColoniesMod>().GetSettings<FactionColonies>().settlementMaxLevel;
			}

			Widgets.Label(new Rect(xoffset + 2, yoffset - yspacing + 2, length - 4, height - 4 + yspacing * 2), desc);
			Widgets.DrawBox(new Rect(xoffset, yoffset - yspacing, length, height - yspacing * 2));

			

		//reset anchor/font
		Text.Font = fontBefore;
			Text.Anchor = anchorBefore;
		}
	}
}
