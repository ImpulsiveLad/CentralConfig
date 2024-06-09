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
using Unity.Netcode;
using System.Collections;
using System.Reflection.Emit;

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPriority(666)]
    public class WaitForMoonsToRegister
    {
        public static CreateMoonConfig Config;

        public static SpawnableMapObject turretContainerObjectReference = null;
        public static SpawnableMapObject landmineObjectReference = null;
        public static SpawnableMapObject spikeRoofTrapHazardObjectReference = null;

        [DataContract]
        public class CreateMoonConfig : ConfigTemplate
        {
            // Declare config entries tied to the dictionary

            [DataMember] public static Dictionary<string, SyncedEntry<int>> RoutePriceOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> RiskLevelOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DescriptionOverride;

            [DataMember] public static Dictionary<string, SyncedEntry<int>> MinScrapOverrides;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MaxScrapOverrides;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> ScrapValueMultiplier;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> ScrapListOverrides;

            [DataMember] public static Dictionary<string, SyncedEntry<int>> InteriorEnemyPowerCountOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> DaytimeEnemyPowerCountOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DaytimeEnemyOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> NighttimeEnemyPowerCountOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NighttimeEnemyOverride;

            [DataMember] public static Dictionary<string, SyncedEntry<int>> MinTurretOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MaxTurretOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MinMineOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MaxMineOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MinSpikeTrapOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<int>> MaxSpikeTrapOverride;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> WeatherTypeOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> AddTags;
            [DataMember] public static Dictionary<string, SyncedEntry<bool>> VisibleOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<bool>> LockedOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<bool>> TimeOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> TimeMultiplierOverride;
            [DataMember] public static Dictionary<string, SyncedEntry<bool>> WatiForShipToLandBeforeTimeMoves;

            [DataMember] public static Dictionary<string, SyncedEntry<float>> FaciltySizeOverride;

            public CreateMoonConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                // Intialize config entries tied to the dictionary

                RoutePriceOverride = new Dictionary<string, SyncedEntry<int>>();
                RiskLevelOverride = new Dictionary<string, SyncedEntry<string>>();
                DescriptionOverride = new Dictionary<string, SyncedEntry<string>>();

                MinScrapOverrides = new Dictionary<string, SyncedEntry<int>>();
                MaxScrapOverrides = new Dictionary<string, SyncedEntry<int>>();
                ScrapValueMultiplier = new Dictionary<string, SyncedEntry<float>>();
                ScrapListOverrides = new Dictionary<string, SyncedEntry<string>>();

                InteriorEnemyPowerCountOverride = new Dictionary<string, SyncedEntry<int>>();
                InteriorEnemyOverride = new Dictionary<string, SyncedEntry<string>>();
                DaytimeEnemyPowerCountOverride = new Dictionary<string, SyncedEntry<int>>();
                DaytimeEnemyOverride = new Dictionary<string, SyncedEntry<string>>();
                NighttimeEnemyPowerCountOverride = new Dictionary<string, SyncedEntry<int>>();
                NighttimeEnemyOverride = new Dictionary<string, SyncedEntry<string>>();

                MinTurretOverride = new Dictionary<string, SyncedEntry<int>>();
                MaxTurretOverride = new Dictionary<string, SyncedEntry<int>>();
                MinMineOverride = new Dictionary<string, SyncedEntry<int>>();
                MaxMineOverride = new Dictionary<string, SyncedEntry<int>>();
                MinSpikeTrapOverride = new Dictionary<string, SyncedEntry<int>>();
                MaxSpikeTrapOverride = new Dictionary<string, SyncedEntry<int>>();

                WeatherTypeOverride = new Dictionary<string, SyncedEntry<string>>();
                AddTags = new Dictionary<string, SyncedEntry<string>>();

                VisibleOverride = new Dictionary<string, SyncedEntry<bool>>();
                LockedOverride = new Dictionary<string, SyncedEntry<bool>>();
                TimeOverride = new Dictionary<string, SyncedEntry<bool>>();
                TimeMultiplierOverride = new Dictionary<string, SyncedEntry<float>>();
                WatiForShipToLandBeforeTimeMoves = new Dictionary<string, SyncedEntry<bool>>();

                FaciltySizeOverride = new Dictionary<string, SyncedEntry<float>>();

                List<ExtendedLevel> allExtendedLevels;
                string ignoreList = CentralConfig.SyncConfig.BlacklistMoons.Value;

                if (CentralConfig.SyncConfig.IsWhiteList)
                {
                    allExtendedLevels = PatchedContent.ExtendedLevels.Where(level => ignoreList.Split(',').Any(b => level.NumberlessPlanetName.Equals(b))).ToList();
                }
                else
                {
                    allExtendedLevels = PatchedContent.ExtendedLevels.Where(level => !ignoreList.Split(',').Any(b => level.NumberlessPlanetName.Equals(b))).ToList();
                }
                foreach (ExtendedLevel level in allExtendedLevels)
                {
                    string PlanetName = level.NumberlessPlanetName;

                    // General

                    if (CentralConfig.SyncConfig.DoGenOverrides)
                    {
                        RoutePriceOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName, // Assigns the config with the dictionary so that it is unique to the level/moon/planet
                            PlanetName + " - Route Price",
                            level.RoutePrice,
                            "Sets the cost of routing to the moon.");

                        RiskLevelOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Risk Level",
                            level.SelectableLevel.riskLevel,
                            "Sets the risk level of the moon (only visual).");

                        DescriptionOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Description",
                            level.SelectableLevel.LevelDescription,
                            "Sets the description of the moon, \\n is used to go to the next line (basically the enter key) (only visual).");
                    }

                    // Scrap

                    if (CentralConfig.SyncConfig.DoScrapOverrides)
                    {
                        MinScrapOverrides[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Min Scrap",
                            level.SelectableLevel.minScrap,
                            "Sets the minimum amount of scrap to generate on the moon.");

                        MaxScrapOverrides[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Max Scrap",
                            level.SelectableLevel.maxScrap,
                            "Sets the maximum amount of scrap to generate on the moon.");

                        ScrapValueMultiplier[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " -  Scrap Value Multiplier",
                            1f,
                            "Each scrap object on this moon will have its personal min/max values multiplied by this amount.");

                        string ScrapList = ConfigAider.ConvertItemListToString(level.SelectableLevel.spawnableScrap); // Method turns the scrap list into string (check postfix)

                        ScrapListOverrides[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Scrap List",
                            ScrapList,
                            "Sets the list of scrap with attached rarities to generate on the moon.");
                    }

                    // Enemies

                    if (CentralConfig.SyncConfig.DoEnemyOverrides)
                    {
                        InteriorEnemyPowerCountOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Interior Enemy Power",
                            level.SelectableLevel.maxEnemyPowerCount,
                            "Sets the power available for interior enemies on the moon.");

                        string InteriorEnemyList = ConfigAider.ConvertEnemyListToString(level.SelectableLevel.Enemies); // As above with these lists

                        InteriorEnemyOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Interior Enemy List",
                            InteriorEnemyList,
                            "Sets the spawn weights for interior enemies on the moon.");

                        DaytimeEnemyPowerCountOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Daytime Enemy Power",
                            level.SelectableLevel.maxDaytimeEnemyPowerCount,
                            "Sets the power available for daytime enemies on the moon.");

                        string DaytimeEnemyList = ConfigAider.ConvertEnemyListToString(level.SelectableLevel.DaytimeEnemies);

                        DaytimeEnemyOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Daytime Enemy List",
                            DaytimeEnemyList,
                            "Sets the spawn weights for daytime enemies on the moon.");

                        NighttimeEnemyPowerCountOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Nighttime Enemy Power",
                            level.SelectableLevel.maxOutsideEnemyPowerCount,
                            "Sets the power available for nighttime enemies on the moon.");

                        string NighttimeEnemyList = ConfigAider.ConvertEnemyListToString(level.SelectableLevel.OutsideEnemies);

                        NighttimeEnemyOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Nighttime Enemy List",
                            NighttimeEnemyList,
                            "Sets the spawn weights for nighttime enemies on the moon.");

                    }

                    // Traps

                    if (CentralConfig.SyncConfig.DoTrapOverrides)
                    {
                        List<string> mapObjectNames = level.SelectableLevel.spawnableMapObjects.Select(mapObject => mapObject.prefabToSpawn.name).ToList();

                        var turretContainerObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "TurretContainer");
                        if (turretContainerObject != null)
                        {
                            AnimationCurve numberToSpawnCurve = turretContainerObject.numberToSpawn;
                            int leftMost = (int)Math.Round(numberToSpawnCurve.Evaluate(0));
                            int rightMost = (int)Math.Round(numberToSpawnCurve.Evaluate(numberToSpawnCurve.keys[numberToSpawnCurve.length - 1].time));

                            MinTurretOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Turrets",
                                leftMost,
                                "Sets the minimum number of turrets to spawn on the moon.");

                            MaxTurretOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Turrets",
                                rightMost,
                                "Sets the maximum number of turrets to spawn on the moon.");

                            if (turretContainerObjectReference == null)
                            {
                                turretContainerObjectReference = Array.Find(level.SelectableLevel.spawnableMapObjects, obj => obj.prefabToSpawn != null && obj.prefabToSpawn.name == "TurretContainer");

                                if (turretContainerObjectReference != null)
                                {
                                    // CentralConfig.instance.mls.LogInfo("TurretContainer reference saved from " + PlanetName);
                                }
                            }
                        }
                        else
                        {
                            MinTurretOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Turrets",
                                0,
                                "Sets the minimum number of turrets to spawn on the moon.");

                            MaxTurretOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Turrets",
                                0,
                                "Sets the maximum number of turrets to spawn on the moon.");

                            if (turretContainerObjectReference != null)
                            {
                                SpawnableMapObject newObject = new SpawnableMapObject
                                {
                                    prefabToSpawn = turretContainerObjectReference.prefabToSpawn,
                                    spawnFacingAwayFromWall = true,
                                    spawnFacingWall = false,
                                    spawnWithBackToWall = false,
                                    spawnWithBackFlushAgainstWall = false,
                                    requireDistanceBetweenSpawns = false,
                                    disallowSpawningNearEntrances = false
                                };
                                SpawnableMapObject[] newArray = new SpawnableMapObject[level.SelectableLevel.spawnableMapObjects.Length + 1];
                                Array.Copy(level.SelectableLevel.spawnableMapObjects, newArray, level.SelectableLevel.spawnableMapObjects.Length);
                                newArray[newArray.Length - 1] = newObject;
                                level.SelectableLevel.spawnableMapObjects = newArray;

                                // CentralConfig.instance.mls.LogInfo("Successfully accessed stored TurretContainer reference to use for " + PlanetName);
                            }
                            else
                            {
                                // CentralConfig.instance.mls.LogInfo("Tried to access unstored TurretContainer reference to use for " + PlanetName);
                            }
                        }
                        var landmineObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "Landmine");
                        if (landmineObject != null)
                        {
                            AnimationCurve numberToSpawnCurve = landmineObject.numberToSpawn;
                            int leftMost = (int)Math.Round(numberToSpawnCurve.Evaluate(0));
                            int rightMost = (int)Math.Round(numberToSpawnCurve.Evaluate(numberToSpawnCurve.keys[numberToSpawnCurve.length - 1].time));

                            MinMineOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Mines",
                                leftMost,
                                "Sets the minimum number of mines to spawn on the moon.");

                            MaxMineOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Mines",
                                rightMost,
                                "Sets the maximum number of mines to spawn on the moon.");

                            if (landmineObjectReference == null)
                            {
                                landmineObjectReference = Array.Find(level.SelectableLevel.spawnableMapObjects, obj => obj.prefabToSpawn != null && obj.prefabToSpawn.name == "Landmine");

                                if (landmineObjectReference != null)
                                {
                                    // CentralConfig.instance.mls.LogInfo("Landmine reference saved from " + PlanetName);
                                }
                            }
                        }
                        else
                        {
                            MinMineOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Mines",
                                0,
                                "Sets the minimum number of mines to spawn on the moon.");

                            MaxMineOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Mines",
                                0,
                                "Sets the maximum number of mines to spawn on the moon.");

                            if (landmineObjectReference != null)
                            {
                                SpawnableMapObject newObject = new SpawnableMapObject
                                {
                                    prefabToSpawn = landmineObjectReference.prefabToSpawn,
                                    spawnFacingAwayFromWall = false,
                                    spawnFacingWall = false,
                                    spawnWithBackToWall = false,
                                    spawnWithBackFlushAgainstWall = false,
                                    requireDistanceBetweenSpawns = false,
                                    disallowSpawningNearEntrances = false
                                };
                                SpawnableMapObject[] newArray = new SpawnableMapObject[level.SelectableLevel.spawnableMapObjects.Length + 1];
                                Array.Copy(level.SelectableLevel.spawnableMapObjects, newArray, level.SelectableLevel.spawnableMapObjects.Length);
                                newArray[newArray.Length - 1] = newObject;
                                level.SelectableLevel.spawnableMapObjects = newArray;

                                // CentralConfig.instance.mls.LogInfo("Successfully accessed stored Landmine reference to use for " + PlanetName);
                            }
                            else
                            {
                                // CentralConfig.instance.mls.LogInfo("Tried to access unstored Landmine reference to use for " + PlanetName);
                            }
                        }
                        var spikeRoofTrapHazardObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "SpikeRoofTrapHazard");
                        if (spikeRoofTrapHazardObject != null)
                        {
                            AnimationCurve numberToSpawnCurve = spikeRoofTrapHazardObject.numberToSpawn;
                            int leftMost = (int)Math.Round(numberToSpawnCurve.Evaluate(0));
                            int rightMost = (int)Math.Round(numberToSpawnCurve.Evaluate(numberToSpawnCurve.keys[numberToSpawnCurve.length - 1].time));

                            MinSpikeTrapOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Spike Traps",
                                leftMost,
                                "Sets the minimum number of spike traps to spawn on the moon.");

                            MaxSpikeTrapOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Spike Traps",
                                rightMost,
                                "Sets the maximum number of spike traps to spawn on the moon.");

                            if (spikeRoofTrapHazardObjectReference == null)
                            {
                                spikeRoofTrapHazardObjectReference = Array.Find(level.SelectableLevel.spawnableMapObjects, obj => obj.prefabToSpawn != null && obj.prefabToSpawn.name == "SpikeRoofTrapHazard");

                                if (spikeRoofTrapHazardObjectReference != null)
                                {
                                    // CentralConfig.instance.mls.LogInfo("SpikeRoofTrapHazard reference saved from " + PlanetName);
                                }
                            }
                        }
                        else
                        {
                            MinSpikeTrapOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Min Spike Traps",
                                0,
                                "Sets the minimum number of spike traps to spawn on the moon.");

                            MaxSpikeTrapOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                                PlanetName + " - Max Spike Traps",
                                0,
                                "Sets the maximum number of spike traps to spawn on the moon.");

                            if (spikeRoofTrapHazardObjectReference != null)
                            {
                                SpawnableMapObject newObject = new SpawnableMapObject
                                {
                                    prefabToSpawn = spikeRoofTrapHazardObjectReference.prefabToSpawn,
                                    spawnFacingAwayFromWall = false,
                                    spawnFacingWall = true,
                                    spawnWithBackToWall = true,
                                    spawnWithBackFlushAgainstWall = true,
                                    requireDistanceBetweenSpawns = true,
                                    disallowSpawningNearEntrances = false
                                };
                                SpawnableMapObject[] newArray = new SpawnableMapObject[level.SelectableLevel.spawnableMapObjects.Length + 1];
                                Array.Copy(level.SelectableLevel.spawnableMapObjects, newArray, level.SelectableLevel.spawnableMapObjects.Length);
                                newArray[newArray.Length - 1] = newObject;
                                level.SelectableLevel.spawnableMapObjects = newArray;

                                // CentralConfig.instance.mls.LogInfo("Successfully accessed stored SpikeRoofTrapHazard reference to use for " + PlanetName);
                            }
                            else
                            {
                                // CentralConfig.instance.mls.LogInfo("Tried to access unstored SpikeRoofTrapHazard reference to use for " + PlanetName);
                            }
                        }
                    }

                    // Weather + Tags

                    if (CentralConfig.SyncConfig.DoWeatherAndTagOverrides)
                    {
                        string PossibleWeatherArray = ConfigAider.ConvertWeatherArrayToString(level.SelectableLevel.randomWeathers);

                        WeatherTypeOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Possible Weathers",
                            PossibleWeatherArray,
                            "Sets the possible weathers that can occur on the moon");

                        string ContentTags = ConfigAider.ConvertTagsToString(level.ContentTags);

                        AddTags[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Content Tags",
                            ContentTags,
                            "Add new content tags to the moon (The tags shown in the default value cannot be removed).");
                    }

                    // Misc

                    if (CentralConfig.SyncConfig.DoDangerBools)
                    {
                        VisibleOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Should The Moon Be Hidden?",
                            level.IsRouteHidden,
                            "Set to true to hide the moon in the terminal.");

                        LockedOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Should The Moon Be Locked?",
                            level.IsRouteLocked,
                            "Set to true to prevent visiting the moon.");

                        TimeOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Should The Moon Have Time?",
                            level.SelectableLevel.planetHasTime,
                            "Set to true to enable time progression. Set to false for no time progression.");

                        TimeMultiplierOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Day Speed Multiplier",
                            level.SelectableLevel.DaySpeedMultiplier,
                            "Adjusts the speed of day progression. For example, 2 means 2x faster.");

                        WatiForShipToLandBeforeTimeMoves[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Should Time Wait on the Ship?",
                            true,
                            "Set to true to make time only progress AFTER the ship has landed.");
                    }

                    // Dungeon Size

                    if (CentralConfig.SyncConfig.DoDunSizeOverrides)
                    {
                        FaciltySizeOverride[PlanetName] = cfg.BindSyncedEntry("Moon: " + PlanetName,
                            PlanetName + " - Dungeon Size",
                            level.SelectableLevel.factorySizeMultiplier,
                            "Sets the dungeon size multiplier granted by this moon.");
                    }
                }
                ConfigAider.Instance.CleanConfig(cfg); // Cleans out orphaned config entries (ones that you don't want to use anymore)
                CentralConfig.instance.mls.LogInfo("Moon config has been registered.");
            }
        }
        static void Prefix()
        {
            if (StartOfRound.Instance.randomMapSeed == 0 && NetworkManager.Singleton.IsHost)
            {
                StartOfRound.Instance.randomMapSeed = UnityEngine.Random.Range(1, 100000000);
                CentralConfig.instance.mls.LogInfo("Starting Seed " + StartOfRound.Instance.randomMapSeed);
            }
            CentralConfig.ConfigFile = new CreateMoonConfig(CentralConfig.instance.Config); // Moon config is created when you join a lobby (So every other config is already applied)
        }
    }
    public class ApplyMoonConfig
    {
        public static bool Ready = false;
        public void UpdateMoonValues() // This is called on as a postfix on the same method as creating the config stuff so it gets applied here right after the config is intialized
        {
            List<ExtendedLevel> allExtendedLevels;
            string ignoreList = CentralConfig.SyncConfig.BlacklistMoons.Value;

            if (CentralConfig.SyncConfig.IsWhiteList)
            {
                allExtendedLevels = PatchedContent.ExtendedLevels.Where(level => ignoreList.Split(',').Any(b => level.NumberlessPlanetName.Equals(b))).ToList();
            }
            else
            {
                allExtendedLevels = PatchedContent.ExtendedLevels.Where(level => !ignoreList.Split(',').Any(b => level.NumberlessPlanetName.Equals(b))).ToList();
            }
            foreach (ExtendedLevel level in allExtendedLevels)
            {

                string PlanetName = level.NumberlessPlanetName;

                // General

                if (CentralConfig.SyncConfig.DoGenOverrides)
                {
                    level.RoutePrice = WaitForMoonsToRegister.CreateMoonConfig.RoutePriceOverride[PlanetName]; // These are just easy variables

                    level.SelectableLevel.riskLevel = WaitForMoonsToRegister.CreateMoonConfig.RiskLevelOverride[PlanetName];

                    level.SelectableLevel.LevelDescription = WaitForMoonsToRegister.CreateMoonConfig.DescriptionOverride[PlanetName];
                    level.OverrideRouteNodeDescription = WaitForMoonsToRegister.CreateMoonConfig.DescriptionOverride[PlanetName];
                    level.OverrideRouteConfirmNodeDescription = WaitForMoonsToRegister.CreateMoonConfig.DescriptionOverride[PlanetName];
                    level.OverrideInfoNodeDescription = WaitForMoonsToRegister.CreateMoonConfig.DescriptionOverride[PlanetName];
                }

                // Scrap

                if (CentralConfig.SyncConfig.DoScrapOverrides)
                {
                    level.SelectableLevel.minScrap = WaitForMoonsToRegister.CreateMoonConfig.MinScrapOverrides[PlanetName];
                    level.SelectableLevel.maxScrap = WaitForMoonsToRegister.CreateMoonConfig.MaxScrapOverrides[PlanetName];

                    // ScrapList
                    string scrapStr = WaitForMoonsToRegister.CreateMoonConfig.ScrapListOverrides[PlanetName]; // Ok so the lists kinda suck
                    Vector2 clamprarity = new Vector2(0, 99999);
                    List<SpawnableItemWithRarity> scrap = ConfigAider.ConvertStringToItemList(scrapStr, clamprarity); // This method turns the string back into a list
                    if (scrap.Count > 0)
                    {
                        level.SelectableLevel.spawnableScrap = scrap;
                    }
                }

                // Enemies

                if (CentralConfig.SyncConfig.DoEnemyOverrides)
                {
                    if (CentralConfig.SyncConfig.ScaleEnemySpawnRate)
                    {
                        float Intmultiplier = WaitForMoonsToRegister.CreateMoonConfig.InteriorEnemyPowerCountOverride[PlanetName] / level.SelectableLevel.maxEnemyPowerCount;
                        level.SelectableLevel.enemySpawnChanceThroughoutDay = ConfigAider.MultiplyYValues(level.SelectableLevel.enemySpawnChanceThroughoutDay, Intmultiplier, level.NumberlessPlanetName, "Interior Curve");
                        float Daymultiplier = WaitForMoonsToRegister.CreateMoonConfig.DaytimeEnemyPowerCountOverride[PlanetName] / level.SelectableLevel.maxDaytimeEnemyPowerCount;
                        level.SelectableLevel.daytimeEnemySpawnChanceThroughDay = ConfigAider.MultiplyYValues(level.SelectableLevel.daytimeEnemySpawnChanceThroughDay, Daymultiplier, level.NumberlessPlanetName, "Daytime Curve");
                        float Noxmultiplier = WaitForMoonsToRegister.CreateMoonConfig.NighttimeEnemyPowerCountOverride[PlanetName] / level.SelectableLevel.maxOutsideEnemyPowerCount;
                        level.SelectableLevel.outsideEnemySpawnChanceThroughDay = ConfigAider.MultiplyYValues(level.SelectableLevel.outsideEnemySpawnChanceThroughDay, Noxmultiplier, level.NumberlessPlanetName, "Nighttime Curve");
                    }
                    level.SelectableLevel.maxEnemyPowerCount = WaitForMoonsToRegister.CreateMoonConfig.InteriorEnemyPowerCountOverride[PlanetName]; // Same as the scrap list but I had to explicitly exclude Lasso since he will fuck up the stuff (pls for the love of god if you bring back Lasso don't make its enemyName = "Lasso" I will cry) ((This mod will ignore it))
                    // InteriorEnemyList
                    string IntEneStr = WaitForMoonsToRegister.CreateMoonConfig.InteriorEnemyOverride[PlanetName];
                    Vector2 clampIntRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> IntEnemies = ConfigAider.ConvertStringToEnemyList(IntEneStr, clampIntRarity);
                    if (IntEnemies.Count > 0)
                    {
                        level.SelectableLevel.Enemies = IntEnemies;
                    }

                    level.SelectableLevel.maxDaytimeEnemyPowerCount = WaitForMoonsToRegister.CreateMoonConfig.DaytimeEnemyPowerCountOverride[PlanetName];
                    // DaytimeEnemyList
                    string DayEneStr = WaitForMoonsToRegister.CreateMoonConfig.DaytimeEnemyOverride[PlanetName];
                    Vector2 clampDayRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> DayEnemies = ConfigAider.ConvertStringToEnemyList(DayEneStr, clampDayRarity);
                    if (DayEnemies.Count > 0)
                    {
                        level.SelectableLevel.DaytimeEnemies = DayEnemies;
                    }

                    level.SelectableLevel.maxOutsideEnemyPowerCount = WaitForMoonsToRegister.CreateMoonConfig.NighttimeEnemyPowerCountOverride[PlanetName];
                    // NighttimeEnemyList
                    string NightEneStr = WaitForMoonsToRegister.CreateMoonConfig.NighttimeEnemyOverride[PlanetName];
                    Vector2 clampNightRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> NightEnemies = ConfigAider.ConvertStringToEnemyList(NightEneStr, clampNightRarity);
                    if (NightEnemies.Count > 0)
                    {
                        level.SelectableLevel.OutsideEnemies = NightEnemies;
                    }
                }

                // Traps

                if (CentralConfig.SyncConfig.DoTrapOverrides)
                {
                    List<string> mapObjectNames = level.SelectableLevel.spawnableMapObjects.Select(mapObject => mapObject.prefabToSpawn.name).ToList();

                    var turretContainerObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "TurretContainer");
                    if (turretContainerObject != null)
                    {
                        int minTurrets = WaitForMoonsToRegister.CreateMoonConfig.MinTurretOverride[PlanetName];
                        int maxTurrets = WaitForMoonsToRegister.CreateMoonConfig.MaxTurretOverride[PlanetName];

                        Keyframe key1 = new Keyframe(0, minTurrets);
                        Keyframe key2 = new Keyframe(1, maxTurrets);
                        AnimationCurve newCurve = new AnimationCurve(key1, key2);
                        turretContainerObject.numberToSpawn = newCurve;
                    }
                    var landmineObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "Landmine");
                    if (landmineObject != null)
                    {
                        int minMines = WaitForMoonsToRegister.CreateMoonConfig.MinMineOverride[PlanetName];
                        int maxMines = WaitForMoonsToRegister.CreateMoonConfig.MaxMineOverride[PlanetName];

                        Keyframe key1 = new Keyframe(0, minMines);
                        Keyframe key2 = new Keyframe(1, maxMines);
                        AnimationCurve newCurve = new AnimationCurve(key1, key2);
                        landmineObject.numberToSpawn = newCurve;
                    }
                    var spikeRoofTrapHazardObject = level.SelectableLevel.spawnableMapObjects.FirstOrDefault(mo => mo.prefabToSpawn.name == "SpikeRoofTrapHazard");
                    if (spikeRoofTrapHazardObject != null)
                    {
                        int minSpikes = WaitForMoonsToRegister.CreateMoonConfig.MinSpikeTrapOverride[PlanetName];
                        int maxSpikes = WaitForMoonsToRegister.CreateMoonConfig.MaxSpikeTrapOverride[PlanetName];

                        Keyframe key1 = new Keyframe(0, minSpikes);
                        Keyframe key2 = new Keyframe(1, maxSpikes);
                        AnimationCurve newCurve = new AnimationCurve(key1, key2);
                        spikeRoofTrapHazardObject.numberToSpawn = newCurve;
                    }
                }

                // Weather + Tags

                if (CentralConfig.SyncConfig.DoWeatherAndTagOverrides)
                {
                    string WeatherStr = WaitForMoonsToRegister.CreateMoonConfig.WeatherTypeOverride[PlanetName];
                    RandomWeatherWithVariables[] PossibleWeathers = ConfigAider.ConvertStringToWeatherArray(WeatherStr);
                    level.SelectableLevel.randomWeathers = PossibleWeathers;
                    level.SelectableLevel.overrideWeather = false;
                    string PossibleWeatherArray = ConfigAider.ConvertWeatherArrayToString(level.SelectableLevel.randomWeathers);

                    Ororo Ororo = new Ororo();
                    Ororo.SetSinglePlanetWeather(level);

                    string TagStr = WaitForMoonsToRegister.CreateMoonConfig.AddTags[PlanetName];
                    List<ContentTag> MoonTags = ConfigAider.ConvertStringToTagList(TagStr);
                    if (MoonTags.Count > 0)
                    {
                        level.ContentTags.AddRange(MoonTags);
                    }
                }

                // Misc

                if (CentralConfig.SyncConfig.DoDangerBools)
                {
                    level.IsRouteHidden = WaitForMoonsToRegister.CreateMoonConfig.VisibleOverride[PlanetName];
                    level.IsRouteLocked = WaitForMoonsToRegister.CreateMoonConfig.LockedOverride[PlanetName];
                    level.SelectableLevel.planetHasTime = WaitForMoonsToRegister.CreateMoonConfig.TimeOverride[PlanetName];
                }

                // Dungeon Size

                if (CentralConfig.SyncConfig.DoDunSizeOverrides)
                {
                    level.SelectableLevel.factorySizeMultiplier = WaitForMoonsToRegister.CreateMoonConfig.FaciltySizeOverride[PlanetName];
                }
            }
            CentralConfig.instance.mls.LogInfo("Moon config Values Applied.");
            Ready = true;
        }
    }
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class FrApplyMoon
    {
        static void Postfix()
        {
            ApplyMoonConfig applyConfig = new ApplyMoonConfig();
            applyConfig.UpdateMoonValues();
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnDaytimeEnemiesOutside")]
    public class CountTraps
    {
        static void Postfix(RoundManager __instance)
        {
            var landmines = UnityEngine.Object.FindObjectsOfType<Landmine>();

            CentralConfig.instance.mls.LogInfo("Number of landmines in the level: " + landmines.Length);

            var turrets = UnityEngine.Object.FindObjectsOfType<Turret>();

            CentralConfig.instance.mls.LogInfo("Number of turrets in the level: " + turrets.Length);

            var spikes = UnityEngine.Object.FindObjectsOfType<SpikeRoofTrap>();

            CentralConfig.instance.mls.LogInfo("Number of spike traps in the level: " + spikes.Length);
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class ApplyScrapValueMultiplier
    {
        static bool Prefix(RoundManager __instance)
        {
            if (CentralConfig.SyncConfig.FreeEnemies)
            {
                __instance.hourTimeBetweenEnemySpawnBatches = 1;
            }
            if (!CentralConfig.SyncConfig.DoScrapOverrides)
            {
                CentralConfig.instance.mls.LogInfo("Scrap Overrides are disabled, not applying multiplier.");
                return true;
            }

            string currentMoon = LevelManager.CurrentExtendedLevel.NumberlessPlanetName;

            if (WaitForMoonsToRegister.CreateMoonConfig.ScrapValueMultiplier.ContainsKey(currentMoon))
            {
                __instance.scrapValueMultiplier = WaitForMoonsToRegister.CreateMoonConfig.ScrapValueMultiplier[currentMoon].Value / 2.5f;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(TimeOfDay), "MoveGlobalTime")]
    public static class TimeFix
    {
        static bool Prefix(TimeOfDay __instance)
        {
            if (!CentralConfig.SyncConfig.DoDangerBools)
            {
                return true;
            }
            StartOfRound startOfRound = StartOfRound.Instance;
            string currentMoon = LevelManager.CurrentExtendedLevel.NumberlessPlanetName;
            if (WaitForMoonsToRegister.CreateMoonConfig.WatiForShipToLandBeforeTimeMoves[currentMoon].Value)
            {
                if (!startOfRound.shipHasLanded && !__instance.shipLeavingAlertCalled)
                {
                    return false;
                }
            }
            __instance.globalTimeSpeedMultiplier = WaitForMoonsToRegister.CreateMoonConfig.TimeMultiplierOverride[currentMoon].Value;

            float num = __instance.globalTime;
            __instance.globalTime = Mathf.Clamp(__instance.globalTime + Time.deltaTime * __instance.globalTimeSpeedMultiplier, 0f, __instance.globalTimeAtEndOfDay);
            num = __instance.globalTime - num;
            __instance.timeUntilDeadline -= num;
            CentralConfig.shid += num;
            // CentralConfig.instance.mls.LogInfo("shid is now : " + CentralConfig.shid);

            return false;
        }
    }
    [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
    public static class DayTimePassFix
    {
        static bool Prefix(StartOfRound __instance, int connectedPlayersOnServer = 0)
        {
            if (!CentralConfig.SyncConfig.DoDangerBools)
            {
                return true;
            }
            if (__instance.isChallengeFile)
            {
                TimeOfDay.Instance.globalTime = 100f;
                __instance.SetMapScreenInfoToCurrentLevel();
                return false;
            }
            float num = 980 - CentralConfig.shid;
            if (LevelManager.CurrentExtendedLevel.NumberlessPlanetName != "Gordion" || TimeOfDay.Instance.daysUntilDeadline <= 0)
            {
                TimeOfDay.Instance.timeUntilDeadline -= num;
                // CentralConfig.instance.mls.LogInfo("timeUntilDeadline adjustment was: " + num);
                TimeOfDay.Instance.OnDayChanged();
            }
            TimeOfDay.Instance.globalTime = 100f;
            TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
            if (LevelManager.CurrentExtendedLevel.NumberlessPlanetName != "Gordion" || TimeOfDay.Instance.daysUntilDeadline <= 0)
            {
                HUDManager.Instance.DisplayDaysLeft((int)Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime));
            }
            UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
            __instance.SetMapScreenInfoToCurrentLevel();
            if (TimeOfDay.Instance.timeUntilDeadline > 0f && TimeOfDay.Instance.daysUntilDeadline <= 0 && TimeOfDay.Instance.timesFulfilledQuota <= 0)
            {
                __instance.StartCoroutine(playDaysLeftAlertSFXDelayed());
            }
            CentralConfig.shid = 0;
            return false;
        }
        public static IEnumerator playDaysLeftAlertSFXDelayed()
        {
            yield return new WaitForSeconds(3f);
            StartOfRound.Instance.speakerAudioSource.PlayOneShot(StartOfRound.Instance.zeroDaysLeftAlertSFX);
        }
    }
    public class Ororo
    {
        public void SetSinglePlanetWeather(ExtendedLevel level)
        {
            string currentMoon = level.NumberlessPlanetName;
            string WeatherStr = WaitForMoonsToRegister.CreateMoonConfig.WeatherTypeOverride[currentMoon];
            RandomWeatherWithVariables[] PossibleWeathers = ConfigAider.ConvertStringToWeatherArray(WeatherStr);

            System.Random rand = new System.Random(StartOfRound.Instance.randomMapSeed + 69);
            if (PossibleWeathers != null && PossibleWeathers.Length != 0)
            {
                level.SelectableLevel.currentWeather = PossibleWeathers[rand.Next(0, PossibleWeathers.Length)].weatherType;
            }
            else
            {
                level.SelectableLevel.currentWeather = LevelWeatherType.None;
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
    public static class ShowIntEnemyCount
    {
        public static int IntEnemiesSpawned = 0;
        static void Postfix(RoundManager __instance)
        {
            float fakenum = __instance.currentLevel.enemySpawnChanceThroughoutDay.Evaluate(__instance.timeScript.currentDayTime / __instance.timeScript.totalTime);
            float fakenum2 = fakenum + (float)Mathf.Abs(TimeOfDay.Instance.daysUntilDeadline - 3) / 1.6f;
            int fakevalue = Mathf.Clamp(__instance.AnomalyRandom.Next((int)(fakenum2 - __instance.currentLevel.spawnProbabilityRange), (int)(fakenum + __instance.currentLevel.spawnProbabilityRange)), __instance.minEnemiesToSpawn, 20);
            fakevalue = Mathf.Clamp(fakevalue, 0, __instance.allEnemyVents.Length);

            IntEnemiesSpawned += fakevalue;
            CentralConfig.instance.mls.LogInfo("There are now " + IntEnemiesSpawned + " interior enemies");
        }
    }
    [HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
    public static class MoarEnemies1
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.opcode == OpCodes.Ldc_I4_S && (sbyte)inst.operand == 20)
                {
                    if (CentralConfig.SyncConfig.FreeEnemies)
                    {
                        inst.operand = (sbyte)127;
                    }
                }
                yield return inst;
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnDaytimeEnemiesOutside")]
    public static class MoarEnemies2
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.opcode == OpCodes.Ldc_I4_S && (sbyte)inst.operand == 20)
                {
                    if (CentralConfig.SyncConfig.FreeEnemies)
                    {
                        inst.operand = (sbyte)127;
                    }
                }
                yield return inst;
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnEnemiesOutside")]
    public static class MoarEnemies3
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.opcode == OpCodes.Ldc_I4_S && (sbyte)inst.operand == 20)
                {
                    if (CentralConfig.SyncConfig.FreeEnemies)
                    {
                        inst.operand = (sbyte)127;
                    }
                }
                yield return inst;
            }
        }
    }
}