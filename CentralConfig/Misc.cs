using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using HarmonyLib;
using LethalLevelLoader;
using LethalLevelLoader.Tools;
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
            public CreateMiscConfig(ConfigFile cfg) : base(cfg, "CentralConfig", 0)
            {
                if (CentralConfig.SyncConfig.DoFineOverrides)
                {
                    FineAmount = cfg.BindSyncedEntry("_Misc_",
                        "Penalty for each fallen player",
                        20f,
                        "This is the percentage of current credits that will be deducted for each fallen player.");

                    InsuranceReduction = cfg.BindSyncedEntry("_Misc_",
                        "Penalty reduction for player revival",
                        40f,
                        "This value determines the reduction in penalty if a fallen player's body is revived. For instance, setting this to 0 means no penalty for revived players, 50 means half the penalty is applied upon revival, and 100 means the penalty remains the same regardless of revival. It can be any value between 0 and 100.");

                    CompanyFineAmount = cfg.BindSyncedEntry("_Misc_",
                        "Penalty for each fallen player (on Gordion)",
                        0f,
                        "This is the percentage of current credits that will be deducted for each fallen player on Gordion.");

                    CompanyInsuranceReduction = cfg.BindSyncedEntry("_Misc_",
                        "Penalty reduction for player revival (on Gordion)",
                        0f,
                        "This value determines the reduction in penalty on Gordion if a fallen player's body is revived. For instance, setting this to 0 means no penalty for revived players, 50 means half the penalty is applied upon revival, and 100 means the penalty remains the same regardless of revival. It can be any value between 0 and 100.");

                    ProportionalFine = cfg.BindSyncedEntry("?Misc?",
                        "Proportional Fine",
                        false,
                        "Should the fine to proportional to the ratio of dead players over total players?\nFor instance, if the penatly if 50% and half of the lobby dies, the fine will be 25% as opposed to being 50% for each dead player.");
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
}
