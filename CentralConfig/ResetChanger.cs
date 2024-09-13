using LethalLevelLoader;
using System.Collections.Generic;
using HarmonyLib;
using static CentralConfig.ScrapShuffler;
using static CentralConfig.EnemyShuffler;
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
            static void Postfix()
            {
                List<ExtendedLevel> AllLevels = PatchedContent.ExtendedLevels;
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
                ScrapAppearances.Clear();
                EnemyAppearances.Clear();
                DidSpawnYet.Clear();
                if (NetworkManager.Singleton.IsHost && CentralConfig.SyncConfig.ShuffleSave) // save data again on dc
                {
                    if (CentralConfig.SyncConfig.ScrapShuffle)
                    {
                        ES3.Save("ScrapAppearanceString", ScrapAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                    }
                    if (CentralConfig.SyncConfig.EnemyShuffle)
                    {
                        ES3.Save("EnemyAppearanceString", EnemyAppearanceString, GameNetworkManager.Instance.currentSaveFileName);
                    }
                }
                ScrapAppearanceString.Clear();
                EnemyAppearanceString.Clear();
                IncreaseScrapAppearances.CapturedScrapToSpawn.Clear();
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
                if (!CentralConfig.SyncConfig.ScrapShuffle)
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
                            // CentralConfig.instance.mls.LogInfo($"Remembered saved Item Key: {item.itemName}, Days: {ScrapAppearances[item]}");
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
                if (!CentralConfig.SyncConfig.EnemyShuffle)
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
                            // CentralConfig.instance.mls.LogInfo($"Remembered saved Enemy Key: {enemy.enemyType.enemyName}, Days: {EnemyAppearances[enemy.enemyType]}");
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
                foreach (SpawnableEnemyWithRarity enemy in LevelManager.CurrentExtendedLevel.SelectableLevel.Enemies.Concat(LevelManager.CurrentExtendedLevel.SelectableLevel.DaytimeEnemies.Concat(LevelManager.CurrentExtendedLevel.SelectableLevel.OutsideEnemies)))
                {
                    if (!CentralConfig.SyncConfig.EnemyShuffle)
                    {
                        return;
                    }

                    if (!DidSpawnYet.ContainsKey(enemy.enemyType))
                    {
                        DidSpawnYet.Add(enemy.enemyType, false);
                    }

                    if (!EnemyAppearances.ContainsKey(enemy.enemyType)) // enemy hasn't been saved to the dict this session
                    {
                        if (EnemyAppearanceString.ContainsKey(enemy.enemyType.enemyName)) // the string version has it (save data)
                        {
                            EnemyAppearances.Add(enemy.enemyType, EnemyAppearanceString[enemy.enemyType.enemyName]); // creates the dict from the saved data
                            // CentralConfig.instance.mls.LogInfo($"Remembered saved Enemy Key: {enemy.enemyType.enemyName}, Days: {EnemyAppearances[enemy.enemyType]}");
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
    public static class ShuffleSaver
    {
        public static Dictionary<string, int> ScrapAppearanceString = new Dictionary<string, int>();
        public static Dictionary<string, int> EnemyAppearanceString = new Dictionary<string, int>();
        [HarmonyPatch(typeof(StartOfRound), "PassTimeToNextDay")]
        public static class SaveShuffleDataStrings
        {
            static void Postfix()
            {
                if (!CentralConfig.SyncConfig.ShuffleSave)
                {
                    return;
                }
                if (NetworkManager.Singleton.IsHost)
                {
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
                }
            }
        }
    }
}