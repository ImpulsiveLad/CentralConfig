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
using System.Web;
using DunGen.Graph;

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class WaitForDungeonsToRegister
    {
        public static CreateDungeonConfig Config;
        public static ExtendedDungeonFlow DefaultFacility;
        [DataContract]
        public class CreateDungeonConfig : ConfigTemplate
        {
            // Declare config entries tied to the dictionary

            [DataMember] public static Dictionary<string, SyncedEntry<float>> MinDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> MaxDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> DungeonSizeScaler;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonPlanetNameList;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonTagList;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonRoutePriceList;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DungeonModNameList;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyByDungeon;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> InteriorEnemiesD;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyReplacementD;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayTimeEnemyByDungeon;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> DayEnemiesD;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayEnemyReplacementD;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightTimeEnemyByDungeon;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> NightEnemiesD;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightEnemyReplacementD;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> ScrapByDungeon;
            [DataMember] public static Dictionary<string, List<SpawnableItemWithRarity>> ScrapD;

            public CreateDungeonConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                // Intialize config entries tied to the dictionary

                MinDungeonSize = new Dictionary<string, SyncedEntry<float>>();
                MaxDungeonSize = new Dictionary<string, SyncedEntry<float>>();
                DungeonSizeScaler = new Dictionary<string, SyncedEntry<float>>();

                DungeonPlanetNameList = new Dictionary<string, SyncedEntry<string>>();
                DungeonTagList = new Dictionary<string, SyncedEntry<string>>();
                DungeonRoutePriceList = new Dictionary<string, SyncedEntry<string>>();
                DungeonModNameList = new Dictionary<string, SyncedEntry<string>>();

                InteriorEnemyByDungeon = new Dictionary<string, SyncedEntry<string>>();
                InteriorEnemiesD = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                InteriorEnemyReplacementD = new Dictionary<string, SyncedEntry<string>>();
                DayTimeEnemyByDungeon = new Dictionary<string, SyncedEntry<string>>();
                DayEnemiesD = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                DayEnemyReplacementD = new Dictionary<string, SyncedEntry<string>>();
                NightTimeEnemyByDungeon = new Dictionary<string, SyncedEntry<string>>();
                NightEnemiesD = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                NightEnemyReplacementD = new Dictionary<string, SyncedEntry<string>>();

                ScrapByDungeon = new Dictionary<string, SyncedEntry<string>>();
                ScrapD = new Dictionary<string, List<SpawnableItemWithRarity>>();

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

                    // CentralConfig.instance.mls.LogInfo(dungeon.DungeonName);
                    if (dungeon.DungeonName == "Facility")
                    {
                        DefaultFacility = dungeon;
                        CentralConfig.instance.mls.LogInfo("Saved Default dungeon" + DefaultFacility.DungeonName);
                    }

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

                        string dungeonTagList = ConfigAider.ConvertStringWithRarityToString(dungeon.LevelMatchingProperties.levelTags);

                        DungeonTagList[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Dungeon by Planet Tags",
                            dungeonTagList,
                            "The dungeon will be added to all moons with a matching tag");

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

                    if (CentralConfig.SyncConfig.DoEnemyInjectionsByDungeon)
                    {
                        InteriorEnemyByDungeon[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Interior Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the interior enemy list on any moons currently featuring this dungeon.");

                        InteriorEnemyReplacementD[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Replace Interior Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Flowerman:Plantman,Crawler:Mauler\",\nOn any moons currently featuring this dungeon, brackens will be replaced with hypothetical plantmen, and crawlers with hypothetical maulers.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");

                        DayTimeEnemyByDungeon[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Day Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the interior enemy list on any moons currently featuring this dungeon.");

                        DayEnemyReplacementD[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Replace Day Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Manticoil:Mantisoil,Docile Locust Bees:Angry Moth Wasps\",\nOn any moons currently featuring this dungeon, manticoils will be replaced with hypothetical mantisoils, and docile locust bees with hypothetical angry moth wasps.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");

                        NightTimeEnemyByDungeon[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Night Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the interior enemy list on any moons currently featuring this dungeon.");

                        NightEnemyReplacementD[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Replace Night Enemies",
                            "Default Values Were Empty",
                            "In the example, \"MouthDog:OceanDog,ForestGiant:FireGiant\",\nOn any moons currently featuring this dungeon, mouthdogs will be replaced with hypothetical oceandogs, and forestgiants with hypothetical firegiants.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");
                    }
                    if (CentralConfig.SyncConfig.DoScrapInjectionsByDungeon)
                    {
                        ScrapByDungeon[DungeonName] = cfg.BindSyncedEntry("Tag: " + DungeonName,
                            DungeonName + " - Add Scrap",
                            "Default Values Were Empty",
                            "Scrap listed here in the ScrapName:rarity,ScrapName,rarity format will be added to the scrap list any moons currently featuring this dungeon");
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

                    // Tags
                    string TagStr = WaitForDungeonsToRegister.CreateDungeonConfig.DungeonTagList[DungeonName];
                    Vector2 tagRarity = new Vector2(0, 99999);
                    List<StringWithRarity> InjectionTags = ConfigAider.ConvertTagStringToStringWithRarityList(TagStr, tagRarity);
                    if (InjectionTags.Count > 0)
                    {
                        dungeon.LevelMatchingProperties.levelTags = InjectionTags;
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

                if (CentralConfig.SyncConfig.DoEnemyInjectionsByDungeon)
                {
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemyByDungeon.ContainsKey(DungeonName))
                    {
                        string IntEneStr = WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemyByDungeon[DungeonName];
                        Vector2 clampIntRarity = new Vector2(0, 99999);
                        List<SpawnableEnemyWithRarity> interiorenemyList = ConfigAider.ConvertStringToEnemyList(IntEneStr, clampIntRarity);
                        WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemiesD[DungeonName] = interiorenemyList;

                        string DayEneStr = WaitForDungeonsToRegister.CreateDungeonConfig.DayTimeEnemyByDungeon[DungeonName];
                        Vector2 clampDayRarity = new Vector2(0, 99999);
                        List<SpawnableEnemyWithRarity> dayenemyList = ConfigAider.ConvertStringToEnemyList(DayEneStr, clampDayRarity);
                        WaitForDungeonsToRegister.CreateDungeonConfig.DayEnemiesD[DungeonName] = dayenemyList;

                        string NightEneStr = WaitForDungeonsToRegister.CreateDungeonConfig.NightTimeEnemyByDungeon[DungeonName];
                        Vector2 clampNightRarity = new Vector2(0, 99999);
                        List<SpawnableEnemyWithRarity> nightenemyList = ConfigAider.ConvertStringToEnemyList(NightEneStr, clampNightRarity);
                        WaitForDungeonsToRegister.CreateDungeonConfig.NightEnemiesD[DungeonName] = nightenemyList;
                    }
                }

                if (CentralConfig.SyncConfig.DoScrapInjectionsByDungeon)
                {
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.ScrapByDungeon.ContainsKey(DungeonName))
                    {
                        string ScrStr = WaitForDungeonsToRegister.CreateDungeonConfig.ScrapByDungeon[DungeonName];
                        Vector2 clampScrRarity = new Vector2(0, 99999);
                        List<SpawnableItemWithRarity> scraplist = ConfigAider.ConvertStringToItemList(ScrStr, clampScrRarity);
                        WaitForDungeonsToRegister.CreateDungeonConfig.ScrapD[DungeonName] = scraplist;
                    }
                }
            }
            CentralConfig.instance.mls.LogInfo("Dungeon config Values Applied.");
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
            CentralConfig.instance.mls.LogInfo("Dungeon Selected: " + DungeonName);

            __instance.dungeonGenerator.Generator.ShouldRandomizeSeed = true;
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
            }
            catch (Exception ex)
            {
                if (ex.Message == "Dungeon Generation failed.")
                {
                    CentralConfig.instance.mls.LogInfo("Dungeon Generation has failed. Defaulting interior");
                    __instance.dungeonGenerator.Generator.DungeonFlow = WaitForDungeonsToRegister.DefaultFacility.DungeonFlow;
                    if (LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier >= 1)
                    {
                        __instance.dungeonGenerator.Generator.LengthMultiplier = LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier;
                    }
                    else
                    {
                        __instance.dungeonGenerator.Generator.LengthMultiplier = 1f;
                    }
                    CentralConfig.instance.mls.LogInfo(DungeonManager.CurrentExtendedDungeonFlow.DungeonName);
                    __instance.dungeonGenerator.Generate();
                    CentralConfig.instance.mls.LogInfo("Dungeon has been loaded by Central Config.");
                    InnerGenerateWithRetries.RetryCounter = 0;
                    InnerGenerateWithRetries.TryBig = false;
                    InnerGenerateWithRetries.GenFailed = false;
                }
            }
            if (!InnerGenerateWithRetries.GenFailed)
            {
                CentralConfig.instance.mls.LogInfo("Dungeon has been loaded by Central Config.");
                InnerGenerateWithRetries.RetryCounter = 0;
                InnerGenerateWithRetries.TryBig = false;
                InnerGenerateWithRetries.GenFailed = false;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate")]
    public static class InnerGenerateWithRetries
    {
        public static int RetryCounter = 0;
        public static bool TryBig = false;
        public static bool GenFailed = false;
        static bool Prefix(DungeonGenerator __instance, ref bool isRetry)
        {
            if (!CentralConfig.SyncConfig.UseNewGen || DungeonManager.CurrentExtendedDungeonFlow.DungeonName == "BunkerFlow")
            {
                return true;
            }
            RoundManager.Instance.dungeonGenerator.Generator.Seed = StartOfRound.Instance.randomMapSeed + 420 - RetryCounter * 5;
            // CentralConfig.instance.mls.LogInfo("Trying seed: " + RoundManager.Instance.dungeonGenerator.Generator.Seed);
            if (RetryCounter > 0)
            {
                if (__instance.LengthMultiplier > 1)
                {
                    __instance.LengthMultiplier -= __instance.LengthMultiplier * 0.1f;
                }
                else
                {
                    __instance.LengthMultiplier -= 0.05f;
                }
                __instance.LengthMultiplier = (float)Math.Round(__instance.LengthMultiplier, 2);
                CentralConfig.instance.mls.LogInfo("Dungeon Length Multiplier reduced to: " + __instance.LengthMultiplier);
            }
            RetryCounter++;

            if (__instance.LengthMultiplier <= 0 && TryBig == true)
            {
                CentralConfig.instance.mls.LogInfo("Dungeon Generation has failed " + RetryCounter + " times, this would indicate the dungeon refused to generate at any size. Please notify me if you see this.");
                GenFailed = true;
                isRetry = false;
                throw new Exception("Dungeon Generation failed.");
            }
            if (__instance.LengthMultiplier <= 0 && TryBig == false)
            {
                if (LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier >= 0)
                {
                    __instance.LengthMultiplier = LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier + 1f;
                }
                else
                {
                    __instance.LengthMultiplier = 1f;
                }
                TryBig = true;
                CentralConfig.instance.mls.LogInfo("Trying to increase dungeon size in case it was too small.");
            }
            isRetry = false;

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
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    [HarmonyPriority(69)]
    public class EnactDungeonInjections
    {
        static void Postfix()
        {
            ExtendedDungeonFlow dungeon = DungeonManager.CurrentExtendedDungeonFlow;

            string Dun = dungeon.DungeonName + " (" + dungeon.name + ")";
            Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
            string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");

            if (CentralConfig.SyncConfig.DoEnemyInjectionsByDungeon)
            {
                if (WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemyByDungeon.ContainsKey(DungeonName))
                {
                    LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies, WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemyReplacementD[DungeonName]);
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemiesD[DungeonName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies.Concat(WaitForDungeonsToRegister.CreateDungeonConfig.InteriorEnemiesD[DungeonName]).ToList();
                    }
                    LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies, WaitForDungeonsToRegister.CreateDungeonConfig.DayEnemyReplacementD[DungeonName]);
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.DayEnemiesD[DungeonName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies.Concat(WaitForDungeonsToRegister.CreateDungeonConfig.DayEnemiesD[DungeonName]).ToList();
                    }
                    LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies, WaitForDungeonsToRegister.CreateDungeonConfig.NightEnemyReplacementD[DungeonName]);
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.NightEnemiesD[DungeonName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies.Concat(WaitForDungeonsToRegister.CreateDungeonConfig.NightEnemiesD[DungeonName]).ToList();
                    }
                }
            }

            if (CentralConfig.SyncConfig.DoScrapInjectionsByDungeon)
            {
                if (WaitForDungeonsToRegister.CreateDungeonConfig.ScrapD.ContainsKey(DungeonName))
                {
                    if (WaitForDungeonsToRegister.CreateDungeonConfig.ScrapD[DungeonName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap.Concat(WaitForDungeonsToRegister.CreateDungeonConfig.ScrapD[DungeonName]).ToList();
                        LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = ConfigAider.RemoveLowerRarityDuplicateItems(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap);
                    }
                }
            }

            CentralConfig.instance.mls.LogInfo("Dungeon Enemy/Scrap Injections Enacted.");
            if (CentralConfig.SyncConfig.RemoveDuplicateEnemies)
            {
                LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = ConfigAider.RemoveLowerRarityDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies);
                LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = ConfigAider.RemoveLowerRarityDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies);
                LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = ConfigAider.RemoveLowerRarityDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies);
                CentralConfig.instance.mls.LogInfo("Duplicate Enemies Removed.");
            }
        }
    }
}
