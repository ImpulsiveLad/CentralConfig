using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CSync.Lib;
using CSync.Extensions;
using HarmonyLib;
using System.Runtime.Serialization;
using static CentralConfig.WaitForMoonsToRegister;
using static CentralConfig.WaitForDungeonsToRegister;
using static CentralConfig.MiscConfig;
using static CentralConfig.WaitForTagsToRegister;
using static CentralConfig.WaitForWeathersToRegister;
using static CentralConfig.ResetChanger;
using static CentralConfig.ScrapShuffler;
using static CentralConfig.EnemyShuffler;
using static CentralConfig.ShuffleSaver;
using static CentralConfig.DungeonShuffler;

namespace CentralConfig
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Xilef.LethalBestiary", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Kittenji.FootballEntity", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Chaos.Diversity", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("nomnomab.rollinggiant", BepInDependency.DependencyFlags.SoftDependency)]
    public class CentralConfig : BaseUnityPlugin
    {
        private const string modGUID = "impulse.CentralConfig";
        private const string modName = "CentralConfig";
        private const string modVersion = "0.13.5";
        public static Harmony harmony = new Harmony(modGUID);

        public ManualLogSource mls;

        public static CentralConfig instance;

        public static int LastUsedSeed;

        public static float shid = 0;

        public static bool HarmonyTouch = false;
        public static bool HarmonyTouch2 = false;
        public static bool HarmonyTouch3 = false;
        public static bool HarmonyTouch4 = false;
        public static bool HarmonyTouch5 = false;
        public static bool HarmonyTouch6 = false;

        public static CreateMoonConfig ConfigFile;

        public static CreateDungeonConfig ConfigFile2;

        public static CreateMiscConfig ConfigFile3;

        public static CreateTagConfig ConfigFile4;

        public static CreateWeatherConfig ConfigFile5;

        public static GeneralConfig SyncConfig;

        void Awake()
        {
            instance = this;

            SyncConfig = new GeneralConfig(base.Config);

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            ConfigFile = new CreateMoonConfig(base.Config);

            ConfigFile2 = new CreateDungeonConfig(base.Config);

            ConfigFile3 = new CreateMiscConfig(base.Config);

            ConfigFile4 = new CreateTagConfig(base.Config);

            ConfigFile5 = new CreateWeatherConfig(base.Config);

            harmony.PatchAll(typeof(RenameCelest));
            harmony.PatchAll(typeof(ResetOnDisconnect));
            harmony.PatchAll(typeof(WaitForMoonsToRegister));
            harmony.PatchAll(typeof(FrApplyMoon));
            harmony.PatchAll(typeof(FreeEnemies));
            harmony.PatchAll(typeof(TimeFix));
            harmony.PatchAll(typeof(DayTimePassFix));
            harmony.PatchAll(typeof(RandomNextPatch));
            harmony.PatchAll(typeof(UpdateTimeFaster));
            harmony.PatchAll(typeof(WaitForDungeonsToRegister));
            harmony.PatchAll(typeof(FrApplyDungeon));
            harmony.PatchAll(typeof(NewDungeonGenerator));
            harmony.PatchAll(typeof(InnerGenerateWithRetries));
            harmony.PatchAll(typeof(MiscConfig));
            harmony.PatchAll(typeof(ChangeFineAmount));
            harmony.PatchAll(typeof(IncreaseNodeDistanceOnShipAndMain));
            harmony.PatchAll(typeof(IncreaseNodeDistanceOnFE));
            harmony.PatchAll(typeof(ExtendScan));
            harmony.PatchAll(typeof(UpdateScanNodes));
            harmony.PatchAll(typeof(AddPlayerToDict));
            harmony.PatchAll(typeof(WaitForWeathersToRegister));
            harmony.PatchAll(typeof(FrApplyWeather));
            harmony.PatchAll(typeof(ResetMoonsScrapAfterWeather));
            harmony.PatchAll(typeof(ApplyWeatherScrapMultipliers));
            harmony.PatchAll(typeof(WaitForTagsToRegister));
            harmony.PatchAll(typeof(FrApplyTag));
            harmony.PatchAll(typeof(IncreaseScrapAppearances));
            harmony.PatchAll(typeof(CheckForEnemySpawns));
            harmony.PatchAll(typeof(UpdateEnemyDictionary));
            harmony.PatchAll(typeof(UpdateDungeonDictionary));
            harmony.PatchAll(typeof(SaveShuffleDataStrings));

            harmony.PatchAll(typeof(ResetEnemyAndScrapLists));
            harmony.PatchAll(typeof(FetchEnemyAndScrapLists));
            harmony.PatchAll(typeof(EnactWeatherInjections));
            harmony.PatchAll(typeof(EnactTagInjections));
            harmony.PatchAll(typeof(EnactDungeonInjections));

            harmony.PatchAll(typeof(MoarEnemies1));
            harmony.PatchAll(typeof(MoarEnemies2));
            harmony.PatchAll(typeof(MoarEnemies3));
            // Logging stuff
            // harmony.PatchAll(typeof(ShowIntEnemyCount));
            // harmony.PatchAll(typeof(CountTraps));
            // harmony.PatchAll(typeof(LogFinalSize));
            // harmony.PatchAll(typeof(LogScrapValueMultipler));
        }
    }
    [DataContract]
    public class GeneralConfig : SyncedConfig2<GeneralConfig>
    {
        [DataMember] public SyncedEntry<string> Important { get; private set; }
        [DataMember] public SyncedEntry<string> BlacklistMoons { get; private set; }
        [DataMember] public SyncedEntry<bool> IsWhiteList { get; private set; }
        [DataMember] public SyncedEntry<bool> DoGenOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScrapOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoEnemyOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoTrapOverrides { get; private set; }
        // [DataMember] public SyncedEntry<bool> DoMoonWeatherOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoDangerBools { get; private set; }
        [DataMember] public SyncedEntry<string> BlackListDungeons { get; private set; }
        [DataMember] public SyncedEntry<bool> IsDunWhiteList { get; private set; }
        [DataMember] public SyncedEntry<bool> UseNewGen { get; private set; }
        [DataMember] public SyncedEntry<bool> DoDunSizeOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoDungeonSelectionOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoFineOverrides { get; private set; }
        [DataMember] public SyncedEntry<string> BlacklistTags { get; private set; }
        [DataMember] public SyncedEntry<bool> IsTagWhiteList { get; private set; }
        [DataMember] public SyncedEntry<bool> DoEnemyTagInjections { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScrapTagInjections { get; private set; }
        [DataMember] public SyncedEntry<bool> FreeEnemies { get; private set; }
        [DataMember] public SyncedEntry<bool> ScaleEnemySpawnRate { get; private set; }
        [DataMember] public SyncedEntry<bool> RenameCelest { get; private set; }
        [DataMember] public SyncedEntry<bool> RemoveDuplicateEnemies { get; private set; }
        [DataMember] public SyncedEntry<string> BlacklistWeathers { get; private set; }
        [DataMember] public SyncedEntry<bool> IsWeatherWhiteList { get; private set; }
        [DataMember] public SyncedEntry<bool> DoEnemyWeatherInjections { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScrapWeatherInjections { get; private set; }
        [DataMember] public SyncedEntry<bool> DoEnemyInjectionsByDungeon { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScrapInjectionsByDungeon { get; private set; }
        [DataMember] public SyncedEntry<int> UnShrankDungenTries { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScanNodeOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> KeepSmallerDupes { get; private set; }
        [DataMember] public SyncedEntry<bool> AlwaysKeepZeros { get; private set; }
        [DataMember] public SyncedEntry<bool> BigEnemyList { get; private set; }
        [DataMember] public SyncedEntry<int> RandomSeed { get; private set; }
        [DataMember] public SyncedEntry<bool> KeepOrphans { get; private set; }
        [DataMember] public SyncedEntry<bool> TimeSettings { get; private set; }
        [DataMember] public SyncedEntry<bool> UpdateTimeFaster { get; private set; }
        [DataMember] public SyncedEntry<int> BracketTries { get; private set; }
        [DataMember] public SyncedEntry<bool> LogEnemies { get; private set; }
        [DataMember] public SyncedEntry<string> NewTags { get; private set; }
        [DataMember] public SyncedEntry<bool> GlobalEnemyAndScrap { get; private set; }
        [DataMember] public SyncedEntry<bool> EnemySpawnTimes { get; private set; }
        [DataMember] public SyncedEntry<bool> ScrapShuffle { get; private set; }
        [DataMember] public SyncedEntry<bool> EnemyShuffle { get; private set; }
        [DataMember] public SyncedEntry<int> ScrapShuffleRandomMin { get; private set; }
        [DataMember] public SyncedEntry<int> ScrapShuffleRandomMax { get; private set; }
        [DataMember] public SyncedEntry<int> EnemyShuffleRandomMin { get; private set; }
        [DataMember] public SyncedEntry<int> EnemyShuffleRandomMax { get; private set; }
        [DataMember] public SyncedEntry<bool> ShuffleSave { get; private set; }
        [DataMember] public SyncedEntry<string> ScrapShuffleBlacklist { get; private set; }
        [DataMember] public SyncedEntry<string> EnemyShuffleBlacklist { get; private set; }
        [DataMember] public SyncedEntry<bool> RemoveZeros { get; private set; }
        [DataMember] public SyncedEntry<bool> DungeonShuffler { get; private set; }
        [DataMember] public SyncedEntry<int> DungeonShuffleRandomMin { get; private set; }
        [DataMember] public SyncedEntry<int> DungeonShuffleRandomMax { get; private set; }
        [DataMember] public SyncedEntry<bool> DungeonShufflerPercent { get; private set; }
        [DataMember] public SyncedEntry<bool> EnemyShufflerPercent { get; private set; }
        [DataMember] public SyncedEntry<bool> ScrapShufflerPercent { get; private set; }
        [DataMember] public SyncedEntry<bool> RolloverNegatives { get; private set; }
        public GeneralConfig(ConfigFile cfg) : base("CentralConfig") // This config generates on opening the game
        {
            ConfigManager.Register(this);

            Important = cfg.BindSyncedEntry("!READ THIS!",
                "Important Info !!!",
                "This does nothing btw.",
                "The bulk of settings are created and applied individually foreach different entry in a group. They are not generated until after enabling the main true/false toggle for the related setting then rebooting the game.\nEach intial entry is marked with either 'Host Only' or 'All Players'. 'Host Only' settings are only set by the host and do not run on clients. They do not need to be synced.\n'All Players' settings MUST BE SYNCED and set on all players in the lobby.\nIf in doubt just send the config file to everyone.");

            BlacklistMoons = cfg.BindSyncedEntry("_MoonLists_", // These are used to decide what more in-depth config values should be made
                "Blacklisted Moons (All Players (for general, time, and/or misc settings)",
                "Liquidation,Gordion",
                "Excludes the listed moons from the config. If they are already created, they will be removed on config regeneration.");

            IsWhiteList = cfg.BindSyncedEntry("_MoonLists_",
                "Is Moon Blacklist a Whitelist? (All Players (for general, time, and/or misc settings))",
                false,
                "If set to true, only the moons listed above will be generated.");

            GlobalEnemyAndScrap = cfg.BindSyncedEntry("~Global~",
                "Global Enemy and Scrap Injection (Host Only)",
                false,
                "If set to true, allows adding/replacing enemies to the indoor, daytime, and nighttime enemy pools as well as adding scrap onto every moon through only a few settings.");

            DoGenOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable General Overrides? (All Players)",
                false,
                "If set to true, allows altering of some basic properties including the route price, risk level, and description for each moon.");

            DoScrapOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Scrap Overrides? (Host Only)",
                false,
                "If set to true, allows altering of the min/max scrap count, the list of scrap objects on each moon, and a multiplier for the individual scrap item's values.");

            DoEnemyOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Enemy Overrides? (Host Only)",
                false,
                "If set to true, allows altering of the max power counts and lists for enemies on each moon (Interior, Nighttime, and Daytime).");

            BigEnemyList = cfg.BindSyncedEntry("~Big Lists~",
                "Big Enemy Lists? (Host Only)",
                false,
                "If set to true, you will be able to set the enemy lists for every moon with just three strings. This is helpful if you want to copy-paste from a spreadsheet.\nIf you set enemy lists per moon with the setting above, they will contribute to the default value but be overridden by the big lists given the specific moon is found in the big list string.");

            FreeEnemies = cfg.BindSyncedEntry("_Enemies_",
                "Free Them? (Host Only)",
                false,
                "If set to true, extends the 20 inside/day/night enemy caps to the maximum (127) and sets the interior enemy spawn waves to be hourly instead of every other hour.");

            ScaleEnemySpawnRate = cfg.BindSyncedEntry("_Enemies_",
                "Scale Enemy Spawn Rate? (Host Only)",
                false,
                "When enabled, this setting adjusts the enemy spawn rate to match the new enemy powers. Note that this requires the ‘Enemy Overrides’ setting to be true.\nFor example, Experimentation has a default max power of 4 for interior enemies. If you set this to 6, interior enemies will spawn ~1.5x as fast.\nThis applies to interior, day, and night enemy spawns.");

            EnemySpawnTimes = cfg.BindSyncedEntry("_Enemies_",
                "Accelerate Enemy Spawning? (Host Only)",
                false,
                "If set to true, allows you to set a new value per moon that tweaks the enemy spawn timing.");

            DoTrapOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Trap Overrides? (Host Only)",
                false,
                "If set to true, allows altering of the min/max count for each trap on each moon.");

            /*DoMoonWeatherOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Weather Overrides?",
                false,
                "If set to true, allows altering of the possible weathers to each moon.\nBeware that adding new weathers to moons that didn't have them before will likely cause funky buggies.\nDO NOT USE WITH WEATHER REGISTRY!!");*/

            UpdateTimeFaster = cfg.BindSyncedEntry("~Misc~",
                "Accurate Clock? (All Players)",
                true,
                "If set to true, the in-game clock will be updated when time moves instead of every 3 seconds.");

            TimeSettings = cfg.BindSyncedEntry("_Moons_",
                "Enable Time Settings? (All Players)",
                false,
                "If set to true, allows setting if time exists, the time speed multiplier, and if time should wait until the ship lands to begin moving (All per moon).");

            DoDangerBools = cfg.BindSyncedEntry("_Moons_",
                "Enable Misc Overrides? (All Players)",
                false,
                "If set to true, allows altering of miscellaneous traits of moons such as hidden/unhidden status, and locked/unlocked status (Keep this false for Selene's Choice compat).");

            RenameCelest = cfg.BindSyncedEntry("_Moons_",
                "Rename Celest? (All Players)",
                false,
                "If set to true, Celest will be renamed to Celeste. This fixes any config entry mismatches between her and Celestria.");

            BlackListDungeons = cfg.BindSyncedEntry("_DungeonLists_",
                "Blacklisted Dungeons (All Players (for dungeon size))",
                "",
                "Excludes the listed dungeons from the config. If they are already created, they will be removed on config regeneration.");

            IsDunWhiteList = cfg.BindSyncedEntry("_DungeonLists_",
                "Is Dungeon Blacklist a Whitelist? (All Players (for dungeon size))",
                false,
                "If set to true, only the dungeons listed above will be generated.");

            UseNewGen = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Dungeon Generation Safeguards? (All Players)",
                true,
                "If set to true, this refines the dungeon loading process to retry with various imput sizes.\nInstead of a hard-cap of 20 attempts, the dungeon will go through and attempt to generate with a different size until it succeeds.\nThis *should* only fail if the dungeon has its own generation issues or its size multiplier is reduced below an acceptable size to begin with.");

            UnShrankDungenTries = cfg.BindSyncedEntry("_Dungeons_",
                "Retries before Changing Size (All Players)",
                20,
                "The number of attempts made by the dungeon to generate using its original input size multiplier before it begins to adjust the multiplier either upwards or downwards.\nPreviously, the size adjustment used to happen after just one failed attempt.");

            BracketTries = cfg.BindSyncedEntry("_Dungeons_",
                "Tries per Dungeon Size Bracket (All Players)",
                25,
                "The number of generation attempts spent on each size bracket (The brackets are: 20x-10x, 10x-5x, 5x-3x, 3x-2x, and 2x-1x for reference).\nIncreasing this value gives an exponentially higher chance of generation success but takes more time.");

            DoDunSizeOverrides = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Dungeon Size Overrides? (All Players)",
                false,
                "If set to true, allows altering various dungeon size related numbers. This includes the moon-tied size multipliers, the Dungeon's Map Tile Size (which is divided from the moon's size), a Dungeon specific min/max size clamp applied after the MTS is factored in, the scaler to determine how strict the size clamp should be, and a Dungeon specific min/max random size multiplier applied after the clamping.");

            DoDungeonSelectionOverrides = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Dungeon Selection Overrides? (Host Only)",
                false,
                "If set to true, allows altering the dungeon selection pool tied to the dungeon by moon name, level tags, route price range, and mod name.");

            DoEnemyInjectionsByDungeon = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Enemy Injection by Current Dungeon? (Host Only)",
                false,
                "If set to true, allows adding/replacing enemies on levels based on the current dungeon (inside, day, and night.");

            DoScrapInjectionsByDungeon = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Scrap Injection by Current Dungeon? (Host Only)",
                false,
                "If set to true, allows adding scrap to levels based on matching tags.");

            KeepOrphans = cfg.BindSyncedEntry("~Misc~",
                "Keep Orphaned Entries? (Personal)",
                false,
                "If set to true, the config will not 'clean' itself after processing, this will result in a more crowded/messy config but will prevent unloaded entries from being removed.");

            RandomSeed = cfg.BindSyncedEntry("~Misc~",
                "Starting Random Seed (Host Only)",
                -1,
                "Leave at -1 to have it be random. The seed will be updated daily regardless of this setting.");

            LogEnemies = cfg.BindSyncedEntry("~Misc~",
                "Log Current Enemy/Scrap Tables? (Host Only)",
                false,
                "If set to true, the console will log the current indoor, daytime, and nighttime enemy spawn pools as well as the current scrap pool 10 seconds after loading into the level.\nOnly accurate on the host, as enemy and scrap pools are ultimately decided by the host.");

            DoFineOverrides = cfg.BindSyncedEntry("~Misc~",
                "Enable Fine Overrides? (All Players)",
                false,
                "If set to true, allows you to set the fine for dead/missing players and the reduction on the fine for having brought the body back to the ship.");

            DoScanNodeOverrides = cfg.BindSyncedEntry("~Misc~",
                "Enable Scan Node Extensions? (All Players)",
                false,
                "If set to true, allows you to set the min/max ranges for the scan nodes on the ship, main entrance, and fire exits (if you have ScannableFireExit installed).");

            BlacklistTags = cfg.BindSyncedEntry("_TagLists_",
                "Blacklisted Tags (Host Only)",
                "vanilla,custom,free,paid,argon,canyon,company,forest,marsh,military,ocean,rocky,tundra,valley,wasteland,volcanic,rosiedev,sfdesat,starlancermoons,tolian",
                "Excludes the listed tags from the config. If they are already created, they will be removed on config regeneration.");

            IsTagWhiteList = cfg.BindSyncedEntry("_TagLists_",
                "Is Tag Blacklist a Whitelist? (Host Only)",
                true,
                "If set to true, only the tags listed above will be generated.");

            DoEnemyTagInjections = cfg.BindSyncedEntry("_Tags_",
                "Enable Enemy Injection by Tag? (Host Only)",
                false,
                "If set to true, allows adding/replacing enemies on levels based on matching tags (inside, day, and night).");

            DoScrapTagInjections = cfg.BindSyncedEntry("_Tags_",
                "Enable Scrap Injection by Tag? (Host Only)",
                false,
                "If set to true, allows adding scrap to levels based on matching tags.");

            RemoveDuplicateEnemies = cfg.BindSyncedEntry("_Enemies_",
                "Remove Duplicate Enemies? (Host Only)",
                false,
                "If set to true, after enemy spawn lists are updated by various means, any time there are 2 or more of the same enemy, only the entry for the enemy with the highest rarity will be kept.\nThis means that if 4 sources add the bracken at various rarities, only the highest value is kept. (In \"Flowerman:5,Flowerman:75,Flowerman:66,Flowerman:73\" The bracken will only be added once with a rarity of 75.)");

            KeepSmallerDupes = cfg.BindSyncedEntry("_Enemies_",
                "Keep Smallest Rarity? (Host Only)",
                false,
                "If this and Remove Duplicates is set to true, the lowest rarity of a given enemy with be kept instead of its highest rarity.");

            AlwaysKeepZeros = cfg.BindSyncedEntry("_Enemies_",
                "Always Keep Zeros? (Host Only)",
                false,
                "If this and Remove Duplicates is set to true, any enemies with that have an entry of 0 rarity will be kept regardless. This allows you to blacklist enemies from specific weathers/tags/dungeons by putting EnemyName:0 under the setting.");

            BlacklistWeathers = cfg.BindSyncedEntry("_WeatherLists_",
                "Blacklisted Weathers (Host Only)",
                "DustClouds",
                "Excludes the listed weathers from the config. If they are already created, they will be removed on config regeneration.");

            IsWeatherWhiteList = cfg.BindSyncedEntry("_WeatherLists_",
                "Is Weather Blacklist a Whitelist? (Host Only)",
                false,
                "If set to true, only the weathers listed above will be generated.");

            DoEnemyWeatherInjections = cfg.BindSyncedEntry("_Weathers_",
                "Enable Enemy Injection by Current Weather? (Host Only)",
                false,
                "If set to true, allows adding/replacing enemies on levels based on the current weather (inside, day, and night).");

            DoScrapWeatherInjections = cfg.BindSyncedEntry("_Weathers_",
                "Enable Scrap Injection by Current Weather? (Host Only)",
                false,
                "If set to true, allows adding scrap to levels based on the current weather as well as multipliers to the scrap amount and individiual scrap values.");

            NewTags = cfg.BindSyncedEntry("_TagLists_",
                "New Tags (Host Only)",
                "Smunguss,Glorble,Badungle",
                "New 'tags' that are considered by this mod's settings (Remember to not blacklist them above).");

            ScrapShuffle = cfg.BindSyncedEntry("~Shufflers~",
                "Scrap Shuffler (Host Only)",
                false,
                "If set to true, scrap that could have but did not spawn on a given day will be more likely to spawn the next day, provided that the scrap is in the next level's final scrap pool as well.\nThis temporary selection chance boost increases every day the specific scrap was in the scrap pool but was not selected. The boost returns to 0 when ANY amount of that scrap is spawned.");

            ScrapShuffleRandomMin = cfg.BindSyncedEntry("~Shufflers~",
                "Scrap Shuffler Random Min (Host Only)",
                0,
                "The number of days since the last appearance of this scrap is multiplied by a random value before being applied added to the scrap's rarity in the current scrap pool. If it is (1, 1) then it will increase the scrap's rarity by exactly 1 per day since it last spawned.\nBy default, the scrap's rarity will be increased by 0, 1 or 2 * the number of days since it last spawned.");

            ScrapShuffleRandomMax = cfg.BindSyncedEntry("~Shufflers~",
                "Scrap Shuffler Random Max (Host Only)",
                2,
                "The number of days since the last appearance of this scrap is multiplied by a random value before being applied added to the scrap's rarity in the current scrap pool. If it is (1, 1) then it will increase the scrap's rarity by exactly 1 per day since it last spawned.\nBy default, the scrap's rarity will be increased by 0, 1 or 2 * the number of days since it last spawned.");

            ScrapShufflerPercent = cfg.BindSyncedEntry("~Shufflers~",
                "Scrap Shuffler Percent? (Host Only)",
                false,
                "If set to true, the random value from above will be a percent of the rarity times the days since it last appeared.\nFalse: Rarity += DayCount * RandomValue\nTrue: Rarity *= (DayCount * (Randomvalue/100))");

            ScrapShuffleBlacklist = cfg.BindSyncedEntry("~Shufflers~",
                "Blacklisted Scrap (Host Only)",
                "Default Values Are Empty",
                "Scrap listed here in 'ScrapName,ScrapName' format will be ignored by the shuffle.");

            EnemyShuffle = cfg.BindSyncedEntry("~Shufflers~",
                "Enemy Shuffler (Host Only)",
                false,
                "If set to true, enemies that could have but did spawn on a given day will be more likely to spawn the next day, provided that the enemy is in one of the next level's final enemy pools as well.\nThis temporary selection chance boost increases every day the specific enemy was in the spawn pool but was not spawned. The boost returns to 0 when ANY number of that enemy is spawned inside, during the day, or during the night.");

            EnemyShuffleRandomMin = cfg.BindSyncedEntry("~Shufflers~",
                "Enemy Shuffler Random Min (Host Only)",
                0,
                "The number of days since the last appearance of this enemy is multiplied by a random value before being applied added to the enemy's rarity in all the current enemy pools. If it is (1, 1) then it will increase the enemy's rarity by exactly 1 per day since it last spawned.\nBy default, the enemy's rarity will be increased by 0, 1 or 2 * the number of days since it last spawned.");

            EnemyShuffleRandomMax = cfg.BindSyncedEntry("~Shufflers~",
                "Enemy Shuffler Random Max (Host Only)",
                2,
                "The number of days since the last appearance of this enemy is multiplied by a random value before being applied added to the enemy's rarity in all the current enemy pools. If it is (1, 1) then it will increase the enemy's rarity by exactly 1 per day since it last spawned.\nBy default, the enemy's rarity will be increased by 0, 1 or 2 * the number of days since it last spawned.");

            EnemyShufflerPercent = cfg.BindSyncedEntry("~Shufflers~",
                "Enemy Shuffler Percent? (Host Only)",
                false,
                "If set to true, the random value from above will be a percent of the rarity times the days since it last appeared.\nFalse: Rarity += DayCount * RandomValue\nTrue: Rarity *= (DayCount * (Randomvalue/100))");

            EnemyShuffleBlacklist = cfg.BindSyncedEntry("~Shufflers~",
                "Blacklisted Enemies (Host Only)",
                "Default Values Are Empty",
                "Enemies listed here in 'EnemyName,EnemyName' format will be ignored by the shuffle.");

            DungeonShuffler = cfg.BindSyncedEntry("~Shufflers~",
                "Dungeon Shuffler (Host Only)",
                false,
                "If set to true, dungeons that are not selected after a given day will be more likely to be selected the next day, provided that the dungeon is possible to be selected by the next level.\nThis temporary selection chance boost increases every day that dungeon was selectable but was not chosen. The boost returns to 0 when the dungeon is selected.");

            DungeonShuffleRandomMin = cfg.BindSyncedEntry("~Shufflers~",
                "Dungeon Shuffler Random Min (Host Only)",
                 0,
                "The number of days since the last selection of this interior is multiplied by a random value before being applied added to the dungeon's rarity in all matches. If it is (1, 1) then it will increase the dungeon's rarity by exactly 1 per day since it was last selected.\nBy default, the dungeon's rarity will be increased by 0-20% * the number of days since it last spawned.");

            DungeonShuffleRandomMax = cfg.BindSyncedEntry("~Shufflers~",
                "Dungeon Shuffler Random Max (Host Only)",
                20,
                "The number of days since the last selection of this interior is multiplied by a random value before being applied added to the dungeon's rarity in all matches. If it is (1, 1) then it will increase the dungeon's rarity by exactly 1 per day since it was last selected.\nBy default, the dungeon's rarity will be increased by 0-20% * the number of days since it last spawned.");

            DungeonShufflerPercent = cfg.BindSyncedEntry("~Shufflers~",
                "Dungeon Shuffler Percent? (Host Only)",
                true,
                "If set to true, the random value from above will be a percent of the rarity times the days since it last appeared.\nFalse: Rarity += DayCount * RandomValue\nTrue: Rarity *= (DayCount * (Randomvalue/100))");

            ShuffleSave = cfg.BindSyncedEntry("~Shufflers~",
                "Save Shuffle Data (Host Only)",
                false,
                "If set to true, the shuffle data for enemies and scrap will be committed to the save file and loaded on start-up. This means that the counters for how many days since they last spawned will be preserved, and the rarity boosters will be applied accordingly on next landing.\nIf this setting remains false, the shuffle will only exist in the session and be forgotten on reboot.");

            RemoveZeros = cfg.BindSyncedEntry("~Shufflers~",
                "Remove Zero Rarity Scrap and Enemies? (Host Only)",
                true,
                "If set to true, the shuffler will remove all scrap/enemies with a rarity of 0 before boosting rarities. This ensures they won’t gain days when they may have a rarity of 0 to prevent them from spawning on specific moons, during certain weather conditions, or in particular dungeons.");

            RolloverNegatives = cfg.BindSyncedEntry("~Shufflers~",
                "Rollover Zero or Less (Host Only)",
                false,
                "If set to true, entries with a rarity of 0 or less will always increase linearly instead of by percent.\nThis setting has a very specific use, enemies or scrap with a negative rarity will gradually increase until they become positive, effectively creating a minimum number of days between their appearances on that level.");
        }
    }
}