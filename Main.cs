﻿using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ATouchOfMagic
{
    internal class Main
    {
     internal class Settings
        {
            internal bool bestMentalStat { get; }

            internal Settings()
            {

                using (StreamReader settings_file = File.OpenText("Mods/ATouchOfMagic/settings.json"))
                using (JsonTextReader reader = new JsonTextReader(settings_file))
                {
                    JObject jo = (JObject)JToken.ReadFrom(reader);
                    bestMentalStat = (bool)jo["bestMentalStat"];
                }
            }
        }

        static internal Settings settings = new Settings();
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        internal static LibraryScriptableObject library;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        internal static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        internal static bool enabled;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                var harmony = Harmony12.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                DebugError(ex);
                throw ex;
            }
            return true;
        }
        [Harmony12.HarmonyAfter("CallOfTheWild")]
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [Harmony12.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix(LibraryScriptableObject __instance)
            {
                var self = __instance;
                if (Main.library != null) return;
                Main.library = self;
                try
                {
                    Main.DebugLog("Loading A Touch of Magic");

                    CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = @"./Mods/ATouchOfMagic/Icons/";
#if DEBUG
                    bool allow_guid_generation = true;
#else
                    bool allow_guid_generation = false; //no guids should be ever generated in release
#endif
                    CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints, allow_guid_generation);

                
                    ArcaneArcherClass.CreateArcaneArcherClass();
                    DeadeyeDevoteeClass.CreateDeadeyeDevoteeClass();


#if DEBUG
                    string guid_file_name = @"D:\SteamLibrary\SteamApps\common\Pathfinder Kingmaker\ATouchOfMagic\blueprints.txt";
                    CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    CallOfTheWild.Helpers.GuidStorage.dump(@"./Mods/ATouchOfMagic/loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        internal static Exception Error(String message)
        {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }
    }
}
