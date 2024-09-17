using LethalLevelLoader;
using System.Collections.Generic;
using HarmonyLib;
using static CentralConfig.ScrapShuffler;
using static CentralConfig.EnemyShuffler;
using static CentralConfig.DungeonShuffler;
using static CentralConfig.ShuffleSaver;
using System.Linq;
using Unity.Netcode;

namespace CentralConfig
{
    public static class ResetChanger
    {
        public static Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>> OGIndoorEnemies = new Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>>();
        public static Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>> OGDayEnemies = new Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>>();
        public static Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>> OGNightEnemies = new Dictionary<ExtendedLevel, List<SpawnableEnemyWithRarity>>();
        public static Dictionary<ExtendedLevel, List<SpawnableItemWithRarity>> OGScrapPool = new Dictionary<ExtendedLevel, List<SpawnableItemWithRarity>>();
        public static void SavePlanetData()
        {
            List<ExtendedLevel> AllLevels = PatchedContent.ExtendedLevels;
            foreach (ExtendedLevel level in AllLevels)
            {
                OGIndoorEnemies[level] = level.SelectableLevel.Enemies;
                OGDayEnemies[level] = level.SelectableLevel.DaytimeEnemies;
                OGNightEnemies[level] = level.SelectableLevel.OutsideEnemies;
                OGScrapPool[level] = level.SelectableLevel.spawnableScrap;
            }
            if (CentralConfig.HarmonyTouch5)
            {
                CentralConfig.instance.mls.LogInfo("Saved OG enemy/scrap lists for all moons.");
            }
            CentralConfig.HarmonyTouch5 = true;
        }
        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        public static class ResetOnDisconnect
        {
            public static List<ExtendedDungeonFlow> AllDungeons = new List<ExtendedDungeonFlow>();
            public static List<ExtendedLevel> AllLevels = new List<ExtendedLevel>();
            static void Postfix()
            {
                foreach (ExtendedLevel level in AllLevels)
                {
                    level.SelectableLevel.Enemies = OGIndoorEnemies[level];
                    level.SelectableLevel.DaytimeEnemies = OGDayEnemies[level];
                    level.SelectableLevel.OutsideEnemies = OGNightEnemies[level];
                    level.SelectableLevel.spawnableScrap = OGScrapPool[level];
                }
                OGIndoorEnemies.Clear();
                OGDayEnemies.Clear();
                OGNightEnemies.Clear();
                OGNightEnemies.Clear();
                AllLevels.Clear();

                if (CentralConfig.SyncConfig.ScrapShuffle)
                {
                    ScrapAppearances.Clear();
                    IncreaseScrapAppearances.CapturedScrapToSpawn.Clear();
                }
                if (CentralConfig.SyncConfig.EnemyShuffle)
                {
                    EnemyAppearances.Clear();
                    DidSpawnYet.Clear();
                }
                if (CentralConfig.SyncConfig.DungeonShuffler)
                {
                    lastdungeon = null;
                    DungeonAppearances.Clear();
                    lastpossibledungeons.Clear();
                }

                if (NetworkManager.Singleton.IsHost && CentralConfig.SyncConfig.ShuffleSave) // save data again on dc
                {
                    if (CentralConfig.SyncConfig.ScrapShuffle)
                    {
                        ES3.Save("ScrapAppearanceString", ScrapAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                        ScrapAppearanceString.Clear();
                    }
                    if (CentralConfig.SyncConfig.EnemyShuffle)
                    {
                        ES3.Save("EnemyAppearanceString", EnemyAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                        EnemyAppearanceString.Clear();
                    }
                    if (CentralConfig.SyncConfig.DungeonShuffler)
                    {
                        ES3.Save("DungeonAppearanceString", DungeonAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                        DungeonAppearanceString.Clear();
                        if (LastGlorp != -1)
                        {
                            ES3.Save("LastGlorp", LastGlorp, GameNetworkManager.Instance.currentSaveFileName);
                            LastGlorp = -1;
                        }
                    }
                }

                if (CentralConfig.SyncConfig.DungeonShuffler && NetworkManager.Singleton.IsHost)
                {
                    foreach (ExtendedDungeonFlow flow in AllDungeons)
                    {
                        flow.LevelMatchingProperties.planetNames = DungeonMoonMatches[flow];
                        flow.LevelMatchingProperties.modNames = DungeonModMatches[flow];
                        flow.LevelMatchingProperties.levelTags = DungeonTagMatches[flow];
                        flow.LevelMatchingProperties.currentRoutePrice = DungeonRouteMatches[flow];
                    }
                    DungeonMoonMatches.Clear();
                    DungeonModMatches.Clear();
                    DungeonTagMatches.Clear();
                    DungeonRouteMatches.Clear();
                    AllDungeons.Clear();
                }

                CentralConfig.instance.mls.LogInfo("Reset enemy/scrap lists for all moons.");
            }
        }
    }
    public static class ScrapShuffler
    {
        public static Dictionary<Item, int> ScrapAppearances = new Dictionary<Item, int>();

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        public static class IncreaseScrapAppearances
        {
            public static List<Item> CapturedScrapToSpawn = new List<Item>();
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.ScrapShuffle || !NetworkManager.Singleton.IsHost)
                {
                    return;
                }

                CapturedScrapToSpawn.Clear();

                List<GrabbableObject> ScrapInLevel = UnityEngine.Object.FindObjectsOfType<GrabbableObject>().ToList();
                foreach (GrabbableObject obj in ScrapInLevel)
                {
                    if (obj.isInFactory && !obj.isInShipRoom && !obj.hasBeenHeld)
                    {
                        CapturedScrapToSpawn.Add(obj.itemProperties);
                    }
                }

                foreach (SpawnableItemWithRarity spawnableItemWithRarity in LevelManager.CurrentExtendedLevel.SelectableLevel.spawnableScrap)
                {
                    Item item = spawnableItemWithRarity.spawnableItem;

                    if (!ScrapAppearances.ContainsKey(item))
                    {
                        if (ScrapAppearanceString.ContainsKey(item.itemName))
                        {
                            ScrapAppearances.Add(item, ScrapAppearanceString[item.itemName]);
                            CentralConfig.instance.mls.LogInfo($"Remembered saved Item Key: {item.itemName}, Days: {ScrapAppearances[item]}");
                        }
                        else
                        {
                            ScrapAppearances.Add(item, 0);
                            ScrapAppearanceString.Add(item.itemName, 0);
                            // CentralConfig.instance.mls.LogInfo($"Added new Item Key: {item.itemName}");
                        }
                    }
                    if (!ScrapAppearanceString.ContainsKey(item.itemName))
                    {
                        ScrapAppearanceString.Add(item.itemName, ScrapAppearances[item]);
                    }

                    if (CapturedScrapToSpawn.Contains(item))
                    {
                        ScrapAppearances[item] = 0;
                        ScrapAppearanceString[item.itemName] = 0;
                        // CentralConfig.instance.mls.LogInfo($"Scrap: {item.itemName} was spawned, resetting days since last appearance to 0.");
                    }
                    else
                    {
                        ScrapAppearances[item]++;
                        ScrapAppearanceString[item.itemName]++;
                        // CentralConfig.instance.mls.LogInfo($"Scrap: {item.itemName} did not spawn, increasing days since last appearance to {ScrapAppearances[item]}.");
                    }
                }
            }
        }
    }
    public static class EnemyShuffler
    {
        public static Dictionary<EnemyType, int> EnemyAppearances = new Dictionary<EnemyType, int>();
        public static Dictionary<EnemyType, bool> DidSpawnYet = new Dictionary<EnemyType, bool>();

        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        public static class CheckForEnemySpawns
        {
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.EnemyShuffle || !NetworkManager.Singleton.IsHost)
                {
                    return;
                }

