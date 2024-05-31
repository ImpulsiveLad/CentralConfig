using LethalLevelLoader;
using System.Collections.Generic;
using System.Linq;
using CSync.Extensions;
using CSync.Lib;
using BepInEx.Configuration;
using System.Runtime.Serialization;
using HarmonyLib;
using LethalLevelLoader.Tools;
using UnityEngine;
using System;
using DunGen;
using System.Collections;

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class WaitForDungeonsToRegister
    {
        public static CreateDungeonConfig Config;

        [DataContract]
        public class CreateDungeonConfig : ConfigTemplate
        {
            // Declare config entries tied to the dictionary

            [DataMember] public static Dictionary<string, SyncedEntry<float>> MinDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> MaxDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> DungeonSizeScaler;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonPlanetNameList;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonRoutePriceList;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonModNameList;

            public CreateDungeonConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                // Intialize config entries tied to the dictionary

                MinDungeonSize = new Dictionary<string, SyncedEntry<float>>();
                MaxDungeonSize = new Dictionary<string, SyncedEntry<float>>();
                DungeonSizeScaler = new Dictionary<string, SyncedEntry<float>>();

                DungeonPlanetNameList = new Dictionary<string, SyncedEntry<string>>();
                DungeonRoutePriceList = new Dictionary<string, SyncedEntry<string>>();
                DungeonModNameList = new Dictionary<string, SyncedEntry<string>>();

                List<ExtendedDungeonFlow> AllExtendedDungeons;
                string ignoreList = CentralConfig.SyncConfig.BlackListDungeons.Value;

                if (CentralConfig.SyncConfig.IsWhiteList)
                {
                    AllExtendedDungeons = PatchedContent.ExtendedDungeonFlows.Where(dungeon => ignoreList.Split(',').Any(b => dungeon.DungeonName.Equals(b))).ToList();
                }
                else
                {
                    AllExtendedDungeons = PatchedContent.ExtendedDungeonFlows.Where(dungeon => !ignoreList.Split(',').Any(b => dungeon.DungeonName.Equals(b))).ToList();
                }
                foreach (ExtendedDungeonFlow dungeon in AllExtendedDungeons)
                {
                    string Dun = dungeon.DungeonName + " (" + dungeon.name + ")";
                    Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
                    string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");

                    // Size

                    if (CentralConfig.SyncConfig.DoDunSizeOverrides)
                    {
                        MinDungeonSize[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName, // Assigns the config with the dictionary so that it is unique to the level/moon/planet
                            DungeonName + " - Minimum Size Multiplier",
                            dungeon.DynamicDungeonSizeMinMax.x,
                            "Sets the min size multiplier this dungeon can have.");

                        MaxDungeonSize[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Maximum Size Multiplier",
                            dungeon.DynamicDungeonSizeMinMax.y,
                            "Sets the max size multiplier this dungeon can have.");

                        DungeonSizeScaler[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Dungeon Size Scaler",
                            dungeon.DynamicDungeonSizeLerpRate * 100,
                            "This setting controls the strictness of the clamp. At 0%, the clamp is inactive. At 100%, the clamp is fully enforced, pulling any out-of-bounds values back to the nearest boundary. For percentages in between, out-of-bounds values are partially pulled back towards the nearest boundary. For example given a value of 50%, a value exceeding the max would be adjusted halfway back to the max.");
                    }

                    // Injection

                    if (CentralConfig.SyncConfig.DoDungeonSelectionOverrides)
                    {
                        string DungeonPlanetList = ConfigAider.ConvertStringWithRarityToString(dungeon.LevelMatchingProperties.planetNames);

                        DungeonPlanetNameList[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Dungeon by Planet Name",
                            DungeonPlanetList,
                            "The dungeon will be added to any moons listed here. This must be the exact numberless name. \"Experimentatio\" =/= \"Experimentation\"");

                        string DungeonRouteList = ConfigAider.ConvertVector2WithRaritiesToString(dungeon.LevelMatchingProperties.currentRoutePrice);

                        DungeonRoutePriceList[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Dungeon by Route Price",
                            DungeonRouteList,
                            "This dungeon will be added to all moons that have a route price between the first two values with a rarity of the final value.");

                        string DungeonModList = ConfigAider.ConvertStringWithRarityToString(dungeon.LevelMatchingProperties.modNames);

                        DungeonModNameList[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Dungeon By Mod Name",
                            DungeonModList,
                            "The dungeon will be added to all moons of any mod listed here. This doesn't have to be an exact match, \"Rosie\" works for \"Rosie's Moons\".");
                    }
                }
                CentralConfig.instance.mls.LogInfo("Dungeon config has been registered.");
            }
        }
        static void Prefix()
        {
            CentralConfig.ConfigFile2 = new CreateDungeonConfig(CentralConfig.instance.Config); // Dungeon config is created when you join a lobby (So every other config is already applied)
        }
    }
    public class ApplyDungeonConfig
    {
        public static bool Ready = false;
        public void UpdateDungeonValues() // This is called on as a postfix on the same method as creating the config stuff so it gets applied here right after the config is intialized
        {
            List<ExtendedDungeonFlow> AllExtendedDungeons;
            string ignoreList = CentralConfig.SyncConfig.BlackListDungeons.Value;

            if (CentralConfig.SyncConfig.IsWhiteList)
            {
                AllExtendedDungeons = PatchedContent.ExtendedDungeonFlows.Where(dungeon => ignoreList.Split(',').Any(b => dungeon.DungeonName.Equals(b))).ToList();
            }
            else
            {
                AllExtendedDungeons = PatchedContent.ExtendedDungeonFlows.Where(dungeon => !ignoreList.Split(',').Any(b => dungeon.DungeonName.Equals(b))).ToList();
            }
            foreach (ExtendedDungeonFlow dungeon in AllExtendedDungeons)
            {

                string Dun = dungeon.DungeonName + " (" + dungeon.name + ")";
                Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
                string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");


                // Size

                if (CentralConfig.SyncConfig.DoDunSizeOverrides)
                {
                    Vector2 newSize = dungeon.DynamicDungeonSizeMinMax;
                    newSize.x = WaitForDungeonsToRegister.CreateDungeonConfig.MinDungeonSize[DungeonName];
                    newSize.y = WaitForDungeonsToRegister.CreateDungeonConfig.MaxDungeonSize[DungeonName];
                    dungeon.DynamicDungeonSizeMinMax = newSize;

                    dungeon.DynamicDungeonSizeLerpRate = WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler[DungeonName];
                }

                // Injection

                if (CentralConfig.SyncConfig.DoDungeonSelectionOverrides)
                {
                    // PlanetName
                    string PlanetNameStr = WaitForDungeonsToRegister.CreateDungeonConfig.DungeonPlanetNameList[DungeonName];
                    Vector2 planetNameRarity = new Vector2(0, 99999);
                    List<StringWithRarity> InjectionPlanets = ConfigAider.ConvertPlanetNameStringToStringWithRarityList(PlanetNameStr, planetNameRarity);
                    if (InjectionPlanets.Count > 0)
                    {
                        dungeon.LevelMatchingProperties.planetNames = InjectionPlanets;
                    }

                    // ModName
                    string ModNameStr = WaitForDungeonsToRegister.CreateDungeonConfig.DungeonModNameList[DungeonName];
                    Vector2 modNameRarity = new Vector2(0, 99999);
                    List<StringWithRarity> InjectionMods = ConfigAider.ConvertModStringToStringWithRarityList(ModNameStr, modNameRarity);
                    if (InjectionMods.Count > 0)
                    {
                        dungeon.LevelMatchingProperties.modNames = InjectionMods;
                    }

                    // RoutePrice
                    string RoutePriceStr = WaitForDungeonsToRegister.CreateDungeonConfig.DungeonRoutePriceList[DungeonName];
                    Vector2 RoutePriceRarity = new Vector2(0, 99999);
                    List<Vector2WithRarity> InjectionPrices = ConfigAider.ConvertStringToVector2WithRarityList(RoutePriceStr, RoutePriceRarity);
                    if (InjectionPrices.Count > 0)
                    {
                        dungeon.LevelMatchingProperties.currentRoutePrice = InjectionPrices;
                    }
                }
            }
            CentralConfig.instance.mls.LogInfo("Dungeon config Values Applied");
            Ready = true;
        }
    }
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class FrApplyDungeon
    {
        static void Postfix()
        {
            ApplyDungeonConfig applyConfig = new ApplyDungeonConfig();
            applyConfig.UpdateDungeonValues();
        }
    }
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    public static class NewDungeonGenerator
    {
        private static int failureCount = 19;
        static float PreClampValue;
        static bool Prefix(RoundManager __instance)
        {
            if (!CentralConfig.SyncConfig.UseNewGen)
            {
                CentralConfig.instance.mls.LogInfo("New generation is not in use, proceeding with standard generation.");
                return true;
            }

            List<int> list = new List<int>();
            for (int i = 0; i < __instance.currentLevel.dungeonFlowTypes.Length; i++)
            {
                list.Add(__instance.currentLevel.dungeonFlowTypes[i].rarity);
            }

            System.Random seededRandom = new System.Random(StartOfRound.Instance.randomMapSeed - 69);
            int DungeonID = __instance.currentLevel.dungeonFlowTypes[__instance.GetRandomWeightedIndex(list.ToArray(), seededRandom)].id;
            __instance.dungeonGenerator.Generator.DungeonFlow = __instance.dungeonFlowTypes[DungeonID].dungeonFlow;

            if (DungeonID < __instance.firstTimeDungeonAudios.Length && __instance.firstTimeDungeonAudios[DungeonID] != null)
            {
                EntranceTeleport[] array = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
                if (array != null && array.Length != 0)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (array[j].isEntranceToBuilding)
                        {
                            array[j].firstTimeAudio = __instance.firstTimeDungeonAudios[DungeonID];
                            array[j].dungeonFlowId = DungeonID;
                        }
                    }
                }
            }
            ExtendedDungeonFlow dungeon = DungeonManager.CurrentExtendedDungeonFlow;
            string Dun = dungeon.DungeonName + " (" + dungeon.name + ")";
            Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
            string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");

            __instance.dungeonGenerator.Generator.ShouldRandomizeSeed = false;

            while (failureCount < 20)
            {
                __instance.dungeonGenerator.Generator.Seed = StartOfRound.Instance.randomMapSeed + 420;
                float NewMultiplier = __instance.currentLevel.factorySizeMultiplier / __instance.dungeonFlowTypes[DungeonID].MapTileSize * __instance.mapSizeMultiplier;

                if (WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler.ContainsKey(DungeonName))
                {
                    PreClampValue = NewMultiplier;
                    if (NewMultiplier < WaitForDungeonsToRegister.CreateDungeonConfig.MinDungeonSize[DungeonName])
                    {
                        NewMultiplier = Mathf.Lerp(NewMultiplier, WaitForDungeonsToRegister.CreateDungeonConfig.MinDungeonSize[DungeonName], WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler[DungeonName] / 100);
                    }
                    else if (NewMultiplier > WaitForDungeonsToRegister.CreateDungeonConfig.MaxDungeonSize[DungeonName])
                    {
                        NewMultiplier = Mathf.Lerp(NewMultiplier, WaitForDungeonsToRegister.CreateDungeonConfig.MaxDungeonSize[DungeonName], WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler[DungeonName] / 100);
                    }
                    CentralConfig.instance.mls.LogInfo("Clamps for the dungeon have been applied. Original value: " + PreClampValue + " New value: " + NewMultiplier);
                }
                else
                {
                    CentralConfig.instance.mls.LogInfo("Either the current dungeon is blacklisted, or clamp overrides are false. No clamping will be applied.");
                }
                NewMultiplier = (float)((double)Mathf.Round(NewMultiplier * 100f) / 100.0);
                __instance.dungeonGenerator.Generator.LengthMultiplier = NewMultiplier;

                try
                {
                    __instance.dungeonGenerator.Generate();
                    CentralConfig.instance.mls.LogInfo("Dungeon has been loaded by Central Config.");
                    failureCount = 19;
                    return false;
                }
                catch (Exception)
                {
                    failureCount++;
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate")]
    public static class InnerGenerateWithRetries
    {
        private static int retryCount = 20;

        static bool Prefix(DungeonGenerator __instance, bool isRetry, ref IEnumerator __result)
        {
            if (!CentralConfig.SyncConfig.UseNewGen)
            {
                return true;
            }
            if (DungeonManager.CurrentExtendedDungeonFlow.DungeonName == "SectorFlow" || DungeonManager.CurrentExtendedDungeonFlow.DungeonName == "CozyOffice" || DungeonManager.CurrentExtendedDungeonFlow.DungeonName == "Black Mesa")
            {
                retryCount = 15;
                CentralConfig.instance.mls.LogInfo("Current Dungeon is incompatible, granting retries.");
            }
            IEnumerator TerminateCoroutine()
            {
                yield break;
            }

            if (isRetry)
            {
                retryCount++;

                if (retryCount >= 20)
                {
                    __result = TerminateCoroutine();
                    return false;
                }
            }
            else
            {
                retryCount = 20;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate")]
    public static class LogFinalSize
    {
        static void Postfix(DungeonGenerator __instance)
        {
            float finalMultiplier = __instance.LengthMultiplier;
            CentralConfig.instance.mls.LogInfo("Final Length Multiplier " + finalMultiplier);
        }
    }
}