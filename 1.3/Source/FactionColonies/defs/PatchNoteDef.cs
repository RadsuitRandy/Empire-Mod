using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactionColonies.util;
using RimWorld;
using UnityEngine;
using Verse;

namespace FactionColonies
{
    class PatchNoteDef : Def
    {
        private readonly int major = 0;
        private readonly int minor = 0;
        private readonly int patch = 0;
        private readonly PatchNoteType patchNoteType = PatchNoteType.Undefined;
        private readonly List<string> patchNoteLines = new List<string>();
        private readonly List<string> additionalNotes = new List<string>();

        [NoTranslate]
        public readonly string ModId = "saakra.empire";

        [NoTranslate]
        private readonly string link = "";

        [NoTranslate]
        private readonly string linkButtonImagePath = "";

        [NoTranslate]
        private readonly List<string> authors = new List<string>();

        [NoTranslate]
        private readonly List<string> patchNoteImagePaths = new List<string>();

        [NoTranslate]
        private readonly List<string> patchNoteImageDescriptions = new List<string>();

        [NoTranslate]
        private readonly string authorStringTranslationKey = "FCPatchNotesAuthorString";

        [NoTranslate]
        private readonly string introStringTranslationKey = "FCUpdateIntroString";

        [NoTranslate]
        private readonly string modNameTranslationKey = "FCPatchNotesModName";

        private readonly int releaseDay = 01;
        private readonly int releaseMonth = 01;
        private readonly int releaseYear = 2000;

        private bool hasImagesCached = false;
        private readonly List<Texture2D> imagesCached = new List<Texture2D>();
        private ModContentPack modContentPackCached = null;
        private Texture2D linkButtonImageCached;

        /// <summary>
        /// The title of the update example: [Empire] Update 0.38.00
        /// </summary>
        public string Title => $"[{(modContentPackCached ?? (modContentPackCached = LoadedModManager.RunningModsListForReading.FirstOrFallback(pack => pack.PackageId == ModId))).ModMetaData.Name}] {label} {VersionNumber}";

        /// <summary>
        /// The def description
        /// </summary>
        public string Description => description;

        /// <summary>
        /// The complete Version number formatted like: "1.02.03"
        /// </summary>
        public string VersionNumber => $"{major}.{ToVersion(minor)}.{ToVersion(patch)}";

        /// <summary>
        /// Converts the version to the old Empire version format
        /// </summary>
        public double ToOldEmpireVersion => double.Parse($"{major}.{ToVersion(minor)}{ToVersion(patch)}", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// The Major version number
        /// </summary>
        public int Major => major;

        /// <summary>
        /// The Minor version number
        /// </summary>
        public int Minor => minor;

        /// <summary>
        /// The Patch version number
        /// </summary>
        public int Patch => patch;

        /// <summary>
        /// Returns the PatchNoteType
        /// </summary>
        public PatchNoteType GetPatchNoteType => patchNoteType;

        /// <summary>
        /// Returns a link as provided by the def
        /// </summary>
        public string Link => link;

        /// <summary>
        /// Returns the patch notes seperated by new lines
        /// </summary>
        public string PatchNotesFormatted => string.Join("\n", patchNoteLines);

        /// <summary>
        /// Returns additional notes as provided by the def
        /// </summary>
        public string AdditionalNotesFormatted => string.Join("\n", additionalNotes);

        /// <summary>
        /// Returns the opening sentence of the patch notes
        /// </summary>
        public string PatchNotesIntroString => introStringTranslationKey.Translate(modNameTranslationKey.Translate(), VersionNumber);

        /// <summary>
        /// Returns the list of authors in this format: "name0, name1, name2, ..., nameN-1 and nameN" where N is the amount of authors
        /// only returns the name of one author if there is only one
        /// </summary>
        public string AuthorsFormatted
        {
            get
            {
                List<string> workList = authors.ListFullCopy();
                string lastAuthor = workList.Pop();

                if (workList.NullOrEmpty()) return lastAuthor;

                return $"{string.Join(", ", workList)} and {lastAuthor}";
            }
        }

        /// <summary>
        /// Returns a string that contains the authors in a sentence
        /// </summary>
        public string AuthorLine => authorStringTranslationKey.Translate(AuthorsFormatted);

        /// <summary>
        /// Returns the cached image, caches it if not yet cached
        /// </summary>
        public Texture2D LinkButtonImage => linkButtonImageCached ?? (linkButtonImageCached = ContentFinder<Texture2D>.Get(linkButtonImagePath));

        /// <summary>
        /// Returns the cached patchnote images, caches them if not yet cached
        /// </summary>
        public List<Texture2D> PatchNoteImages
        {
            get
            {
                if (!hasImagesCached)
                {
                    foreach (string path in patchNoteImagePaths)
                    {
                        imagesCached.Add(ContentFinder<Texture2D>.Get(path));
                    }

                    hasImagesCached = true;
                }

                return imagesCached;
            }
        }

        public List<string> PatchNoteImageDescriptions => patchNoteImageDescriptions;

        public DateTime ReleaseDate => new DateTime(releaseYear, releaseMonth, releaseDay);

        public string CompletePatchNotesString => $"{description}\n\n{PatchNotesIntroString}\n{PatchNotesFormatted}\n\n{AuthorLine}{(additionalNotes.NullOrEmpty() ? "" : "\n" + AdditionalNotesFormatted)}";

        /// <summary>
        /// Clears the cached data of this def
        /// </summary>
        public override void ClearCachedData()
        {
            base.ClearCachedData();

            imagesCached.RemoveAll((_) => true);
            linkButtonImageCached = null;
            modContentPackCached = null;
            hasImagesCached = false;
        }

        private static string ToVersion(int num) => (num > 10) ? num.ToString() : '0' + num.ToString();

        [DefOf]
        public class PatchNoteDefOf
        {
            static PatchNoteDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(PatchNoteDefOf));
        }
    }
}
