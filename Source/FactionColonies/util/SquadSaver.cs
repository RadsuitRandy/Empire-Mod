using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FactionColonies
{
    [StaticConstructorOnStartup]
    public static class FactionColoniesMilitary
    {
        private static List<SavedUnitFC> savedUnits = new List<SavedUnitFC>();
        private static List<SavedSquadFC> savedSquads = new List<SavedSquadFC>();

        public static string EmpireConfigFolderPath;
        public static string EmpireMilitaryUnitFolder;
        public static string EmpireMilitarySquadFolder;

        static FactionColoniesMilitary()
        {
            EmpireConfigFolderPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "Empire");
            EmpireMilitarySquadFolder = Path.Combine(EmpireConfigFolderPath, "Squads");
            EmpireMilitaryUnitFolder = Path.Combine(EmpireConfigFolderPath, "Units");
            if (!Directory.Exists(EmpireConfigFolderPath) ||
                !Directory.Exists(EmpireMilitarySquadFolder) ||
                !Directory.Exists(EmpireMilitaryUnitFolder))
            {
                Directory.CreateDirectory(EmpireConfigFolderPath);
                Directory.CreateDirectory(EmpireMilitarySquadFolder);
                Directory.CreateDirectory(EmpireMilitaryUnitFolder);
            }

            Read();
        }

        public static void RemoveSquad(SavedSquadFC squad)
        {
            savedSquads.Remove(squad);
            File.Delete(GetSquadPath(squad.name));
        }

        public static void RemoveUnit(SavedUnitFC unit)
        {
            savedUnits.Remove(unit);
            File.Delete(GetUnitPath(unit.name));
        }

        [DebugAction("Empire", "Reload Saved Military")]
        public static void Read()
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
                throw new Exception("Empire - Attempt to load saved military while scribe is active");

            foreach (string path in Directory.EnumerateFiles(EmpireMilitarySquadFolder))
            {
                try
                {
                    SavedSquadFC squad = new SavedSquadFC();
                    Scribe.loader.InitLoading(path);
                    squad.ExposeData();
                    savedSquads.Add(squad);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to load squad at path " + path);
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }
            }

            foreach (string path in Directory.EnumerateFiles(EmpireMilitaryUnitFolder))
            {
                try
                {
                    SavedUnitFC unit = new SavedUnitFC();
                    Scribe.loader.InitLoading(path);
                    unit.ExposeData();
                    savedUnits.Add(unit);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to load unit at path " + path);
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }
            }
        }

        public static string GetUnitPath(string name) => Path.Combine(EmpireMilitaryUnitFolder, name);
        public static string GetSquadPath(string name) => Path.Combine(EmpireMilitarySquadFolder, name);

        public static void SaveSquad(SavedSquadFC squad)
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
                throw new Exception("Empire - Attempt to save squad while scribe is active");

            string path = GetSquadPath(squad.name);
            try
            {
                Scribe.saver.InitSaving(path, "squad");
                int version = 0;
                Scribe_Values.Look(ref version, "version");
                squad.ExposeData();
            }
            catch (Exception e)
            {
                Log.Error("Failed to save squad " + squad.name + " " + e);
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }

        public static void SaveUnit(SavedUnitFC unit)
        {
            if (Scribe.mode != LoadSaveMode.Inactive)
                throw new Exception("Empire - Attempt to save unit while scribe is active");

            string path = GetUnitPath(unit.name);
            try
            {
                Scribe.saver.InitSaving(path, "unit");
                int version = 0;
                Scribe_Values.Look(ref version, "version");
                unit.ExposeData();
            }
            catch (Exception e)
            {
                Log.Error("Failed to save unit " + unit.name + " " + e);
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }
        }

        public static void SaveAllUnits() => savedUnits.ForEach(SaveUnit);
        public static void SaveAllSquads() => savedSquads.ForEach(SaveSquad);
    }

    public class SavedUnitFC : IExposable
    {
        public string name;
        public bool isTrader;
        public bool isCivilian;
        public PawnKindDef animal;
        public PawnKindDef pawnKind;
        public ThingDef weapon;
        public List<ThingDef> apparel;

        public SavedUnitFC() {}

        public SavedUnitFC(MilUnitFC unit)
        {
            Pawn pawn = unit.defaultPawn;
            name = unit.name;
            weapon = pawn.equipment.Primary.def;
            apparel = pawn.apparel.WornApparel.Select(a => a.def).ToList();
            isTrader = unit.isTrader;
            isCivilian = unit.isCivilian;
            animal = unit.animal;
            pawnKind = unit.pawnKind;
        }

        public MilUnitFC CreateMilUnit()
        {
            MilUnitFC unit = new MilUnitFC();
            unit.name = name;
            unit.isCivilian = isCivilian;
            unit.isTrader = isTrader;
            unit.animal = animal;
            unit.pawnKind = pawnKind;
            unit.generateDefaultPawn();

            unit.equipWeapon((ThingWithComps)ThingMaker.MakeThing(weapon));
            apparel.ForEach(a => unit.wearEquipment((Apparel)ThingMaker.MakeThing(a), true));

            unit.changeTick();
            unit.updateEquipmentTotalCost();

            return unit;
        }

        public bool IsValid => !(animal == null || pawnKind == null || weapon == null || apparel.Any(null));
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref isTrader, "isTrader");
            Scribe_Values.Look(ref isCivilian, "isCivilian");
            Scribe_Defs.Look(ref animal, "animal");
            Scribe_Defs.Look(ref pawnKind, "pawnKind");
            Scribe_Defs.Look(ref weapon, "weapon");
            Scribe_Collections.Look(ref apparel, "apparel", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.LoadingVars && !this.IsValid)
            {
                string message = $"Failed to load unit {name}. You are probably missing a mod for this unit.";
                Log.Message(message);
            }
        }
    }

    public class SavedSquadFC : IExposable
    {
        public string name;
        public List<SavedUnitFC> unitTemplates = new List<SavedUnitFC>();
        public int[] units = new int[30];
        public bool isTraderCaravan;
        public bool isCivilian;
        public SavedSquadFC() {}

        public SavedSquadFC(MilSquadFC squad)
        {
            name = squad.name;
            isTraderCaravan = squad.isTraderCaravan;
            isCivilian = squad.isCivilian;
            var squadTemplates = squad.units.Distinct().ToList();
            unitTemplates = squadTemplates.Select(unit => new SavedUnitFC(unit)).ToList();

            for(int template = 0; template < squadTemplates.Count(); template++)
            {
                for (int i = 0; i < 30; i++)
                {
                    if (squad.units[i] == squadTemplates[template])
                    {
                        units[i] = template;
                    }
                }
            }
        }

        public MilSquadFC CreateMilSquad()
        {
            MilSquadFC squad = new MilSquadFC(true);
            squad.name = name;
            squad.isCivilian = isCivilian;
            squad.isTraderCaravan = isTraderCaravan;
            squad.units = new List<MilUnitFC>();
            squad.units.Capacity = 30;
            foreach (int i in units)
            {
                squad.units.Add(unitTemplates[i].CreateMilUnit());
            }

            return squad;
        }

        public bool IsValid => unitTemplates.All(u => u.IsValid);
        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref isCivilian, "isCivilian");
            Scribe_Values.Look(ref isTraderCaravan, "isTraderCaravan");
            Scribe_Collections.Look(ref unitTemplates, "unitTemplates", LookMode.Deep);
            Scribe_Values.Look(ref units, "units");
            if(Scribe.mode == LoadSaveMode.LoadingVars && !this.IsValid);
            {
                string message = $"Failed to load squad {name}. You are probably missing a mod for this squad.";
                Log.Message(message);
            }
        }
    }
}