                List<EnemyAI> SpawnedEnemies = UnityEngine.Object.FindObjectsOfType<EnemyAI>().ToList();
                foreach (EnemyAI enemy in SpawnedEnemies)
                {
                    if (!DidSpawnYet.ContainsKey(enemy.enemyType))
                    {
                        DidSpawnYet.Add(enemy.enemyType, false);
                    }

                    if (!EnemyAppearances.ContainsKey(enemy.enemyType))
                    {
                        if (EnemyAppearanceString.ContainsKey(enemy.enemyType.enemyName))
                        {
                            EnemyAppearances.Add(enemy.enemyType, EnemyAppearanceString[enemy.enemyType.enemyName]);
                            CentralConfig.instance.mls.LogInfo($"Remembered saved Enemy Key: {enemy.enemyType.enemyName}, Days: {EnemyAppearances[enemy.enemyType]}");
                        }
                        else
                        {
                            EnemyAppearances.Add(enemy.enemyType, 0);
                            EnemyAppearanceString.Add(enemy.enemyType.enemyName, 0);
                            // CentralConfig.instance.mls.LogInfo($"Added new Enemy Key: {enemy.enemyType.enemyName}");
                        }
                    }
                    if (!EnemyAppearanceString.ContainsKey(enemy.enemyType.enemyName))
                    {
                        EnemyAppearanceString.Add(enemy.enemyType.enemyName, EnemyAppearances[enemy.enemyType]);
                    }

                    if (!DidSpawnYet[enemy.enemyType])
                    {
                        DidSpawnYet[enemy.enemyType] = true;
                        // CentralConfig.instance.mls.LogInfo($"Enemy: {enemy.enemyType.enemyName} has spawned.");
                    }
                }
            }
        }
        [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
        public static class UpdateEnemyDictionary
        {
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.EnemyShuffle || !NetworkManager.Singleton.IsHost)
                {
                    return;
                }

                foreach (SpawnableEnemyWithRarity enemy in LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies.Concat(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies.Concat(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies)))
                {
                    if (!DidSpawnYet.ContainsKey(enemy.enemyType))
                    {
                        DidSpawnYet.Add(enemy.enemyType, false);
                    }

                    if (!EnemyAppearances.ContainsKey(enemy.enemyType)) // enemy hasn't been saved to the dict this session
                    {
                        if (EnemyAppearanceString.ContainsKey(enemy.enemyType.enemyName)) // the string version has it (save data)
                        {
                            EnemyAppearances.Add(enemy.enemyType, EnemyAppearanceString[enemy.enemyType.enemyName]); // creates the dict from the saved data
                            CentralConfig.instance.mls.LogInfo($"Remembered saved Enemy Key: {enemy.enemyType.enemyName}, Days: {EnemyAppearances[enemy.enemyType]}");
                        }
                        else // not in the string version (no save data)
                        {
                            EnemyAppearances.Add(enemy.enemyType, 0); // registers it new
                            EnemyAppearanceString.Add(enemy.enemyType.enemyName, 0); // also creates the string version to be saved
                            // CentralConfig.instance.mls.LogInfo($"Added new Enemy Key: {enemy.enemyType.enemyName}");
                        }
                    }
                    if (!EnemyAppearanceString.ContainsKey(enemy.enemyType.enemyName))
                    {
                        EnemyAppearanceString.Add(enemy.enemyType.enemyName, EnemyAppearances[enemy.enemyType]);
                    }

                    if (!DidSpawnYet[enemy.enemyType])
                    {
                        EnemyAppearances[enemy.enemyType]++; // when this updates
                        EnemyAppearanceString[enemy.enemyType.enemyName]++; // this does
                        // CentralConfig.instance.mls.LogInfo($"Enemy: {enemy.enemyType.enemyName} did not spawn, increasing days since last appearance to {EnemyAppearances[enemy.enemyType]}.");
                    }
                    else
                    {
                        EnemyAppearances[enemy.enemyType] = 0;
                        EnemyAppearanceString[enemy.enemyType.enemyName] = 0;
                        // CentralConfig.instance.mls.LogInfo($"Enemy: {enemy.enemyType.enemyName} was spawned, resetting days since last appearance to 0.");
                    }
                }
                DidSpawnYet.Clear();
            }
        }
    }
    public static class DungeonShuffler
    {
        public static Dictionary<ExtendedDungeonFlow, int> DungeonAppearances = new Dictionary<ExtendedDungeonFlow, int>();
        public static Dictionary<ExtendedDungeonFlow, int> IncreaseIterations = new Dictionary<ExtendedDungeonFlow, int>();
        public static ExtendedDungeonFlow lastdungeon = null;
        public static List<ExtendedDungeonFlowWithRarity> lastpossibledungeons = new List<ExtendedDungeonFlowWithRarity>();

        [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
        public static class UpdateDungeonDictionary
        {
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.DungeonShuffler || !NetworkManager.Singleton.IsHost)
                {
                    return;
                }

                if (lastdungeon != null && lastpossibledungeons.Count != 0)
                {
                    foreach (ExtendedDungeonFlowWithRarity flow in lastpossibledungeons)
                    {
                        string Dun = flow.extendedDungeonFlow.DungeonName + " (" + flow.extendedDungeonFlow.name + ")";
                        Dun = Dun.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
                        string DungeonName = Dun.Replace("ExtendedDungeonFlow", "").Replace("Level", "");

                        if (!DungeonAppearances.ContainsKey(flow.extendedDungeonFlow))
                        {
                            if (DungeonAppearanceString.ContainsKey(DungeonName))
                            {
                                DungeonAppearances.Add(flow.extendedDungeonFlow, DungeonAppearanceString[DungeonName]);
                                CentralConfig.instance.mls.LogInfo($"Remembered saved Dungeon Key: {DungeonName}, Days: {DungeonAppearances[flow.extendedDungeonFlow]}");
                            }
                            else
                            {
                                DungeonAppearances.Add(flow.extendedDungeonFlow, 0);
                                DungeonAppearanceString.Add(DungeonName, 0);
                                // CentralConfig.instance.mls.LogInfo($"Added new Dungeon Key: {DungeonName}");
                            }
                        }
                        if (!DungeonAppearanceString.ContainsKey(DungeonName))
                        {
                            DungeonAppearanceString.Add(DungeonName, DungeonAppearances[flow.extendedDungeonFlow]);
                        }

                        if (flow.extendedDungeonFlow == lastdungeon)
                        {
                            DungeonAppearances[flow.extendedDungeonFlow] = 0;
                            DungeonAppearanceString[DungeonName] = 0;
                            // CentralConfig.instance.mls.LogInfo($"Dungeon: {DungeonName} was selected, resetting days since last appearance to 0.");
                        }
                        else
                        {
                            DungeonAppearances[flow.extendedDungeonFlow]++;
                            DungeonAppearanceString[DungeonName]++;
                            // CentralConfig.instance.mls.LogInfo($"Dungeon: {DungeonName} was not selected, increasing days since last appearance to {DungeonAppearances[flow.extendedDungeonFlow]}.");
                        }
                    }
                    lastdungeon = null;
                    lastpossibledungeons.Clear();
                    foreach (ExtendedDungeonFlow flow in PatchedContent.ExtendedDungeonFlows)
                    {
                        flow.LevelMatchingProperties.planetNames = DungeonMoonMatches[flow];
                        flow.LevelMatchingProperties.modNames = DungeonModMatches[flow];
                        flow.LevelMatchingProperties.levelTags = DungeonTagMatches[flow];
                        flow.LevelMatchingProperties.currentRoutePrice = DungeonRouteMatches[flow];

                        string gen = flow.DungeonName + " (" + flow.name + ")";
                        gen = gen.Replace("13Exits", "3Exits").Replace("1ExtraLarge", "ExtraLarge");
                        string FlowName = gen.Replace("ExtendedDungeonFlow", "").Replace("Level", "");

                        dungeonrandom = new System.Random(StartOfRound.Instance.randomMapSeed);

                        flow.LevelMatchingProperties.planetNames = ConfigAider.IncreaseDungeonRarities(flow.LevelMatchingProperties.planetNames, flow, FlowName);
                        flow.LevelMatchingProperties.modNames = ConfigAider.IncreaseDungeonRarities(flow.LevelMatchingProperties.modNames, flow, FlowName);
                        flow.LevelMatchingProperties.levelTags = ConfigAider.IncreaseDungeonRarities(flow.LevelMatchingProperties.levelTags, flow, FlowName);
                        flow.LevelMatchingProperties.currentRoutePrice = ConfigAider.IncreaseDungeonRaritiesVector2(flow.LevelMatchingProperties.currentRoutePrice, flow, FlowName);
                    }
                    LastGlorp = StartOfRound.Instance.randomMapSeed;
                }
                else
                {
                    CentralConfig.instance.mls.LogInfo("LastMatchDungeonData is null");
                }
            }
        }
    }
    public static class ShuffleSaver
    {
        public static Dictionary<string, int> ScrapAppearanceString = new Dictionary<string, int>();
        public static Dictionary<string, int> EnemyAppearanceString = new Dictionary<string, int>();
        public static Dictionary<string, int> DungeonAppearanceString = new Dictionary<string, int>();
        public static Dictionary<ExtendedDungeonFlow, List<StringWithRarity>> DungeonMoonMatches = new Dictionary<ExtendedDungeonFlow, List<StringWithRarity>>();
        public static Dictionary<ExtendedDungeonFlow, List<StringWithRarity>> DungeonModMatches = new Dictionary<ExtendedDungeonFlow, List<StringWithRarity>>();
        public static Dictionary<ExtendedDungeonFlow, List<StringWithRarity>> DungeonTagMatches = new Dictionary<ExtendedDungeonFlow, List<StringWithRarity>>();
        public static Dictionary<ExtendedDungeonFlow, List<Vector2WithRarity>> DungeonRouteMatches = new Dictionary<ExtendedDungeonFlow, List<Vector2WithRarity>>();
        public static int LastGlorp = -1;
        public static System.Random dungeonrandom;
        public static System.Random enemyrandom;
        public static System.Random scraprandom;

        [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
        public static class SaveShuffleDataStrings
        {
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.ShuffleSave || !NetworkManager.Singleton.IsHost)
                {
                    return;
                }

                if (CentralConfig.SyncConfig.ScrapShuffle)
                {
                    ES3.Save("ScrapAppearanceString", ScrapAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                    CentralConfig.instance.mls.LogInfo("Saved Scrap Shuffle Data");
                }
                if (CentralConfig.SyncConfig.EnemyShuffle)
                {
                    ES3.Save("EnemyAppearanceString", EnemyAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                    CentralConfig.instance.mls.LogInfo("Saved Enemy Shuffle Data");
                }
                if (CentralConfig.SyncConfig.DungeonShuffler)
                {
                    ES3.Save("DungeonAppearanceString", DungeonAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                    if (LastGlorp != -1)
                    {
                        ES3.Save("LastGlorp", LastGlorp, GameNetworkManager.Instance.currentSaveFileName);
                    }
                    CentralConfig.instance.mls.LogInfo("Saved Dungeon Shuffle Data");
                }
            }
        }
    }
}