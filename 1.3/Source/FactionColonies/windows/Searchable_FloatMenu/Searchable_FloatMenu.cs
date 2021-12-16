using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FactionColonies
{
	class Searchable_FloatMenu : FloatMenu
	{
		private string searchTerm = "";

		private Color baseColor = Color.white;
		private static readonly Vector2 SearchBarOffset = new Vector2(25f, -27f);
		private readonly char ignoreBeforeChar;
		private readonly bool useIgnoreBeforeChar;
		private static bool shouldCloseOnSelect = false;

		private readonly List<FloatMenuOption> filteredOptions;

		public bool ShouldCloseOnSelect => shouldCloseOnSelect;

		private float ColumnWidth
		{
			get
			{
				float num = 70f;
				for (int i = 0; i < options.Count; i++)
				{
					float requiredWidth = options[i].RequiredWidth;
					if (requiredWidth >= 300f)
					{
						return 300f;
					}
					if (requiredWidth > num)
					{
						num = requiredWidth;
					}
				}
				return Mathf.Round(num);
			}
		}

		/// <summary>
		///		Like a normal <c>FloatMenu</c>, but searchable!
		/// </summary>
		/// <param name="options"></param>
		/// <param name="ignoreBeforeChar"></param>
		/// <param name="useIgnoreBeforeChar"></param>
		public Searchable_FloatMenu(List<FloatMenuOption> options, bool useIgnoreBeforeChar = true, char ignoreBeforeChar = '-') : base(AddStayOpenOption(options))
		{
			this.useIgnoreBeforeChar = useIgnoreBeforeChar;
			this.ignoreBeforeChar = ignoreBeforeChar;

			options = AddStayOpenOption(options);
			filteredOptions = options.ListFullCopy();

			options.Clear();
		}

		private static List<FloatMenuOption> AddStayOpenOption(List<FloatMenuOption> options)
        {
			List<FloatMenuOption> returnOptions = new List<FloatMenuOption> { new FloatMenuOption("FCFMSStayOpen".Translate(), () => shouldCloseOnSelect = !shouldCloseOnSelect, shouldCloseOnSelect ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, Color.white) };
			returnOptions.AddRange(options);
			return returnOptions;
        }

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Vector2 vector = new Vector2(windowRect.x, windowRect.y);
			Text.Font = GameFont.Small;
			Rect searchRect = new Rect(vector.x - SearchBarOffset.x, vector.y + SearchBarOffset.y, ColumnWidth + SearchBarOffset.x, 27f);

			Find.WindowStack.ImmediateWindow(6830963, searchRect, WindowLayer.Super, delegate
			{
				GUI.color = baseColor;
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;

				//Rect position = searchRect.AtZero();
				Rect bgRect = searchRect.AtZero();

				bgRect.width -= SearchBarOffset.x;
				bgRect.x += SearchBarOffset.x;

				Rect searchBarRect = bgRect.ContractedBy(2f);
				Rect labelRect = bgRect.ContractedBy(2f);

				//position.width = 150f;
				//GUI.DrawTexture(position, TexUI.TextBGBlack);

				Widgets.DrawWindowBackground(bgRect);

				searchTerm = Widgets.TextField(searchBarRect, searchTerm);

				options.Clear();
				if (searchTerm != "")
				{
					vanishIfMouseDistant = false;
					if (useIgnoreBeforeChar)
					{
						options.AddRange(filteredOptions.Where(option => option.Label.ToLower().Substring(0, (option.Label.IndexOf(ignoreBeforeChar) < 0) ? option.Label.Length : option.Label.IndexOf(ignoreBeforeChar) - 1).Contains(searchTerm.ToLower())));
					}
                    else
                    {
						options.AddRange(filteredOptions.Where(option => option.Label.ToLower().Contains(searchTerm.ToLower())));
					}
				}
				else
				{
					Widgets.Label(labelRect, $" {"FloatMenuSearchable".Translate()}");
					options = filteredOptions.ToList();
					vanishIfMouseDistant = true;
				}

				Text.Anchor = TextAnchor.UpperLeft;
			}, false, false, 0f);
		}

        public override void Close(bool doCloseSound = true)
        {
			if (shouldCloseOnSelect) base.Close(doCloseSound);
        }
    }
}
