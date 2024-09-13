using System.Collections.Generic;
using System.Runtime.Serialization;
using CSync.Lib;
using HarmonyLib;
using LethalLevelLoader.Tools;
using BepInEx.Configuration;
using LethalLevelLoader;
using System.Linq;
using CSync.Extensions;
using UnityEngine;
using System;
using Unity.Netcode;
using System.Data;

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class WaitForWeathersToRegister
    {
        public static CreateWeatherConfig Config;

        [DataContract]
        public class CreateWeatherConfig : ConfigTemplate
        {
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyByWeather;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> InteriorEnemiesW;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyReplacementW;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayTimeEnemyByWeather;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> DayEnemiesW;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayEnemyReplacementW;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightTimeEnemyByWeather;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> NightEnemiesW;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightEnemyReplacementW;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> ScrapByWeather;
            [DataMember] public static Dictionary<string, List<SpawnableItemWithRarity>> ScrapW;

            [DataMember] public static Dictionary<string, SyncedEntry<float>> WeatherScrapAmountMultiplier;
            [DataMember] public static Dictionary<string, SyncedEntry<float>> WeatherScrapValueMultiplier;

            public CreateWeatherConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                InteriorEnemyByWeather = new Dictionary<string, SyncedEntry<string>>();
                InteriorEnemiesW = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                InteriorEnemyReplacementW = new Dictionary<string, SyncedEntry<string>>();
                DayTimeEnemyByWeather = new Dictionary<string, SyncedEntry<string>>();
                DayEnemiesW = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                DayEnemyReplacementW = new Dictionary<string, SyncedEntry<string>>();
                NightTimeEnemyByWeather = new Dictionary<string, SyncedEntry<string>>();
                NightEnemiesW = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                NightEnemyReplacementW = new Dictionary<string, SyncedEntry<string>>();

                ScrapByWeather = new Dictionary<string, SyncedEntry<string>>();
                ScrapW = new Dictionary<string, List<SpawnableItemWithRarity>>();

                WeatherScrapAmountMultiplier = new Dictionary<string, SyncedEntry<float>>();
                WeatherScrapValueMultiplier = new Dictionary<string, SyncedEntry<float>>();

                List<string> AllWeatherTypes = Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>().Select(w => w.ToString()).ToList(); ;
                if (WRCompatibility.enabled)
                {
                    AllWeatherTypes.AddRange(WRCompatibility.GetAllWeathersWithWR());
                }
                string ignoreList = ConfigAider.CauterizeString(CentralConfig.SyncConfig.BlacklistWeathers.Value);

                if (CentralConfig.SyncConfig.IsWeatherWhiteList)
                {
                    AllWeatherTypes = AllWeatherTypes.Where(weatherType => ignoreList.Split(',').Any(b => weatherType.ToString().Equals(b))).ToList();
                }
                else
                {
                    AllWeatherTypes = AllWeatherTypes.Where(weatherType => !ignoreList.Split(',').Any(b => weatherType.ToString().Equals(b))).ToList();
                }
                foreach (string weatherType in AllWeatherTypes)
                {
                    string weatherName = weatherType;

                    if (CentralConfig.SyncConfig.DoEnemyWeatherInjections)
                    {
                        InteriorEnemyByWeather[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Add Interior Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the interior enemy list on any moons currently experiencing this weather.");

                        InteriorEnemyReplacementW[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Replace Interior Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Flowerman:Plantman,Crawler:Mauler\",\nOn any moons currently experiencing this weather, Brackens will be replaced with hypothetical Plantmen, and Crawlers with hypothetical Maulers.\nYou could also use inputs such as \"Flowerman-15:Plantman~50\", this will give the Plantman a rarity of 15 instead of using the Bracken's and it will only have a 50% chance to replace.\nThis runs before the above entry adds new enemies, and before the tags and dungeons add enemies.");

                        DayTimeEnemyByWeather[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Add Day Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the day enemy list on any moons currently experiencing this weather.");

                        DayEnemyReplacementW[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Replace Day Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Manticoil:Mantisoil,Docile Locust Bees:Angry Moth Wasps\",\nOn any moons currently experiencing this weather, Manticoils will be replaced with hypothetical Mantisoils, and docile locust bees with hypothetical angry moth wasps.\nYou could also use inputs such as \"Manticoil-90:Mantisoil\", this will give the Mantisoil a rarity of 90 instead of using the Manticoil's and it will still have a 100% chance to replace.\nThis runs before the above entry adds new enemies, and before the tags and dungeons add enemies.");

                        NightTimeEnemyByWeather[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Add Night Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the night enemy list on any moons currently experiencing this weather.");

                        NightEnemyReplacementW[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Replace Night Enemies",
                            "Default Values Were Empty",
                            "In the example, \"MouthDog:OceanDog,ForestGiant:FireGiant\",\nOn any moons currently experiencing this weather, Mouthdogs will be replaced with hypothetical Oceandogs, and Forest giants with hypothetical Fire giants.\nYou could also use inputs such as \"MouthDog:OceanDog~75\", the OceanDog will still inherit the rarity from the MouthDog but it will only have a 75% chance to replace.\nThis runs before the above entry adds new enemies, and before the tags and dungeons add enemies.");
                    }

                    if (CentralConfig.SyncConfig.DoScrapWeatherInjections)
                    {
                        ScrapByWeather[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Add Scrap",
                            "Default Values Were Empty",
                            "Scrap listed here in the ScrapName:rarity,ScrapName,rarity format will be added to the scrap list on any moons currently experiencing this weather.");

                        WeatherScrapAmountMultiplier[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Scrap Count Multiplier",
                            1f,
                            "Multiplier applied to the # of scrap in the level on any moons currently experiencing this weather.");

                        WeatherScrapValueMultiplier[weatherName] = cfg.BindSyncedEntry("Weather: " + weatherName,
                            weatherName + " - Scrap Value Multiplier",
                            1f,
                            "Multiplier applied to each scrap object's value in the level on any moons currently experiencing this weather.");
                    }
                }
                if (CentralConfig.HarmonyTouch4)
                {
                    if (NetworkManager.Singleton.IsHost && (CentralConfig.SyncConfig.DoScrapWeatherInjections || CentralConfig.SyncConfig.DoEnemyWeatherInjections))
                    {
                        CentralConfig.instance.mls.LogInfo("Weather config has been registered.");
                    }
                }
                CentralConfig.HarmonyTouch4 = true;
            }
        }
        static void Prefix()
        {
            CentralConfig.ConfigFile5 = new CreateWeatherConfig(CentralConfig.instance.Config);
        }
    }
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPriority(110)]
    public class FrApplyWeather
    {
        static void Postfix()
        {
            ApplyWeatherConfig applyConfig = new ApplyWeatherConfig();
            applyConfig.UpdateWeatherData();
        }
    }
    public class ApplyWeatherConfig
    {
        public static bool Ready = false;
        public void UpdateWeatherData()
        {
            List<string> AllWeatherTypes = Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>().Select(w => w.ToString()).ToList(); ;
            if (WRCompatibility.enabled)
            {
                AllWeatherTypes.AddRange(WRCompatibility.GetAllWeathersWithWR());
            }
            string ignoreList = ConfigAider.CauterizeString(CentralConfig.SyncConfig.BlacklistWeathers.Value);

            if (CentralConfig.SyncConfig.IsWeatherWhiteList)
            {
                AllWeatherTypes = AllWeatherTypes.Where(weatherType => ignoreList.Split(',').Any(b => weatherType.ToString().Equals(b))).ToList();
            }
            else
            {
                AllWeatherTypes = AllWeatherTypes.Where(weatherType => !ignoreList.Split(',').Any(b => weatherType.ToString().Equals(b))).ToList();
            }
            foreach (string weatherType in AllWeatherTypes)
            {
                string weatherName = weatherType;

                if (CentralConfig.SyncConfig.DoEnemyWeatherInjections && NetworkManager.Singleton.IsHost)
                {
                    if (WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemyByWeather.ContainsKey(weatherName))
                    {
                        string IntEneStr = WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemyByWeather[weatherName];
                        Vector2 clampIntRarity = new Vector2(-99999, 99999);
                        List<SpawnableEnemyWithRarity> interiorenemyList = ConfigAider.ConvertStringToEnemyList(IntEneStr, clampIntRarity);
                        WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemiesW[weatherName] = interiorenemyList;

                        string DayEneStr = WaitForWeathersToRegister.CreateWeatherConfig.DayTimeEnemyByWeather[weatherName];
                        Vector2 clampDayRarity = new Vector2(-99999, 99999);
                        List<SpawnableEnemyWithRarity> dayenemyList = ConfigAider.ConvertStringToEnemyList(DayEneStr, clampDayRarity);
                        WaitForWeathersToRegister.CreateWeatherConfig.DayEnemiesW[weatherName] = dayenemyList;

                        string NightEneStr = WaitForWeathersToRegister.CreateWeatherConfig.NightTimeEnemyByWeather[weatherName];
                        Vector2 clampNightRarity = new Vector2(-99999, 99999);
                        List<SpawnableEnemyWithRarity> nightenemyList = ConfigAider.ConvertStringToEnemyList(NightEneStr, clampNightRarity);
                        WaitForWeathersToRegister.CreateWeatherConfig.NightEnemiesW[weatherName] = nightenemyList;
                    }
                }

                if (CentralConfig.SyncConfig.DoScrapWeatherInjections && NetworkManager.Singleton.IsHost)
                {
                    if (WaitForWeathersToRegister.CreateWeatherConfig.ScrapByWeather.ContainsKey(weatherName))
                    {
                        string ScrStr = WaitForWeathersToRegister.CreateWeatherConfig.ScrapByWeather[weatherName];
                        Vector2 clampScrRarity = new Vector2(-99999, 99999);
                        List<SpawnableItemWithRarity> scraplist = ConfigAider.ConvertStringToItemList(ScrStr, clampScrRarity);
                        WaitForWeathersToRegister.CreateWeatherConfig.ScrapW[weatherName] = scraplist;
                    }
                }
            }
            if (NetworkManager.Singleton.IsHost && (CentralConfig.SyncConfig.DoScrapWeatherInjections || CentralConfig.SyncConfig.DoEnemyWeatherInjections))
            {
                CentralConfig.instance.mls.LogInfo("Weather config Values Applied.");
            }
            Ready = true;
        }
    }
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    [HarmonyPriority(676)]
    public class EnactWeatherInjections
    {
        static void Prefix()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                return;
            }

            string weatherName = LevelManager.CurrentExtendedLevel.SelectableLevel.currentWeather.ToString();
            // CentralConfig.instance.mls.LogInfo($"{weatherName}");

            if (CentralConfig.SyncConfig.DoEnemyWeatherInjections)
            {
                if (WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemyByWeather.ContainsKey(weatherName))
                {
                    LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies, WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemyReplacementW[weatherName]);
                    if (WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemiesW[weatherName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies.Concat(WaitForWeathersToRegister.CreateWeatherConfig.InteriorEnemiesW[weatherName]).ToList();
                    }
                    LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies, WaitForWeathersToRegister.CreateWeatherConfig.DayEnemyReplacementW[weatherName]);
                    if (WaitForWeathersToRegister.CreateWeatherConfig.DayEnemiesW[weatherName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies.Concat(WaitForWeathersToRegister.CreateWeatherConfig.DayEnemiesW[weatherName]).ToList();
                    }
                    LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies, WaitForWeathersToRegister.CreateWeatherConfig.NightEnemyReplacementW[weatherName]);
                    if (WaitForWeathersToRegister.CreateWeatherConfig.NightEnemiesW[weatherName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies.Concat(WaitForWeathersToRegister.CreateWeatherConfig.NightEnemiesW[weatherName]).ToList();
                    }
                }
            }

            if (CentralConfig.SyncConfig.DoScrapWeatherInjections)
            {
                if (WaitForWeathersToRegister.CreateWeatherConfig.ScrapByWeather.ContainsKey(weatherName))
                {
                    if (WaitForWeathersToRegister.CreateWeatherConfig.ScrapW[weatherName].Count > 0)
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap.Concat(WaitForWeathersToRegister.CreateWeatherConfig.ScrapW[weatherName]).ToList();
                        LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = ConfigAider.RemoveLowerRarityDuplicateItems(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap);
                    }
                }
            }
            if (CentralConfig.SyncConfig.DoEnemyWeatherInjections || CentralConfig.SyncConfig.DoScrapWeatherInjections)
            {
                CentralConfig.instance.mls.LogInfo("Weather Enemy/Scrap Injections Enacted.");
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class ApplyWeatherScrapMultipliers
    {
        static void Prefix(RoundManager __instance)
        {
            if (!CentralConfig.SyncConfig.DoScrapWeatherInjections || !NetworkManager.Singleton.IsHost)
            {
                return;
            }

            string weatherName = LevelManager.CurrentExtendedLevel.SelectableLevel.currentWeather.ToString();

            if (!WeatherScrapData.OriginalMinScrap.ContainsKey(LevelManager.CurrentExtendedLevel) || !WeatherScrapData.OriginalMaxScrap.ContainsKey(LevelManager.CurrentExtendedLevel))
            {
                WeatherScrapData.OriginalMinScrap[LevelManager.CurrentExtendedLevel] = LevelManager.CurrentExtendedLevel.SelectableLevel.minScrap;
                WeatherScrapData.OriginalMaxScrap[LevelManager.CurrentExtendedLevel] = LevelManager.CurrentExtendedLevel.SelectableLevel.maxScrap;
                CentralConfig.instance.mls.LogInfo("Saved original Scrap count/value for: " + LevelManager.CurrentExtendedLevel);
            }

            if (WaitForWeathersToRegister.CreateWeatherConfig.WeatherScrapValueMultiplier.ContainsKey(weatherName))
            {
                LevelManager.CurrentExtendedLevel.SelectableLevel.minScrap = (int)(LevelManager.CurrentExtendedLevel.SelectableLevel.minScrap * WaitForWeathersToRegister.CreateWeatherConfig.WeatherScrapAmountMultiplier[weatherName].Value);
                LevelManager.CurrentExtendedLevel.SelectableLevel.maxScrap = (int)(LevelManager.CurrentExtendedLevel.SelectableLevel.maxScrap * WaitForWeathersToRegister.CreateWeatherConfig.WeatherScrapAmountMultiplier[weatherName].Value);
                CentralConfig.instance.mls.LogInfo("Applied weather Scrap count/value multiplier for: " + LevelManager.CurrentExtendedLevel);
            }
        }
    }
    public static class WeatherScrapData
    {
        public static Dictionary<ExtendedLevel, int> OriginalMinScrap = new Dictionary<ExtendedLevel, int>();
        public static Dictionary<ExtendedLevel, int> OriginalMaxScrap = new Dictionary<ExtendedLevel, int>();
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class ResetMoonsScrapAfterWeather
    {
        static void Prefix()
        {
            if (!CentralConfig.SyncConfig.DoScrapWeatherInjections || !NetworkManager.Singleton.IsHost)
            {
                return;
            }
            if (WeatherScrapData.OriginalMinScrap.ContainsKey(LevelManager.CurrentExtendedLevel) && WeatherScrapData.OriginalMaxScrap.ContainsKey(LevelManager.CurrentExtendedLevel))
            {
                LevelManager.CurrentExtendedLevel.SelectableLevel.minScrap = WeatherScrapData.OriginalMinScrap[LevelManager.CurrentExtendedLevel];
                LevelManager.CurrentExtendedLevel.SelectableLevel.maxScrap = WeatherScrapData.OriginalMaxScrap[LevelManager.CurrentExtendedLevel];
                CentralConfig.instance.mls.LogInfo("Reverted weather applied Scrap count/value for: " + LevelManager.CurrentExtendedLevel);
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class LogScrapValueMultipler
    {
        static void Postfix(RoundManager __instance)
        {
            string PlanetName = LevelManager.CurrentExtendedLevel.NumberlessPlanetName;
            float multiplier = __instance.scrapValueMultiplier * 2.5f;
            CentralConfig.instance.mls.LogInfo(PlanetName + " has a current scrap multiplier of: " + multiplier);
        }
    }
}