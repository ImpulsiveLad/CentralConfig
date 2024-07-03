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

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class WaitForTagsToRegister
    {
        public static CreateTagConfig Config;

        [DataContract]
        public class CreateTagConfig : ConfigTemplate
        {
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyByTag;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> InteriorEnemies;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> InteriorEnemyReplacement;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayTimeEnemyByTag;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> DayEnemies;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> DayEnemyReplacement;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightTimeEnemyByTag;
            [DataMember] public static Dictionary<string, List<SpawnableEnemyWithRarity>> NightEnemies;
            [DataMember] public static Dictionary<string, SyncedEntry<string>> NightEnemyReplacement;

            [DataMember] public static Dictionary<string, SyncedEntry<string>> ScrapByTag;
            [DataMember] public static Dictionary<string, List<SpawnableItemWithRarity>> Scrap;

            public CreateTagConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                InteriorEnemyByTag = new Dictionary<string, SyncedEntry<string>>();
                InteriorEnemies = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                InteriorEnemyReplacement = new Dictionary<string, SyncedEntry<string>>();
                DayTimeEnemyByTag = new Dictionary<string, SyncedEntry<string>>();
                DayEnemies = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                DayEnemyReplacement = new Dictionary<string, SyncedEntry<string>>();
                NightTimeEnemyByTag = new Dictionary<string, SyncedEntry<string>>();
                NightEnemies = new Dictionary<string, List<SpawnableEnemyWithRarity>>();
                NightEnemyReplacement = new Dictionary<string, SyncedEntry<string>>();

                ScrapByTag = new Dictionary<string, SyncedEntry<string>>();
                Scrap = new Dictionary<string, List<SpawnableItemWithRarity>>();

                List<ContentTag> AllContentTags;
                List<ContentTag> allcontenttagslist = ConfigAider.GrabFullTagList();
                string ignoreList = CentralConfig.SyncConfig.BlacklistTags.Value;

                if (CentralConfig.SyncConfig.IsTagWhiteList)
                {
                    AllContentTags = allcontenttagslist.Where(tag => ignoreList.Split(',').Any(b => tag.contentTagName.Equals(b))).ToList();
                }
                else
                {
                    AllContentTags = allcontenttagslist.Where(tag => !ignoreList.Split(',').Any(b => tag.contentTagName.Equals(b))).ToList();
                }
                foreach (ContentTag tag in AllContentTags)
                {
                    string TagName = tag.contentTagName.Trim();

                    if (CentralConfig.SyncConfig.DoEnemyTagInjections)
                    {
                        InteriorEnemyByTag[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Add Interior Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the interior enemy list on any moons with this tag.");

                        InteriorEnemyReplacement[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Replace Interior Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Flowerman:Plantman,Crawler:Mauler\",\nOn any moons with this tag, brackens will be replaced with hypothetical plantmen, and crawlers with hypothetical maulers.\nThe main use would be biomatic enemies, This runs before the above entry adds new enemies, and between weather and dungeon adding enemies.");

                        DayTimeEnemyByTag[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Add Day Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the day enemy list on any moons with this tag.");

                        DayEnemyReplacement[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Replace Day Enemies",
                            "Default Values Were Empty",
                            "In the example, \"Manticoil:Mantisoil,Docile Locust Bees:Angry Moth Wasps\",\nOn any moons with this tag, manticoils will be replaced with hypothetical mantisoils, and docile locust bees with hypothetical angry moth wasps.\nThe main use would be biomatic enemies, This runs before the above entry adds new enemies, and between weather and dungeon adding enemies.");

                        NightTimeEnemyByTag[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Add Night Enemies",
                            "Default Values Were Empty",
                            "Enemies listed here in the EnemyName:rarity,EnemyName:rarity format will be added to the night enemy list on any moons with this tag.");

                        NightEnemyReplacement[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Replace Night Enemies",
                            "Default Values Were Empty",
                            "In the example, \"MouthDog:OceanDog,ForestGiant:FireGiant\",\nOn any moons with this tag, mouthdogs will be replaced with hypothetical oceandogs, and forestgiants with hypothetical firegiants.\nThe main use would be biomatic enemies, This runs before the above entry adds new enemies, and between weather and dungeon adding enemies.");
                    }

                    if (CentralConfig.SyncConfig.DoScrapTagInjections)
                    {
                        ScrapByTag[TagName] = cfg.BindSyncedEntry("Tag: " + TagName,
                            TagName + " - Add Scrap",
                            "Default Values Were Empty",
                            "Scrap listed here in the ScrapName:rarity,ScrapName,rarity format will be added to the scrap list on any moons with this tag.");
                    }
                }
                CentralConfig.instance.mls.LogInfo("Tag config has been registered.");
            }
        }
        static void Prefix()
        {
            CentralConfig.ConfigFile4 = new CreateTagConfig(CentralConfig.instance.Config);
        }
    }
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPriority(100)]
    public class FrApplyTag
    {
        static void Postfix()
        {
            ApplyTagConfig applyConfig = new ApplyTagConfig();
            applyConfig.UpdateTagData();
        }
    }
    public class ApplyTagConfig
    {
        public static bool Ready = false;
        public void UpdateTagData()
        {
            List<ContentTag> AllContentTags;
            List<ContentTag> allcontenttagslist = ConfigAider.GrabFullTagList();
            string ignoreList = CentralConfig.SyncConfig.BlacklistTags.Value;

            if (CentralConfig.SyncConfig.IsTagWhiteList)
            {
                AllContentTags = allcontenttagslist.Where(tag => ignoreList.Split(',').Any(b => tag.contentTagName.Equals(b))).ToList();
            }
            else
            {
                AllContentTags = allcontenttagslist.Where(tag => !ignoreList.Split(',').Any(b => tag.contentTagName.Equals(b))).ToList();
            }
            foreach (ContentTag tag in AllContentTags)
            {
                string TagName = tag.contentTagName.Trim();

                if (CentralConfig.SyncConfig.DoEnemyTagInjections)
                {
                    string IntEneStr = WaitForTagsToRegister.CreateTagConfig.InteriorEnemyByTag[TagName];
                    Vector2 clampIntRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> interiorenemyList = ConfigAider.ConvertStringToEnemyList(IntEneStr, clampIntRarity);
                    WaitForTagsToRegister.CreateTagConfig.InteriorEnemies[TagName] = interiorenemyList;

                    string DayEneStr = WaitForTagsToRegister.CreateTagConfig.DayTimeEnemyByTag[TagName];
                    Vector2 clampDayRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> dayenemyList = ConfigAider.ConvertStringToEnemyList(DayEneStr, clampDayRarity);
                    WaitForTagsToRegister.CreateTagConfig.DayEnemies[TagName] = dayenemyList;

                    string NightEneStr = WaitForTagsToRegister.CreateTagConfig.NightTimeEnemyByTag[TagName];
                    Vector2 clampNightRarity = new Vector2(0, 99999);
                    List<SpawnableEnemyWithRarity> nightenemyList = ConfigAider.ConvertStringToEnemyList(NightEneStr, clampNightRarity);
                    WaitForTagsToRegister.CreateTagConfig.NightEnemies[TagName] = nightenemyList;
                }

                if (CentralConfig.SyncConfig.DoScrapTagInjections)
                {
                    string ScrStr = WaitForTagsToRegister.CreateTagConfig.ScrapByTag[TagName];
                    Vector2 clampScrRarity = new Vector2(0, 99999);
                    List<SpawnableItemWithRarity> scraplist = ConfigAider.ConvertStringToItemList(ScrStr, clampScrRarity);
                    WaitForTagsToRegister.CreateTagConfig.Scrap[TagName] = scraplist;
                }
            }
            CentralConfig.instance.mls.LogInfo("Tag config Values Applied.");
            Ready = true;
        }
    }
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    [HarmonyPriority(666)]
    public class EnactTagInjections
    {
        static void Prefix()
        {
            List<ContentTag> ThisLevelTags = LevelManager.CurrentExtendedLevel.ContentTags;
            foreach (ContentTag tag in ThisLevelTags)
            {
                string TagName = tag.contentTagName.Trim();

                if (CentralConfig.SyncConfig.DoEnemyTagInjections)
                {
                    if (WaitForTagsToRegister.CreateTagConfig.InteriorEnemyReplacement.ContainsKey(TagName))
                    {
                        LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies, WaitForTagsToRegister.CreateTagConfig.InteriorEnemyReplacement[TagName]);
                        if (WaitForTagsToRegister.CreateTagConfig.InteriorEnemies[TagName].Count > 0)
                        {
                            LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies = LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies.Concat(WaitForTagsToRegister.CreateTagConfig.InteriorEnemies[TagName]).ToList();
                        }
                        LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies, WaitForTagsToRegister.CreateTagConfig.DayEnemyReplacement[TagName]);
                        if (WaitForTagsToRegister.CreateTagConfig.DayEnemies[TagName].Count > 0)
                        {
                            LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies.Concat(WaitForTagsToRegister.CreateTagConfig.DayEnemies[TagName]).ToList();
                        }
                        LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = ConfigAider.ReplaceEnemies(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies, WaitForTagsToRegister.CreateTagConfig.NightEnemyReplacement[TagName]);
                        if (WaitForTagsToRegister.CreateTagConfig.NightEnemies[TagName].Count > 0)
                        {
                            LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies = LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies.Concat(WaitForTagsToRegister.CreateTagConfig.NightEnemies[TagName]).ToList();
                        }
                    }
                }

                if (CentralConfig.SyncConfig.DoScrapTagInjections)
                {
                    if (WaitForTagsToRegister.CreateTagConfig.Scrap.ContainsKey(TagName))
                    {
                        if (WaitForTagsToRegister.CreateTagConfig.Scrap[TagName].Count > 0)
                        {
                            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap.Concat(WaitForTagsToRegister.CreateTagConfig.Scrap[TagName]).ToList();
                            LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap = ConfigAider.RemoveLowerRarityDuplicateItems(LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap);
                        }
                    }
                }
            }
            CentralConfig.instance.mls.LogInfo("Tag Enemy/Scrap Injections Enacted.");
        }
    }
}


