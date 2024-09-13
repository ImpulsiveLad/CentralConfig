using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using WeatherRegistry;

namespace CentralConfig
{
    public static class WRCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrov.WeatherRegistry");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<string> GetAllWeathersWithWR()
        {
            List<string> weatherlist = WeatherManager.RegisteredWeathers.Cast<Weather>().Select(w => w.ToString()).ToList();
            for (int i = 0; i < weatherlist.Count; i++)
            {
                weatherlist[i] = weatherlist[i].Replace(" (WeatherRegistry.Weather)", "");
            }
            return weatherlist;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetWRWeatherMultiplier(SelectableLevel level)
        {
            if (WeatherManager.CurrentWeathers.ContainsKey(level))
            {
                return WeatherManager.CurrentWeathers[level].ScrapValueMultiplier;
            }
            else
            {
                return 1f;
            }
        }
    }
    public static class LBCompatability
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Xilef.LethalBestiary");
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<EnemyType> AddLBEnemies(List<EnemyType> enemies)
        {
            foreach (LethalBestiary.Modules.Enemies.SpawnableEnemy spawnableEnemy in LethalBestiary.Modules.Enemies.spawnableEnemies)
            {
                if (!enemies.Contains(spawnableEnemy.enemy))
                {
                    enemies.Add(spawnableEnemy.enemy);
                    // CentralConfig.instance.mls.LogMessage($"Added enemy: {spawnableEnemy.enemy.enemyName} from LethalBestiary");
                }
            }
            return enemies;
        }
    }
    public static class DiversityCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Chaos.Diversity");
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<EnemyType> AddWalker(List<EnemyType> enemies)
        {
            EnemyType Walker = Diversity.Diversity.longBoiType;
            if (!enemies.Contains(Walker))
            {
                enemies.Add(Walker);
                // CentralConfig.instance.mls.LogMessage($"Added enemy: {Walker.enemyName} from Diversity");
            }
            return enemies;
        }
    }
    public static class FootBallCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Kittenji.FootballEntity");
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<EnemyType> AddFootball(List<EnemyType> enemies)
        {
            EnemyType Football = Kittenji.FootballEntity.Plugin.EnemyDef;
            if (!enemies.Contains(Football))
            {
                enemies.Add(Football);
                // CentralConfig.instance.mls.LogMessage($"Added enemy: {Football.enemyName} from FootBall");
            }
            return enemies;
        }
    }
    public static class RollingGiantCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("nomnomab.rollinggiant");
                }
                return (bool)_enabled;
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static List<EnemyType> AddRollingGiant(List<EnemyType> enemies)
        {
            EnemyType RollingGiantInside = RollingGiant.Plugin.EnemyTypeInside;
            RollingGiantInside.enemyName = "Rolling Giant Inside";
            EnemyType RollingGiantDay = RollingGiant.Plugin.EnemyTypeOutsideDaytime;
            RollingGiantDay.enemyName = "Rolling Giant Day";
            EnemyType RollingGiantNight = RollingGiant.Plugin.EnemyTypeOutside;
            RollingGiantNight.enemyName = "Rolling Giant Night";
            if (!enemies.Contains(RollingGiantInside))
            {
                enemies.Add(RollingGiantInside);
                // CentralConfig.instance.mls.LogMessage($"Added enemy: {RollingGiantInside.enemyName} from RollingGiant");
            }
            if (!enemies.Contains(RollingGiantDay))
            {
                enemies.Add(RollingGiantDay);
                // CentralConfig.instance.mls.LogMessage($"Added enemy: {RollingGiantDay.enemyName} from RollingGiant");
            }
            if (!enemies.Contains(RollingGiantNight))
            {
                enemies.Add(RollingGiantNight);
                // CentralConfig.instance.mls.LogMessage($"Added enemy: {RollingGiantNight.enemyName} from RollingGiant");
            }
            return enemies;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static string RGSEWRTS(SpawnableEnemyWithRarity enemy)
        {
            string returnString = string.Empty;
            if (enemy.enemyType == RollingGiant.Plugin.EnemyTypeInside && enemy.rarity > 0)
            {
                returnString = "Rolling Giant Inside:" + enemy.rarity.ToString() + ",";
            }
            else if (enemy.enemyType == RollingGiant.Plugin.EnemyTypeOutsideDaytime && enemy.rarity > 0)
            {
                returnString = "Rolling Giant Day:" + enemy.rarity.ToString() + ",";
            }
            else if (enemy.enemyType == RollingGiant.Plugin.EnemyTypeOutside && enemy.rarity > 0)
            {
                returnString = "Rolling Giant Night:" + enemy.rarity.ToString() + ",";
            }
            else
            {
                if (enemy.enemyType.enemyName != "Lasso" && enemy.rarity > 0)
                {
                    returnString = enemy.enemyType.enemyName + ":" + enemy.rarity.ToString() + ",";
                }
            }
            return returnString;
        }
    }
}