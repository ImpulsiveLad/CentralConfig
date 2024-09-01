using LethalLevelLoader;
using System.Collections.Generic;
using HarmonyLib;

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
            CentralConfig.instance.mls.LogInfo("Saved OG enemy/scrap lists for all moons.");
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
                CentralConfig.instance.mls.LogInfo("Reset enemy/scrap lists for all moons.");
            }
        }
    }
}
