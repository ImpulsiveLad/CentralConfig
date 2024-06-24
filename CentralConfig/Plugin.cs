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

namespace CentralConfig
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CentralConfig : BaseUnityPlugin
    {
        private const string modGUID = "impulse.CentralConfig";
        private const string modName = "CentralConfig";
        private const string modVersion = "0.8.0";
        public static Harmony harmony = new Harmony(modGUID);

        public ManualLogSource mls;

        public static CentralConfig instance;

        public static int LastUsedSeed;

        public static float shid = 0;

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
            harmony.PatchAll(typeof(WaitForMoonsToRegister));
            harmony.PatchAll(typeof(FrApplyMoon));
            harmony.PatchAll(typeof(ApplyScrapValueMultiplier));
            harmony.PatchAll(typeof(TimeFix));
            harmony.PatchAll(typeof(DayTimePassFix));
            harmony.PatchAll(typeof(WaitForDungeonsToRegister));
            harmony.PatchAll(typeof(FrApplyDungeon));
            harmony.PatchAll(typeof(NewDungeonGenerator));
            harmony.PatchAll(typeof(InnerGenerateWithRetries));
            harmony.PatchAll(typeof(MiscConfig));
            harmony.PatchAll(typeof(ChangeFineAmount));
            harmony.PatchAll(typeof(WaitForWeathersToRegister));
            harmony.PatchAll(typeof(FrApplyWeather));
            harmony.PatchAll(typeof(ResetWeatherLists));
            harmony.PatchAll(typeof(EnactWeatherInjections));
            harmony.PatchAll(typeof(ResetMoonsScrapAfterWeather));
            harmony.PatchAll(typeof(ApplyWeatherScrapMultipliers));
            harmony.PatchAll(typeof(WaitForTagsToRegister));
            harmony.PatchAll(typeof(FrApplyTag));
            harmony.PatchAll(typeof(EnactTagInjections));
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
        [DataMember] public SyncedEntry<string> BlacklistMoons { get; private set; }
        [DataMember] public SyncedEntry<bool> IsWhiteList { get; private set; }
        [DataMember] public SyncedEntry<bool> DoGenOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoScrapOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoEnemyOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoTrapOverrides { get; private set; }
        [DataMember] public SyncedEntry<bool> DoMoonWeatherOverrides { get; private set; }
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

        public GeneralConfig(ConfigFile cfg) : base("CentralConfig") // This config generates on opening the game
        {
            ConfigManager.Register(this);

            BlacklistMoons = cfg.BindSyncedEntry("_MoonLists_", // These are used to decide what more in-depth config values should be made
                "Blacklisted Moons",
                "Liquidation,Gordion",
                "Excludes the listed moons from the config. If they are already created, they will be removed on config regeneration.");

            IsWhiteList = cfg.BindSyncedEntry("_MoonLists_",
                "Is Moon Blacklist a Whitelist?",
                false,
                "If set to true, only the moons listed above will be generated.");

            DoGenOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable General Overrides?",
                false,
                "If set to true, allows altering of some basic properties including the route price, risk level, and description for each moon.");

            DoScrapOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Scrap Overrides?",
                false,
                "If set to true, allows altering of the min/max scrap count, the list of scrap objects on each moon, and a multiplier for the individual scrap item's values.");

            DoEnemyOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Enemy Overrides?",
                false,
                "If set to true, allows altering of the max power counts and lists for enemies on each moon (Interior, Nighttime, and Daytime).");

            FreeEnemies = cfg.BindSyncedEntry("_Moons_",
                "Free Them?",
                false,
                "If set to true, extends the 20 inside/day/night enemy caps to the maximum (127) and sets the interior enemy spawn waves to be hourly instead of every other hour.");

            ScaleEnemySpawnRate = cfg.BindSyncedEntry("_Moons_",
                "Scale Enemy Spawn Rate?",
                false,
                "When enabled, this setting adjusts the enemy spawn rate to match the new enemy powers. Note that this requires the ‘Enemy Overrides’ setting to be true.\nFor example, Experimentation has a default max power of 4 for interior enemies. If you set this to 6, interior enemies will spawn ~1.5x as fast.\nThis applies to interior, day, and night enemy spawns.");

            DoTrapOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Trap Overrides?",
                false,
                "If set to true, allows altering of the min/max count for each trap on each moon.");

            DoMoonWeatherOverrides = cfg.BindSyncedEntry("_Moons_",
                "Enable Weather Overrides?",
                false,
                "If set to true, allows altering of the possible weathers to each moon.\nBeware that adding new weathers to moons that didn't have them before will likely cause funky buggies.\nDO NOT USE WITH WEATHER REGISTRY!!");

            DoDangerBools = cfg.BindSyncedEntry("_Moons_",
                "Enable Misc Overrides?",
                false,
                "If set to true, allows altering of miscellaneous traits of moons such as hidden/unhidden status, locked/unlocked status, if time exists, the time speed multiplier, and if time should wait until the ship lands to begin moving (Keep this false for Selene's Choice to work).");

            RenameCelest = cfg.BindSyncedEntry("_Moons_",
                "Rename Celest?",
                false,
                "If set to true, Celest will be renamed to Celeste. This fixes any config entry mismatches between her and Celestria.");

            BlackListDungeons = cfg.BindSyncedEntry("_DungeonLists_",
                "Blacklisted Dungeons",
                "",
                "Excludes the listed dungeons from the config. If they are already created, they will be removed on config regeneration.");

            IsDunWhiteList = cfg.BindSyncedEntry("_DungeonLists_",
                "Is Dungeon Blacklist a Whitelist?",
                false,
                "If set to true, only the dungeons listed above will be generated.");

            UseNewGen = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Overhauled Dungeon Generation?",
                true,
                "If set to true, this reconfigures the dungeon loading process to avoid loading failure.");

            DoDunSizeOverrides = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Dungeon Size Overrides?",
                false,
                "If set to true, allows altering of the min/max dungeon size multipliers, and the size scaler.\nThis also allows you to set the dungeon size multiplier applied by the individual moons.");

            DoDungeonSelectionOverrides = cfg.BindSyncedEntry("_Dungeons_",
                "Enable Dungeon Selection Overrides?",
                false,
                "If set to true, allows altering of the dungeon selection settings (By moon name, route price range, and mod name.");

            DoFineOverrides = cfg.BindSyncedEntry("~Misc~",
                "Enable Fine Overrides?",
                false,
                "If set to true, allows you to set the fine for dead/missing players and the reduction on the fine for having brought the body back to the ship.");

            BlacklistTags = cfg.BindSyncedEntry("_TagLists_",
                "Blacklisted Tags",
                "",
                "Excludes the listed tags from the config. If they are already created, they will be removed on config regeneration.");

            IsTagWhiteList = cfg.BindSyncedEntry("_TagLists_",
                "Is Tag Blacklist a Whitelist?",
                false,
                "If set to true, only the tags listed above will be generated.");

            DoEnemyTagInjections = cfg.BindSyncedEntry("_Tags_",
                "Enable Enemy Injection by Tag?",
                false,
                "If set to true, allows adding/replacing enemies on levels based on matching tags (inside, day, and night).");

            DoScrapTagInjections = cfg.BindSyncedEntry("_Tags_",
                "Enable Scrap Injection by Tag?",
                false,
                "If set to true, allows adding scrap to levels based on matching tags.");

            RemoveDuplicateEnemies = cfg.BindSyncedEntry("_Enemies_",
                "Remove Duplicate Enemies?",
                false,
                "If set to true, after enemy spawn lists are updated by various means, any time there are 2 or more of the same enemy, only the entry for the enemy with the highest rarity will be kept.\nThis means that if 4 sources add the bracken at various rarities, only the highest value is kept. (In \"Flowerman:5,Flowerman:75,Flowerman:66,Flowerman:73\" The bracken will only be added once with a rarity of 75.)");

            BlacklistWeathers = cfg.BindSyncedEntry("_WeatherLists_",
                "Blacklisted Weathers",
                "",
                "Excludes the listed weathers from the config. If they are already created, they will be removed on config regeneration.");

            IsWeatherWhiteList = cfg.BindSyncedEntry("_WeatherLists_",
                "Is Weather Blacklist a Whitelist?",
                false,
                "If set to true, only the weathers listed above will be generated.");

            DoEnemyWeatherInjections = cfg.BindSyncedEntry("_Weathers_",
                "Enable Enemy Injection by Current Weather?",
                false,
                "If set to true, allows adding/replacing enemies on levels based on the current weather (inside, day, and night).");

            DoScrapWeatherInjections = cfg.BindSyncedEntry("_Weathers_",
                "Enable Scrap Injection by Current Weather?",
                false,
                "If set to true, allows adding scrap to levels based on the current weather as well as multipliers to the scrap amount and individiual scrap values.");
        }
    }
}