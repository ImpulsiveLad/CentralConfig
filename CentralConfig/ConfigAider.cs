using BepInEx.Configuration;
using LethalLevelLoader;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CentralConfig
{
    public class ConfigAider
    {
        // Safety box of LLL config stuff renamed + some other random methods
        public const string DaComma = ",";
        public const string DaColon = ":";
        public const string DaDash = "-";
        public static List<Item> Items { get; internal set; } = new List<Item>();
        public static string CauterizeString(string inputString)
        {
            return (inputString.SkipToLetters().RemoveWhitespace().ToLower());
        }

        internal static List<Item> GrabFullItemList()
        {
            List<Item> allItems = StartOfRound.Instance.allItemsList.itemsList;
            foreach (Item item in allItems)
            {
                if (!Items.Contains(item))
                {
                    Items.Add(item);
                }
            }
            return Items.ToList();
        }

        public static List<EnemyType> GrabFullEnemyList()
        {
            List<EnemyType> allEnemies = new List<EnemyType>();

            foreach (EnemyType enemy in OriginalContent.Enemies)
            {
                if (!allEnemies.Contains(enemy))
                {
                    allEnemies.Add(enemy);
                }
            }
            foreach (ExtendedEnemyType extendedEnemy in PatchedContent.ExtendedEnemyTypes)
            {
                if (!allEnemies.Contains(extendedEnemy.EnemyType))
                {
                    allEnemies.Add(extendedEnemy.EnemyType);
                }
            }
            foreach (Enemies.SpawnableEnemy spawnableEnemy in Enemies.spawnableEnemies)
            {
                if (!allEnemies.Contains(spawnableEnemy.enemy))
                {
                    allEnemies.Add(spawnableEnemy.enemy);
                }
            }
            return allEnemies;
        }
        public static List<ContentTag> GrabFullTagList()
        {
            List<ContentTag> allContentTagsList = new List<ContentTag>();

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods.Concat(new List<ExtendedMod>() { PatchedContent.VanillaMod }))
            {
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                {
                    foreach (ContentTag contentTag in extendedContent.ContentTags)
                    {
                        if (!allContentTagsList.Contains(contentTag))
                        {
                            allContentTagsList.Add(contentTag);
                        }
                    }
                }
            }
            return allContentTagsList;
        }

        public static List<string> SplitStringsByDaComma(string newInputString)
        {
            List<string> stringList = new List<string>();

            string inputString = newInputString;

            while (inputString.Contains(DaComma))
            {
                string inputStringWithoutTextBeforeFirstComma = inputString.Substring(inputString.IndexOf(DaComma));
                stringList.Add(inputString.Replace(inputStringWithoutTextBeforeFirstComma, ""));
                if (inputStringWithoutTextBeforeFirstComma.Contains(DaComma))
                    inputString = inputStringWithoutTextBeforeFirstComma.Substring(inputStringWithoutTextBeforeFirstComma.IndexOf(DaComma) + 1);

            }
            stringList.Add(inputString);

            return stringList;
        }
        public static (string, string) SplitStringsByDaColon(string inputString)
        {
            return SplitStringByChars(inputString, DaColon);
        }
        public static (string, string) SplitStringByDaDash(string inputString)
        {
            return SplitStringByChars(inputString, DaDash);
        }
        public static (string, string) SplitStringByChars(string newInputString, string splitValue)
        {
            if (!newInputString.Contains(splitValue))
                return ((newInputString, string.Empty));
            else
            {
                string firstValue = string.Empty;
                string secondValue = string.Empty;
                firstValue = newInputString.Replace(newInputString.Substring(newInputString.IndexOf(splitValue)), "");
                secondValue = newInputString.Substring(newInputString.IndexOf(splitValue) + 1);
                return ((firstValue, secondValue));
            }
        }

        public static List<StringWithRarity> ConvertStringToList(string newInputString, Vector2 clampRarity)
        {
            List<StringWithRarity> returnList = new List<StringWithRarity>();

            List<string> stringList = SplitStringsByDaComma(newInputString);

            foreach (string stringString in stringList)
            {
                (string, string) splitStringData = SplitStringsByDaColon(stringString);
                string levelName = splitStringData.Item1;
                int rarity = 0;
                if (int.TryParse(splitStringData.Item2, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    Mathf.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                returnList.Add(new StringWithRarity(levelName, rarity));
            }
            return returnList;
        }

        // Enemies

        public static string ConvertEnemyListToString(List<SpawnableEnemyWithRarity> spawnableEnemiesList)
        {
            string returnString = string.Empty;

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in spawnableEnemiesList)
            {
                if (spawnableEnemyWithRarity.enemyType.enemyName != "Lasso")
                {
                    returnString += spawnableEnemyWithRarity.enemyType.enemyName + DaColon + spawnableEnemyWithRarity.rarity.ToString() + DaComma;
                }
            }

            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";

            return returnString;
        }
        public static List<SpawnableEnemyWithRarity> ConvertStringToEnemyList(string newInputString, Vector2 clampRarity)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty")
            {
                return new List<SpawnableEnemyWithRarity>();
            }
            List<StringWithRarity> stringList = ConvertStringToList(newInputString, clampRarity);
            List<SpawnableEnemyWithRarity> returnList = new List<SpawnableEnemyWithRarity>();

            List<EnemyType> AllEnemies = GrabFullEnemyList();

            foreach (EnemyType enemyType in AllEnemies)
            {
                foreach (StringWithRarity stringString in new List<StringWithRarity>(stringList))
                {
                    if (enemyType.enemyName.ToLower().Contains(stringString.Name.ToLower()))
                    {
                        SpawnableEnemyWithRarity newEnemy = new SpawnableEnemyWithRarity();
                        newEnemy.enemyType = enemyType;
                        newEnemy.rarity = stringString.Rarity;
                        returnList.Add(newEnemy);
                        stringList.Remove(stringString);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Return List:");
            foreach (SpawnableEnemyWithRarity enemyType in returnList)
            {
                sb.AppendLine($"Item Name: {enemyType.enemyType.enemyName}, Rarity: {enemyType.rarity}");
            }
            // CentralConfig.instance.mls.LogInfo(sb.ToString());

            return returnList;
        }

        // Scrap

        public static string ConvertItemListToString(List<SpawnableItemWithRarity> spawnableItemsList)
        {
            string returnString = string.Empty;

            foreach (SpawnableItemWithRarity spawnableItemWithRarity in spawnableItemsList)
                returnString += spawnableItemWithRarity.spawnableItem.itemName + DaColon + spawnableItemWithRarity.rarity.ToString() + DaComma;
            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";
            return returnString;
        }
        public static List<SpawnableItemWithRarity> ConvertStringToItemList(string newInputString, Vector2 clampRarity)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty")
            {
                return new List<SpawnableItemWithRarity>();
            }
            List<StringWithRarity> stringList = ConvertStringToList(newInputString, clampRarity);
            List<SpawnableItemWithRarity> returnList = new List<SpawnableItemWithRarity>();

            List<Item> allItems = GrabFullItemList();

            foreach (Item item in allItems)
            {
                foreach (StringWithRarity stringString in new List<StringWithRarity>(stringList))
                {
                    if (CauterizeString(item.itemName).Contains(CauterizeString(stringString.Name)) || CauterizeString(stringString.Name).Contains(CauterizeString(item.itemName)))
                    {
                        SpawnableItemWithRarity newItem = new SpawnableItemWithRarity();
                        newItem.spawnableItem = item;
                        newItem.rarity = stringString.Rarity;
                        returnList.Add(newItem);
                        stringList.Remove(stringString);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Return List:");
            foreach (SpawnableItemWithRarity item in returnList)
            {
                sb.AppendLine($"Item Name: {item.spawnableItem.itemName}, Rarity: {item.rarity}");
            }
            // CentralConfig.instance.mls.LogInfo(sb.ToString());

            return returnList;
        }

        // Weather + Tags

        public static string ConvertWeatherArrayToString(RandomWeatherWithVariables[] randomWeatherArray)
        {
            string returnString = "None" + DaComma;

            foreach (RandomWeatherWithVariables randomWeather in randomWeatherArray)
                returnString += randomWeather.weatherType + DaComma;
            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            return returnString;
        }
        public static RandomWeatherWithVariables[] ConvertStringToWeatherArray(string newInputString)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty")
            {
                return new RandomWeatherWithVariables[] { new RandomWeatherWithVariables { weatherType = LevelWeatherType.None } };
            }

            string[] weatherStrings = newInputString.Split(',');

            RandomWeatherWithVariables[] returnArray = new RandomWeatherWithVariables[weatherStrings.Length];

            for (int i = 0; i < weatherStrings.Length; i++)
            {
                if (Enum.TryParse(weatherStrings[i], out LevelWeatherType weatherType))
                {
                    returnArray[i] = new RandomWeatherWithVariables { weatherType = weatherType };
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Return Array:");
            foreach (RandomWeatherWithVariables weatherType in returnArray)
            {
                sb.AppendLine($"{weatherType.weatherType}");
            }
            // CentralConfig.instance.mls.LogInfo(sb.ToString());
            return returnArray;
        }

        public static string ConvertTagsToString(List<ContentTag> contentTags)
        {
            string returnString = string.Empty;

            foreach (ContentTag contentTag in contentTags)
                returnString += contentTag.contentTagName + DaComma;

            if (returnString.EndsWith(","))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (string.IsNullOrEmpty(returnString))
                returnString = "Default Values Were Empty";

            return returnString;
        }
        public static List<ContentTag> ConvertStringToTagList(string newInputString)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty")
            {
                return new List<ContentTag>();
            }
            List<string> stringList = newInputString.Split(',').ToList();
            List<ContentTag> returnList = new List<ContentTag>();

            List<ContentTag> allContentTagsList = GrabFullTagList();

            foreach (ContentTag contentTag in allContentTagsList)
            {
                foreach (string stringString in new List<string>(stringList))
                {
                    if (CauterizeString(contentTag.contentTagName).Contains(CauterizeString(stringString)) || CauterizeString(stringString).Contains(CauterizeString(contentTag.contentTagName)))
                    {
                        returnList.Add(contentTag);
                        stringList.Remove(stringString);
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Return List:");
            foreach (ContentTag tag in returnList)
            {
                sb.AppendLine($"{tag.contentTagName}");
            }
            // CentralConfig.instance.mls.LogInfo(sb.ToString());
            return returnList;
        }

        // Misc String

        public static string ConvertStringWithRarityToString(List<StringWithRarity> names)
        {
            string returnString = string.Empty;

            foreach (StringWithRarity name in names)
                returnString += name.Name + DaColon + name.Rarity.ToString() + DaComma;

            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";

            return (returnString);
        }
        public static List<StringWithRarity> ConvertModStringToStringWithRarityList(string newInputString, Vector2 clampRarity)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty" || newInputString == "Default Values Were Empty:0" || newInputString == ":0")
            {
                return new List<StringWithRarity>();
            }
            List<StringWithRarity> returnList = new List<StringWithRarity>();

            List<string> stringList = SplitStringsByDaComma(newInputString);

            foreach (string stringString in stringList)
            {
                (string, string) splitStringData = SplitStringsByDaColon(stringString);
                string modName = splitStringData.Item1;
                int rarity = 0;
                if (int.TryParse(splitStringData.Item2, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    Mathf.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                returnList.Add(new StringWithRarity(modName, rarity));
            }
            return (returnList);
        }

        // This method should make the planet name list have to be the exact name

        public static List<StringWithRarity> ConvertPlanetNameStringToStringWithRarityList(string newInputString, Vector2 clampRarity)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty" || newInputString == "Default Values Were Empty:0" || newInputString == ":0")
            {
                return new List<StringWithRarity>();
            }
            List<StringWithRarity> returnList = new List<StringWithRarity>();

            List<string> stringList = SplitStringsByDaComma(newInputString);

            HashSet<string> allPlanetNames = new HashSet<string>(PatchedContent.ExtendedLevels.Select(level => level.NumberlessPlanetName));

            foreach (string stringString in stringList)
            {
                (string planetName, string rarityString) = SplitStringsByDaColon(stringString);
                int rarity = 0;
                if (int.TryParse(rarityString, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    Mathf.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                if (allPlanetNames.Contains(planetName))
                {
                    returnList.Add(new StringWithRarity(planetName, rarity));
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Return List:");
            foreach (StringWithRarity planetName in returnList)
            {
                sb.AppendLine($"{planetName.Name}: {planetName.Rarity}");
            }
            // CentralConfig.instance.mls.LogInfo(sb.ToString());
            return (returnList);
        }

        // For Route Price settings

        public static string ConvertVector2WithRaritiesToString(List<Vector2WithRarity> values)
        {
            string returnString = string.Empty;

            foreach (Vector2WithRarity vector2withRarity in values)
                returnString += vector2withRarity.Min + DaDash + vector2withRarity.Max + DaColon + vector2withRarity.Rarity + DaComma;

            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";

            return (returnString);
        }
        public static List<Vector2WithRarity> ConvertStringToVector2WithRarityList(string newInputString, Vector2 clampRarity)
        {
            if (string.IsNullOrEmpty(newInputString) || newInputString == "Default Values Were Empty" || newInputString == "Default Values Were Empty:0" || newInputString == ":0" || newInputString == "0-0:0")
            {
                return new List<Vector2WithRarity>();
            }
            List<Vector2WithRarity> returnList = new List<Vector2WithRarity>();

            List<string> stringList = SplitStringsByDaComma(newInputString);

            foreach (string stringString in stringList)
            {
                (string, string) splitStringData = SplitStringsByDaColon(stringString);
                (string, string) vector2Strings = SplitStringByDaDash(splitStringData.Item1);

                float x = 0f;
                float y = 0f;
                int rarity = 0;
                if (float.TryParse(vector2Strings.Item1, out float xValue))
                    x = xValue;
                if (float.TryParse(vector2Strings.Item2, out float yValue))
                    y = yValue;
                if (int.TryParse(splitStringData.Item2, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    Mathf.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                returnList.Add(new Vector2WithRarity(new Vector2(x, y), rarity));
            }
            return (returnList);
        }

        // Modified cfg cleaner from Kitten :3

        public static async Task CleanConfig(ConfigFile cfg)
        {
            while (!ApplyMoonConfig.Ready || !ApplyDungeonConfig.Ready)
            {
                await Task.Delay(1000);
            }
            PropertyInfo orphanedEntriesProp = cfg.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg, null);
            orphanedEntries.Clear();
            cfg.Save();
            CentralConfig.instance.mls.LogInfo("Config Cleaned");
        }
    }
}