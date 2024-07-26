using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader;
using LethalLevelLoader.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using UnityEngine;

namespace CentralConfig
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    public class MiscConfig
    {
        public static CreateMiscConfig Config;

        [DataContract]
        public class CreateMiscConfig : ConfigTemplate
        {
            [DataMember] public static SyncedEntry<float> FineAmount { get; private set; }
            [DataMember] public static SyncedEntry<float> InsuranceReduction { get; private set; }
            [DataMember] public static SyncedEntry<float> CompanyFineAmount { get; private set; }
            [DataMember] public static SyncedEntry<float> CompanyInsuranceReduction { get; private set; }
            [DataMember] public static SyncedEntry<bool> ProportionalFine { get; private set; }
            [DataMember] public static SyncedEntry<int> ShipMinScan { get; private set; }
            [DataMember] public static SyncedEntry<int> ShipMaxScan { get; private set; }
            [DataMember] public static SyncedEntry<int> MEMinScan { get; private set; }
            [DataMember] public static SyncedEntry<int> MEMaxScan { get; private set; }
            [DataMember] public static SyncedEntry<int> FEMinScan { get; private set; }
            [DataMember] public static SyncedEntry<int> FEMaxScan { get; private set; }
            public CreateMiscConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                if (CentralConfig.SyncConfig.DoFineOverrides)
                {
                    FineAmount = cfg.BindSyncedEntry("~Misc~",
                        "Penalty for each fallen player",
                        20f,
                        "This is the percentage of current credits that will be deducted for each fallen player.");

                    InsuranceReduction = cfg.BindSyncedEntry("~Misc~",
                        "Penalty reduction for player retrievalretrieval",
                        40f,
                        "This value determines the reduction in penalty if a fallen player's body is revived. For instance, setting this to 0 means no penalty for revived players, 50 means half the penalty is applied upon revival, and 100 means the penalty remains the same regardless of revival. It can be any value between 0 and 100.");

                    CompanyFineAmount = cfg.BindSyncedEntry("~Misc~",
                        "Penalty for each fallen player (on Gordion)",
                        0f,
                        "This is the percentage of current credits that will be deducted for each fallen player on Gordion.");

                    CompanyInsuranceReduction = cfg.BindSyncedEntry("~Misc~",
                        "Penalty reduction for player retrieval (on Gordion)",
                        0f,
                        "This value determines the reduction in penalty on Gordion if a fallen player's body is revived. For instance, setting this to 0 means no penalty for revived players, 50 means half the penalty is applied upon revival, and 100 means the penalty remains the same regardless of revival. It can be any value between 0 and 100.");

                    ProportionalFine = cfg.BindSyncedEntry("~Misc~",
                        "Proportional Fine",
                        false,
                        "Should the fine to proportional to the ratio of dead players over total players?\nFor instance, if the penatly if 50% and half of the lobby dies, the fine will be 25% as opposed to being 50% for each dead player.");
                }

                if (CentralConfig.SyncConfig.DoScanNodeOverrides)
                {
                    ShipMinScan = cfg.BindSyncedEntry("~Misc~",
                        "Min Ship Scan Distance",
                        17,
                        "You must be this far away to scan the ship (Vanilla is 17).");

                    ShipMaxScan = cfg.BindSyncedEntry("~Misc~",
                        "Max Ship Scan Distance",
                        2147483647,
                        "Max distance you can scan the ship from (Vanilla is 110).\nReduce if you encounter lag.");

                    MEMinScan = cfg.BindSyncedEntry("~Misc~",
                        "Min Main Entrance Scan Distance",
                        17,
                        "You must be this far away to scan the main entrance (Vanilla is 17).");

                    MEMaxScan = cfg.BindSyncedEntry("~Misc~",
                        "Max Main Entrance Scan Distance",
                        2147483647,
                        "Max distance you can scan the main entrance from (Vanilla is 100).\nReduce if you encounter lag.");

                    FEMinScan = cfg.BindSyncedEntry("~Misc~",
                        "Min Fire Exit Scan Distance",
                        12,
                        "You must be this far away to scan fire exits (Default is 12).\nYou must have ScannableFireExit in order for this to work.");

                    FEMaxScan = cfg.BindSyncedEntry("~Misc~",
                        "Max Fire Exit Scan Distance",
                        2147483647,
                        "Max distance you can scan fire exits from (Default is 52).\nYou must have ScannableFireExit in order for this to work.\nReduce if you encounter lag.");
                }
            }
        }
    }
    [HarmonyPatch(typeof(HUDManager), "ApplyPenalty")]
    public class ChangeFineAmount
    {
        static float Grungus;
        static float Shmunguss;
        static float totalPlayers = GameNetworkManager.Instance.connectedPlayers + 1;
        static string Recovered;
        static string Bodies;
        static bool Prefix(HUDManager __instance, int playersDead, int bodiesInsured)
        {
            if (!CentralConfig.SyncConfig.DoFineOverrides)
            {
                return true;
            }
            if (LevelManager.CurrentExtendedLevel.NumberlessPlanetName == "Gordion")
            {
                Grungus = MiscConfig.CreateMiscConfig.CompanyFineAmount / 100f;
                Shmunguss = 1 / (MiscConfig.CreateMiscConfig.CompanyInsuranceReduction / 100f);
            }
            else
            {
                Grungus = MiscConfig.CreateMiscConfig.FineAmount / 100f;
                Shmunguss = 1 / (MiscConfig.CreateMiscConfig.InsuranceReduction / 100f);
            }
            Terminal terminal = Object.FindObjectOfType<Terminal>();
            int groupCredits = terminal.groupCredits;
            bodiesInsured = Mathf.Max(bodiesInsured, 0);
            for (int i = 0; i < playersDead - bodiesInsured; i++)
            {
                if (MiscConfig.CreateMiscConfig.ProportionalFine)
                {
                    terminal.groupCredits -= (int)(groupCredits * Grungus * (playersDead / totalPlayers));
                }
                else
                {
                    terminal.groupCredits -= (int)(groupCredits * Grungus);
                }
            }
            for (int j = 0; j < bodiesInsured; j++)
            {
                if (MiscConfig.CreateMiscConfig.ProportionalFine)
                {
                    terminal.groupCredits -= (int)(groupCredits * (Grungus / Shmunguss) * (playersDead / totalPlayers));
                }
                else
                {
                    terminal.groupCredits -= (int)(groupCredits * (Grungus / Shmunguss));
                }
            }
            if (terminal.groupCredits < 0)
            {
                terminal.groupCredits = 0;
            }
            int totalFinePercentage;
            if (MiscConfig.CreateMiscConfig.ProportionalFine)
            {
                int uninsuredBodies = playersDead - bodiesInsured;
                float uninsuredProportion = uninsuredBodies / totalPlayers;
                float insuredProportion = bodiesInsured / (Shmunguss * totalPlayers);

                totalFinePercentage = (int)Mathf.Round(Grungus * 100 * (uninsuredProportion + insuredProportion));
            }
            else
            {
                totalFinePercentage = (int)Mathf.Round((Grungus * 100f) * (playersDead - bodiesInsured) + ((Grungus * 100f / Shmunguss)) * bodiesInsured);
            }
            if (playersDead > 1)
            {
                Bodies = "There were " + playersDead + " casualties, ";
            }
            else
            {
                Bodies = "There was " + playersDead + " casualty, ";
            }
            if (bodiesInsured > 0)
            {
                Recovered = "but " + bodiesInsured + " bodies were recovered.";
            }
            else
            {
                Recovered = "and " + bodiesInsured + " bodies were recovered.";
            }
            __instance.statsUIElements.penaltyAddition.text = Bodies + Recovered + "\nA fine of " + totalFinePercentage + "% will be extracted from the crew.";

            __instance.statsUIElements.penaltyTotal.text = $"DUE: ${groupCredits - terminal.groupCredits}";
            return false;
        }
    }
    [HarmonyPatch(typeof(TimeOfDay), "MoveGlobalTime")]
    public static class IncreaseNodeDistanceOnFE
    {
        public static Dictionary<char, ScanNodeProperties> fireExitScanProperties = new Dictionary<char, ScanNodeProperties>();
        public static bool IsDone = false;
        static void Postfix()
        {
            if (IsDone || !CentralConfig.SyncConfig.DoScanNodeOverrides)
            {
                return;
            }

            List<EntranceTeleport> entranceTeleports = Object.FindObjectsOfType<EntranceTeleport>().ToList();
            int numFireExits = (entranceTeleports.Count / 2) - 1;
            // CentralConfig.instance.mls.LogInfo("Doing it lol");

            char[] letters = { 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            if (numFireExits <= letters.Length)
            {
                letters = letters.Take(numFireExits).ToArray();
            }
            else
            {
                CentralConfig.instance.mls.LogInfo("There are more than 25 fire exits aaaaa (This shouldn't happen so def report plz).");
            }
            char[] dictLetters = letters.Except(fireExitScanProperties.Keys).ToArray();
            if (dictLetters.Length > 0)
            {
                foreach (char letter in dictLetters)
                {
                    string nodeName = "Environment/Teleports/EntranceTeleport" + letter + "/ScanNode";
                    GameObject FireExitScanNode = GameObject.Find(nodeName);
                    if (FireExitScanNode != null)
                    {
                        ScanNodeProperties FireExitScanProperties = FireExitScanNode.GetComponent<ScanNodeProperties>();
                        if (!fireExitScanProperties.ContainsKey(letter))
                        {
                            fireExitScanProperties[letter] = FireExitScanProperties;
                        }
                        FireExitScanProperties.minRange = MiscConfig.CreateMiscConfig.FEMinScan;
                        FireExitScanProperties.maxRange = MiscConfig.CreateMiscConfig.FEMaxScan;
                        FireExitScanProperties.requiresLineOfSight = false;
                        CentralConfig.instance.mls.LogInfo("Updated Fire Exit: " + letter);
                    }
                }
            }
            else
            {
                IsDone = true;
                CentralConfig.instance.mls.LogInfo("All Fire Exits Set Up.");
            }
        }
    }
    [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
    public static class IncreaseNodeDistanceOnShipAndMain
    {
        static void Postfix()
        {
            if (!CentralConfig.SyncConfig.DoScanNodeOverrides)
            {
                return;
            }

            IncreaseNodeDistanceOnFE.fireExitScanProperties.Clear();
            IncreaseNodeDistanceOnFE.IsDone = false;

            List<string> subTexts = new List<string> { "Remember the quota…", "ALERT", "Go away…", "Dare to enter?", "Get that scrap!", "???", "Face the fear", "No one has left alive…", "Turn back…", "Why?", "Take heed…", "Watch your back…", "You should be afraid…" };
            System.Random rand = new System.Random(StartOfRound.Instance.randomMapSeed + 42);
            string randomSubText = subTexts[rand.Next(subTexts.Count)];

            ScanNodeProperties[] allScanNodes = GameObject.FindObjectsOfType<ScanNodeProperties>();
            foreach (ScanNodeProperties scanNode in allScanNodes)
            {
                if (scanNode.headerText.ToLower() == "main entrance" || scanNode.headerText.ToLower() == "mainentrance")
                {
                    scanNode.minRange = MiscConfig.CreateMiscConfig.MEMinScan;
                    scanNode.maxRange = MiscConfig.CreateMiscConfig.MEMaxScan;
                    scanNode.requiresLineOfSight = false;
                    if (scanNode.subText == "")
                    {
                        scanNode.subText = randomSubText;
                    }
                }
                else if (scanNode.headerText.ToLower() == "ship")
                {
                    scanNode.minRange = MiscConfig.CreateMiscConfig.ShipMinScan;
                    scanNode.maxRange = MiscConfig.CreateMiscConfig.ShipMaxScan;
                    scanNode.requiresLineOfSight = false;
                    if (scanNode.subText == "")
                    {
                        scanNode.subText = "Home base";
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(HUDManager), "AssignNewNodes")]
    public static class ExtendScan
    {
        public static float UltimateMax;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (CentralConfig.SyncConfig.DoScanNodeOverrides)
            {
                UltimateMax = Mathf.Max(MiscConfig.CreateMiscConfig.ShipMaxScan, Mathf.Max(MiscConfig.CreateMiscConfig.MEMaxScan, MiscConfig.CreateMiscConfig.FEMaxScan));
            }
            else
            {
                UltimateMax = 80f;
            }
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 80f)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, UltimateMax);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
    [HarmonyPatch(typeof(EntranceTeleport), "TeleportPlayerServerRpc")]
    public static class UpdateScanNodes
    {
        public static string WhereAreThey;
        public static int EntranceScanMax;
        public static int ShipScanMax;
        public static int FEScanMax;
        static void Postfix(EntranceTeleport __instance, int playerObj)
        {
            if (!__instance.IsHost || !CentralConfig.SyncConfig.DoScanNodeOverrides)
            {
                return;
            }

            __instance.StartCoroutine(PerformActionsWithDelay(__instance, playerObj));
        }
        static IEnumerator PerformActionsWithDelay(EntranceTeleport __instance, int playerObj)
        {
            yield return new WaitForSeconds(1);

            StartOfRound startOfRound = Object.FindObjectOfType<StartOfRound>();
            PlayerControllerB player = startOfRound.allPlayerScripts[playerObj];

            PlayerScanNodeProperties scanNodeProperties = PlayerScanNodeProperties.playerScanNodeProperties[player];

            ScanNodeProperties[] allScanNodes = GameObject.FindObjectsOfType<ScanNodeProperties>();

            if (player.isInsideFactory)
            {
                WhereAreThey = "Inside";
            }
            else
            {
                WhereAreThey = "Outside";
            }

            foreach (ScanNodeProperties scanNode in allScanNodes)
            {
                if (scanNode.headerText.ToLower() == "main entrance" || scanNode.headerText.ToLower() == "mainentrance" || scanNode.headerText.ToLower().Contains("entrance"))
                {
                    scanNode.maxRange = player.isInsideFactory ? scanNodeProperties.MEMinScan : scanNodeProperties.MEMaxScan;
                    EntranceScanMax = scanNode.maxRange;
                }
                else if (scanNode.headerText.ToLower() == "ship" || scanNode.headerText.ToLower().Contains("ship"))
                {
                    scanNode.maxRange = player.isInsideFactory ? scanNodeProperties.ShipMinScan : scanNodeProperties.ShipMaxScan;
                    ShipScanMax = scanNode.maxRange;
                }
                else if (scanNode.headerText.ToLower() == "fire exit" || scanNode.headerText.ToLower().Contains("fire exit"))
                {
                    scanNode.maxRange = player.isInsideFactory ? scanNodeProperties.FEMinScan : scanNodeProperties.FEMaxScan;
                    FEScanMax = scanNode.maxRange;
                }
            }
            CentralConfig.instance.mls.LogInfo("Player: " + player.playerUsername + " is now " + WhereAreThey + ". Max Scan Ranges set to: " + EntranceScanMax + ", " + ShipScanMax + " and " + FEScanMax);
        }
        public class PlayerScanNodeProperties
        {
            public int MEMinScan = MiscConfig.CreateMiscConfig.MEMinScan;
            public int MEMaxScan = MiscConfig.CreateMiscConfig.MEMaxScan;
            public int ShipMinScan = MiscConfig.CreateMiscConfig.ShipMinScan;
            public int ShipMaxScan = MiscConfig.CreateMiscConfig.ShipMaxScan;
            public int FEMinScan = MiscConfig.CreateMiscConfig.FEMinScan;
            public int FEMaxScan = MiscConfig.CreateMiscConfig.FEMaxScan;

            public static Dictionary<PlayerControllerB, PlayerScanNodeProperties> playerScanNodeProperties = new Dictionary<PlayerControllerB, PlayerScanNodeProperties>();
        }
    }
    [HarmonyPatch(typeof(PlayerControllerB), "Start")]
    public static class AddPlayerToDict
    {
        static void Postfix(PlayerControllerB __instance)
        {
            if (!CentralConfig.SyncConfig.DoScanNodeOverrides)
            {
                return;
            }
            UpdateScanNodes.PlayerScanNodeProperties scanNodeProperties = new UpdateScanNodes.PlayerScanNodeProperties
            {
                MEMinScan = MiscConfig.CreateMiscConfig.MEMinScan,
                MEMaxScan = MiscConfig.CreateMiscConfig.MEMaxScan,
                ShipMinScan = MiscConfig.CreateMiscConfig.ShipMinScan,
                ShipMaxScan = MiscConfig.CreateMiscConfig.ShipMaxScan,
                FEMinScan = MiscConfig.CreateMiscConfig.FEMinScan,
                FEMaxScan = MiscConfig.CreateMiscConfig.FEMaxScan
                
            };

            UpdateScanNodes.PlayerScanNodeProperties.playerScanNodeProperties[__instance] = scanNodeProperties;
        }
    }
}