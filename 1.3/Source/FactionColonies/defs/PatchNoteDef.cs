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
        public int major = 0;
        public int minor = 0;
        public int patch = 0;
        public PatchNoteType patchNoteType = PatchNoteType.Undefined;

        public List<string> patchNoteLines = new List<string>();

        [NoTranslate]
        public List<string> authors = new List<string>();

        [NoTranslate]
        public List<string> patchNoteImagePaths = new List<string>();

        [NoTranslate]
        public List<string> patchNoteImageDescriptions = new List<string>();

        public string authorString = "FCPatchNotesAuthorString";
        private bool hasImagesCached = false;
        private readonly List<Texture2D> imagesCached = new List<Texture2D>();

        public string Title
        {
            get => label;
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public string VersionNumber => $"{major}.{minor}.{patch}";

        public int Major
        {
            get => major;
        }

        public int Minor
        {
            get => minor;
        }

        public int Patch
        {
            get => patch;
        }

        public string PatchNotesFormatted => string.Join("\n", patchNoteLines);

        public string AuthorsFormatted
        {
            get
            {
                List<string> workList = authors.ListFullCopy();
                string lastAuthor = workList.Pop();
                return $"{string.Join(", ", workList)} and {lastAuthor}.";
            }
        }

        public List<Texture2D> PatchNoteImages()
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

        [DefOf]
        public class PatchNoteDefOf
        {
            static PatchNoteDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(PatchNoteDefOf));
        }
    }
}
