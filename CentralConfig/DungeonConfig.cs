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
        public static ExtendedDungeonFlow DefaultFacility;
        [DataContract]
        public class CreateDungeonConfig : ConfigTemplate
        {
            // Declare config entries tied to the dictionary

            [DataMember] public static Dictionary<string, SyncedEntry<float>> MinDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> MaxDungeonSize;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> DungeonSizeScaler;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> MapTileSize;

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
                MapTileSize = new Dictionary<string, SyncedEntry<float>>();

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
                    if (DungeonName == "Facility (1)")
                    {
                        DefaultFacility = dungeon;
                        CentralConfig.instance.mls.LogInfo("Saved Default dungeon" + DefaultFacility.DungeonName);
                    }

                    // Size

                    if (CentralConfig.SyncConfig.DoDunSizeOverrides)
                    {
                        float MinSize;
                        float MaxSize;

                        if (dungeon.DynamicDungeonSizeMinMax.x <= 0)
                        {
                            MinSize = 1;
                        }
                        else
                        {
                            MinSize = dungeon.DynamicDungeonSizeMinMax.x;
                        }
                        if (dungeon.DynamicDungeonSizeMinMax.y <= 0)
                        {
                            MaxSize = 2;
                        }
                        else
                        {
                            MaxSize = dungeon.DynamicDungeonSizeMinMax.y;
                        }
                        if (DungeonName == "Haunted Mansion (2)")
                        {
                            dungeon.MapTileSize = 1.5f;
                        }

                        MinDungeonSize[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName, // Assigns the config with the dictionary so that it is unique to the level/moon/planet
                            DungeonName + " - Minimum Size Multiplier",
                            MinSize,
                            "Sets the min size multiplier this dungeon can have.");

                        MaxDungeonSize[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Maximum Size Multiplier",
                            MaxSize,
                            "Sets the max size multiplier this dungeon can have.");

                        DungeonSizeScaler[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Dungeon Size Scaler",
                            100 - (dungeon.DynamicDungeonSizeLerpRate * 100),
                            "This setting controls the strictness of the clamp. At 0%, the clamp is inactive. At 100%, the clamp is fully enforced, pulling any out-of-bounds values back to the nearest boundary. For percentages in between, out-of-bounds values are partially pulled back towards the nearest boundary. For example given a value of 50%, a value exceeding the max would be adjusted halfway back to the max.");

                        // CentralConfig.instance.mls.LogInfo(DungeonName + " has a maptilesize of " + dungeon.MapTileSize);

                        MapTileSize[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Map Tile Size",
                            dungeon.MapTileSize,
                            "The size multiplier from the moon is divided by this value before clamps are applied. It ensures that interiors with different '1x' tile counts and room sizes are comparable in total size.\nThe Facility is 1x and the Mansion is 1.5x in Vanilla.");
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
                            "In the example, \"Flowerman:Plantman,Crawler:Mauler\",\nOn any moons currently featuring this dungeon, Brackens will be replaced with hypothetical Plantmen, and Crawlers with hypothetical Maulers.\nYou could also use inputs such as \"Flowerman-15:Plantman~50\", this will give the Plantman a rarity of 15 instead of using the Bracken's and it will only have a 50% chance to replace.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");

                        DayTimeEnemyByDungeon[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Day Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the day enemy list on any moons currently featuring this dungeon.");

                        DayEnemyReplacementD[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Replace Day Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Manticoil:Mantisoil,Docile Locust Bees:Angry Moth Wasps\",\nOn any moons currently featuring this dungeon, Manticoils will be replaced with hypothetical Mantisoils, and docile locust bees with hypothetical angry moth wasps.\nYou could also use inputs such as \"Manticoil-90:Mantisoil\", this will give the Mantisoil a rarity of 90 instead of using the Manticoil's and it will still have a 100% chance to replace.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");

                        NightTimeEnemyByDungeon[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Add Night Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the night enemy list on any moons currently featuring this dungeon.");

                        NightEnemyReplacementD[DungeonName] = cfg.BindSyncedEntry("Dungeon: " + DungeonName,
                            DungeonName + " - Replace Night Enemies",
                            "Default Values Were Empty",
                            "In the example, \"MouthDog:OceanDog,ForestGiant:FireGiant\",\nOn any moons currently featuring this dungeon, Mouthdogs will be replaced with hypothetical Oceandogs, and Forest giants with hypothetical Fire giants.\nYou could also use inputs such as \"MouthDog:OceanDog~75\", the OceanDog will still inherit the rarity from the MouthDog but it will only have a 75% chance to replace.\nThis runs before the above entry adds new enemies, and after the weather and tag adds enemies.");
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
                    dungeon.LevelMatchingProperties.planetNames.Clear();
                    dungeon.LevelMatchingProperties.levelTags.Clear();
                    dungeon.LevelMatchingProperties.modNames.Clear();
                    dungeon.LevelMatchingProperties.currentRoutePrice.Clear();

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
            if (LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes != null && LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes.Length != 0)
            {
                List<int> list = new List<int>();
                for (int i = 0; i < LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes.Length; i++)
                {
                    list.Add(LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[i].rarity);
                    // CentralConfig.instance.mls.LogInfo($"DungeonFlowType {i}: ID = {LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[i].id}, Rarity = {LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[i].rarity}");
                }

                System.Random seededRandom = new System.Random(StartOfRound.Instance.randomMapSeed - 69);
                int DungeonID = __instance.GetRandomWeightedIndex(list.ToArray(), seededRandom);
                // CentralConfig.instance.mls.LogInfo($"Selected DungeonID: {DungeonID}");

                __instance.dungeonGenerator.Generator.DungeonFlow = __instance.dungeonFlowTypes[LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[DungeonID].id].dungeonFlow;
                __instance.currentDungeonType = LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[DungeonID].id;
                // CentralConfig.instance.mls.LogInfo($"Assigned DungeonFlow: {__instance.dungeonGenerator.Generator.DungeonFlow}, CurrentDungeonType: {__instance.currentDungeonType}");

                if (LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[DungeonID].overrideLevelAmbience != null)
                {
                    SoundManager.Instance.currentLevelAmbience = LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[DungeonID].overrideLevelAmbience;
                }
                else if (LevelManager.CurrentExtendedLevel.SelectableLevel.levelAmbienceClips != null)
                {
                    SoundManager.Instance.currentLevelAmbience = LevelManager.CurrentExtendedLevel.SelectableLevel.levelAmbienceClips;
                }
            }

            ExtendedDungeonFlow dungeon = DungeonManager.CurrentExtendedDungeonFlow;
            string Dun = dungeon.DungeonName + " (" + dungeon.name + ")";
            Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
            string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");
            CentralConfig.instance.mls.LogInfo("Dungeon Selected: " + DungeonName);

            __instance.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
            __instance.dungeonGenerator.Generator.Seed = StartOfRound.Instance.randomMapSeed + 420;

            float NewMultiplier = LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier;
            if (CentralConfig.SyncConfig.DoDunSizeOverrides)
            {
                if (WaitForDungeonsToRegister.CreateDungeonConfig.MapTileSize.ContainsKey(DungeonName))
                {
                    NewMultiplier /= WaitForDungeonsToRegister.CreateDungeonConfig.MapTileSize[DungeonName];
                    NewMultiplier *= __instance.mapSizeMultiplier;
                    NewMultiplier = (float)((double)Mathf.Round(NewMultiplier * 100f) / 100.0);

                    PreClampValue = NewMultiplier;
                    if (NewMultiplier < WaitForDungeonsToRegister.CreateDungeonConfig.MinDungeonSize[DungeonName])
                    {
                        NewMultiplier = Mathf.Lerp(NewMultiplier, WaitForDungeonsToRegister.CreateDungeonConfig.MinDungeonSize[DungeonName], WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler[DungeonName] / 100);
                    }
                    else if (NewMultiplier > WaitForDungeonsToRegister.CreateDungeonConfig.MaxDungeonSize[DungeonName])
                    {
                        NewMultiplier = Mathf.Lerp(NewMultiplier, WaitForDungeonsToRegister.CreateDungeonConfig.MaxDungeonSize[DungeonName], WaitForDungeonsToRegister.CreateDungeonConfig.DungeonSizeScaler[DungeonName] / 100);
                    }
                    NewMultiplier = (float)((double)Mathf.Round(NewMultiplier * 100f) / 100.0);
                    if (PreClampValue != NewMultiplier)
                    {
                        CentralConfig.instance.mls.LogInfo("Clamps for the dungeon have been applied. Original value: " + PreClampValue + " New value: " + NewMultiplier);
                    }
                    else
                    {
                        CentralConfig.instance.mls.LogInfo("The size was within the clamp range. The size value is: " + NewMultiplier);
                    }
                }
                else
                {
                    NewMultiplier /= DungeonManager.CurrentExtendedDungeonFlow.MapTileSize;
                    NewMultiplier *= __instance.mapSizeMultiplier;
                    NewMultiplier = (float)((double)Mathf.Round(NewMultiplier * 100f) / 100.0);

                    CentralConfig.instance.mls.LogInfo("The current dungeon is blacklisted. No clamping will be applied. The size value is: " + NewMultiplier);
                }
            }
            else
            {
                NewMultiplier /= DungeonManager.CurrentExtendedDungeonFlow.MapTileSize;
                NewMultiplier *= __instance.mapSizeMultiplier;
                NewMultiplier = (float)((double)Mathf.Round(NewMultiplier * 100f) / 100.0);

                CentralConfig.instance.mls.LogInfo("Size overrides are false. The size value is: " + NewMultiplier);
            }

            if (NewMultiplier < 0)
            {
                NewMultiplier = 1f;
                CentralConfig.instance.mls.LogInfo("Dungeon size is in the negatives, go fix your size settings. It is now 1x");
            }
            __instance.dungeonGenerator.Generator.LengthMultiplier = NewMultiplier;

            if (!CentralConfig.SyncConfig.UseNewGen)
            {
                CentralConfig.instance.mls.LogInfo("Generation safeguards are disabled, generating without them:");
                __instance.dungeonGenerator.Generate();
                /*for (int i = 0; i < 100; i++)
                  {
                      CentralConfig.instance.mls.LogInfo("Size Multiplier: " + NewMultiplier);
                      __instance.dungeonGenerator.Generate();
                      TileCounter.CountTiles();
                      __instance.dungeonGenerator.Generator.Cancel();
                      __instance.dungeonGenerator.Generator.Seed = StartOfRound.Instance.randomMapSeed + 420 - InnerGenerateWithRetries.RetryCounter * 5;
                      __instance.dungeonGenerator.Generator.LengthMultiplier = NewMultiplier;
                      CentralConfig.instance.mls.LogInfo("Attempt # " + TileCounter.CallNumber);
                  }
                  int countsum = TileCounter.TileCounts.Sum();
                  float averagecount = (float)countsum / TileCounter.TileCounts.Count;
                  averagecount = (float)((double)Mathf.Round(averagecount * 100f) / 100.0);

                  float lengthsum = TileCounter.TileLengths.Sum();
                  float averagelength = lengthsum / TileCounter.TileCounts.Count;
                  float widthsum = TileCounter.TileWidths.Sum();
                  float averagewidth = widthsum / TileCounter.TileWidths.Count;
                  float heightsum = TileCounter.TileHeights.Sum();
                  float averageheight = heightsum / TileCounter.TileHeights.Count;

                  int min = TileCounter.TileCounts.Min();
                  int max = TileCounter.TileCounts.Max();

                  CentralConfig.instance.mls.LogInfo(TileCounter.BigLog);
                  float FloorArea = averagelength * averagewidth;
                  FloorArea = (float)((double)Mathf.Round(FloorArea * 100f) / 100.0);
                  CentralConfig.instance.mls.LogInfo("Tests: " + TileCounter.TileCounts.Count + " sum: " + countsum + " average: " + averagecount + " min: " + min + " max: " + max + " Average Length: " + averagelength + " Average Height: " + averageheight + " Average Width: " + averagewidth + " Floor Area: " + FloorArea);*/
                return false;
            }

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
                    __instance.currentDungeonType = 0;

                    float factorySizeMultiplier;
                    if (LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier >= 1)
                    {
                        factorySizeMultiplier = LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier;
                    }
                    else
                    {
                        factorySizeMultiplier = 1f;
                    }
                    __instance.dungeonGenerator.Generator.LengthMultiplier = Mathf.Clamp(factorySizeMultiplier, 1f, 2.2f);

                    if (LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[0].overrideLevelAmbience != null)
                    {
                        SoundManager.Instance.currentLevelAmbience = LevelManager.CurrentExtendedLevel.SelectableLevel.dungeonFlowTypes[0].overrideLevelAmbience;
                    }
                    else if (LevelManager.CurrentExtendedLevel.SelectableLevel.levelAmbienceClips != null)
                    {
                        SoundManager.Instance.currentLevelAmbience = LevelManager.CurrentExtendedLevel.SelectableLevel.levelAmbienceClips;
                    }

                    __instance.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
                    __instance.dungeonGenerator.Generator.Seed = StartOfRound.Instance.randomMapSeed - 420;
                    CentralConfig.instance.mls.LogInfo(DungeonManager.CurrentExtendedDungeonFlow.DungeonName);

                    InnerGenerateWithRetries.Defaulted = true;
                    __instance.dungeonGenerator.Generator.Cancel();
                    __instance.dungeonGenerator.Generate();
                    CentralConfig.instance.mls.LogInfo("Dungeon has been loaded by Central Config using a safeguard dungeon.");
                }
            }
            if (!InnerGenerateWithRetries.Defaulted)
            {
                CentralConfig.instance.mls.LogInfo("Dungeon has been loaded by Central Config. Final dungeon size multiplier is: " + InnerGenerateWithRetries.LengthMultiplier + "x after " + InnerGenerateWithRetries.RetryCounter + " attempts.");
            }
            InnerGenerateWithRetries.RetryCounter = 0;
            InnerGenerateWithRetries.TryBig = false;
            InnerGenerateWithRetries.GenFailed = false;
            InnerGenerateWithRetries.Defaulted = false;

            InnerGenerateWithRetries.Catch20 = false;
            InnerGenerateWithRetries.Catch10 = false;
            InnerGenerateWithRetries.Catch5 = false;
            InnerGenerateWithRetries.Catch3 = false;
            InnerGenerateWithRetries.Catch2 = false;
            InnerGenerateWithRetries.Catch1 = false;

            return false;
        }
    }
    [HarmonyPatch(typeof(DungeonGenerator), "InnerGenerate")]
    public static class InnerGenerateWithRetries
    {
        public static int RetryCounter = 0;
        public static bool TryBig = false;
        public static bool GenFailed = false;
        public static bool Defaulted = false;
        public static float LengthMultiplier;

        public static bool Catch20 = false;
        public static bool Catch10 = false;
        public static bool Catch5 = false;
        public static bool Catch3 = false;
        public static bool Catch2 = false;
        public static bool Catch1 = false;
        static bool Prefix(DungeonGenerator __instance, ref bool isRetry)
        {
            if (!CentralConfig.SyncConfig.UseNewGen || Defaulted)
            {
                RetryCounter++;
                return true;
            }
            __instance.Seed = StartOfRound.Instance.randomMapSeed + 420 - RetryCounter * 5;
            // CentralConfig.instance.mls.LogInfo("Seed: " + __instance.Seed);

            if (RetryCounter >= CentralConfig.SyncConfig.UnShrankDungenTries)
            {
                if (__instance.LengthMultiplier > 20f)
                {
                    __instance.LengthMultiplier -= __instance.LengthMultiplier * 0.1f;
                }
                else if (__instance.LengthMultiplier > 10f)
                {
                    if (!Catch20)
                    {
                        __instance.LengthMultiplier = 20f;
                        Catch20 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 10f / CentralConfig.SyncConfig.BracketTries;
                    }
                }
                else if (__instance.LengthMultiplier > 5f)
                {
                    if (!Catch10)
                    {
                        __instance.LengthMultiplier = 10f;
                        Catch10 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 5f / CentralConfig.SyncConfig.BracketTries;
                    }
                }
                else if (__instance.LengthMultiplier > 3f)
                {
                    if (!Catch5)
                    {
                        __instance.LengthMultiplier = 5f;
                        Catch5 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 3f / CentralConfig.SyncConfig.BracketTries;
                    }
                }
                else if (__instance.LengthMultiplier > 2f)
                {
                    if (!Catch3)
                    {
                        __instance.LengthMultiplier = 3f;
                        Catch3 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 2f / CentralConfig.SyncConfig.BracketTries;
                    }
                }
                else if (__instance.LengthMultiplier > 1f)
                {
                    if (!Catch2)
                    {
                        __instance.LengthMultiplier = 2f;
                        Catch2 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 1f / CentralConfig.SyncConfig.BracketTries;
                    }
                }
                else
                {
                    if (!Catch1)
                    {
                        __instance.LengthMultiplier = 1f;
                        Catch1 = true;
                    }
                    else
                    {
                        __instance.LengthMultiplier -= 0.02f;
                    }
                }
                __instance.LengthMultiplier = (float)Math.Round(__instance.LengthMultiplier, 2);
                CentralConfig.instance.mls.LogInfo("Dungeon Length Multiplier reduced to: " + __instance.LengthMultiplier);
            }
            else
            {
                CentralConfig.instance.mls.LogInfo("Retrying before reduction on attempt #" + (RetryCounter + 1));
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
            LengthMultiplier = __instance.LengthMultiplier;
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
    public static class TileCounter
    {
        public static string BigLog;
        public static List<int> TileCounts = new List<int>();
        public static List<float> TileLengths = new List<float>();
        public static List<float> TileWidths = new List<float>();
        public static List<float> TileHeights = new List<float>();
        public static Tile[] tiles;
        public static int CallNumber;
        public static void CountTiles()
        {
            tiles = new Tile[0];
            tiles = UnityEngine.Object.FindObjectsOfType<Tile>();
            TileCounts.Add(tiles.Length);
            BigLog += "\nThere are: " + tiles.Length + " tiles.";
            CallNumber++;
            float averageLength = GetAverageTileLength();
            TileLengths.Add(averageLength);
            float averageWidth = GetAverageTileWidth();
            TileWidths.Add(averageWidth);
            float averageHeight = GetAverageTileHeight();
            TileHeights.Add(averageHeight);
        }
        public static float GetAverageTileLength()
        {
            float totalLength = tiles.Sum(tile => tile.TileBoundsOverride.size.x);
            float averageLength = totalLength / tiles.Length;
            return averageLength;
        }

        public static float GetAverageTileWidth()
        {
            float totalWidth = tiles.Sum(tile => tile.TileBoundsOverride.size.z);
            float averageWidth = totalWidth / tiles.Length;
            return averageWidth;
        }
        public static float GetAverageTileHeight()
        {
            float totalHeight = tiles.Sum(tile => tile.TileBoundsOverride.size.y);
            float averageHeight = totalHeight / tiles.Length;
            return averageHeight;
        }
    }
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    [HarmonyPriority(69)]
    public class EnactDungeonInjections
    {
        public static string EnemyTables;
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
                LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = ConfigAider.RemoveDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies);
                LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = ConfigAider.RemoveDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies);
                LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = ConfigAider.RemoveDuplicateEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies);
                CentralConfig.instance.mls.LogInfo("Duplicate Enemies Removed.");
            }
            if (CentralConfig.SyncConfig.LogEnemies)
            {
                ConfigAider.Instance.StartCoroutine(LogEnemyTables());
            }
        }
        static IEnumerator LogEnemyTables()
        {
            yield return new WaitForSeconds(10);

            List<SpawnableEnemyWithRarity> InteriorEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies;
            EnemyTables += "\nInterior Enemy List for current game:";
            foreach (SpawnableEnemyWithRarity enemy in InteriorEnemies)
            {
                EnemyTables += $"\n{enemy.enemyType.enemyName},{enemy.rarity}";
            }

            List<SpawnableEnemyWithRarity> DaytimeEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies;
            EnemyTables += "\n\nDaytime Enemy List for current game:";
            foreach (SpawnableEnemyWithRarity enemy in DaytimeEnemies)
            {
                EnemyTables += $"\n{enemy.enemyType.enemyName},{enemy.rarity}";
            }

            List<SpawnableEnemyWithRarity> NightEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies;
            EnemyTables += "\n\nNighttime Enemy List for current game:";
            foreach (SpawnableEnemyWithRarity enemy in NightEnemies)
            {
                EnemyTables += $"\n{enemy.enemyType.enemyName},{enemy.rarity}";
            }

            CentralConfig.instance.mls.LogInfo(EnemyTables);
        }
    }
}