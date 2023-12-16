using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace LethalUtils
{
    // This class is the arbiter behind all utility functions
    [BepInPlugin(modGUID, modName, modVersion)]
    internal class LethalUtils : BaseUnityPlugin
    {
        // Mod Metadata
        private const string modGUID = "MUFFIN.UTILS";
        private const string modName = "Lethal Utilities";
        private const string modVersion = "0.0.1";
        private readonly Harmony harmony = new Harmony(modGUID);
        static internal ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        private static LethalGUI gui;

        public static bool guiEnabled = false;

        // Changed flags
        private static bool changedDeadline = false;
        private static bool changedServerName = false;
        private static bool enabledInfiniteSprint = true;

        private static bool enabledCredit = true;
        private static bool enabledCreditLock = false;

        private static bool changedItemSlots = false;



        // Change the server's name (private methods need string literals)
        [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyCreated")]
        [HarmonyPrefix]
        static void UpdateServerName(ref HostSettings ___lobbyHostSettings, ref LobbySlot lobby)
        {
            if (changedServerName) 
            { 
                ___lobbyHostSettings.lobbyName = ServerChangeables.serverName.Value;
            }
            
        }

        // Change the server's deadline (public methods access through method calling)
        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime))]
        [HarmonyPrefix]
        static void ChangeDeadline(ref int ___daysUntilDeadline)
        {
            // If the server's deadline is changed, change the deadline
            ___daysUntilDeadline = ServerChangeables.companyDeadline.Value;
            
        }

        // Change the length of days
        [HarmonyPatch(typeof(TimeOfDay), "Start")]
        [HarmonyPrefix]
        static void ChangeDayTime(ref float ___lengthOfHours, ref int ___numberOfHours)
        {
            ___lengthOfHours = ServerChangeables.serverTimeSpeed.Value;
            ___numberOfHours = ServerChangeables.serverTimeHours.Value;
        }
        

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        static void InfiniteSprintPatch(ref float ___sprintMeter)
        {
            if (enabledInfiniteSprint)
            {
                ___sprintMeter = 1f;
                
            }

            
        }



        /* On start of round, change credits to number specified in config.
        // Need to do a reverse patch on Terminal.Start()
        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        static void ChangeStartingCredits(ref int ___groupCredits)
        {
            ___groupCredits = ServerChangeables.companyCredits.Value;
            mls.LogInfo("changeStartingCredits companyCredits: " + ServerChangeables.companyCredits.Value);
        }*/

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.RunTerminalEvents))]
        [HarmonyPostfix]
        static void SetCredits(ref int ___groupCredits)
        {
            if (enabledCredit)
            {
                ___groupCredits = ServerChangeables.companyStartingCredits.Value;
                mls.LogInfo("ChangeStartingCredits companyStartCredits.Value = " + ServerChangeables.companyStartingCredits.Value);
                // If there is no lock, run this if block once. Otherwise, keep running it.
                enabledCredit = enabledCreditLock;
            }
            
        }

        /*[HarmonyPatch(typeof(Terminal), nameof(Terminal.SyncGroupCreditsServerRpc))]
        [HarmonyPostfix]
        static void ChangeStartingCredits2(ref int ___groupCredits)
        {
            ___groupCredits = ServerChangeables.companyStartingCredits.Value;
            mls.LogInfo("ChangeStartingCredits companyStartCredits.Value = " + ServerChangeables.companyStartingCredits.Value);
        }*/


        // GUI Open will freeze player camera movement
        // GUI Close will resume player camera movement
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void StopCameraMovement(ref PlayerActions ___playerActions)
        {
            if (guiEnabled)
            {
                ___playerActions.Movement.Disable();                
            }
            else if (!guiEnabled)
            {
                ___playerActions.Movement.Enable();

            }

        }


        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPrefix]
        static void GUI()
        {
            if (StartOfRound.Instance == null)
            {
                mls.LogInfo("LethalUtils.GUI()");
                // Create the LethalGUI GameObject at start of round
                var gameObject = new UnityEngine.GameObject("LethalGUI");
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<LethalGUI>();
                gui = (LethalGUI)gameObject.GetComponent("LethalGUI");
            }
            
        }


        private static LethalUtils Instance;
        void Awake()
        {
            Instance = this;
            mls.LogInfo("Starting LethalUtils...");

            mls.LogInfo("Reading config file values.");
            // Config file entries
            ServerChangeables.serverName = Config.Bind("Server", "ServerName", "LethalServer", "Set the name of the server.");

            //ServerChangeables.serverTime = Config.Bind("Server.Time", "ServerTime", 0.0f, "Set the time of day for the server."); NOT A CONFIG OPTION USE IT THROUGH GUI
            ServerChangeables.serverTimeHours = Config.Bind("Server.Time", "ServerTimeHours", 7, "Set the number of hours for the day.");
            ServerChangeables.serverTimeSpeed = Config.Bind("Server.Time", "ServerTimeSpeed", 100.0f, "Set the length of hours for the day.");

            ServerChangeables.companyDeadline = Config.Bind("Company", "Deadline", 4, "Set the number of days until deadline for the server.");
            ServerChangeables.companyCurrentQuota = Config.Bind("Company", "CurrentQuota", 0, "Set the currently obtained quota for the server.");
            ServerChangeables.companyQuota = Config.Bind("Company", "ReachableQuota", 300, "Set the reachable quota for the server.");
            ServerChangeables.companyStartingCredits = Config.Bind("Company", "StartingCredits", 60, "Set the starting credits for the server.");

            mls.LogInfo("Group Credits: " + ServerChangeables.companyStartingCredits.Value);
            mls.LogInfo("Begin patching...");
            harmony.PatchAll(typeof(LethalUtils));
            mls.LogInfo("Finished patching!");
            mls.LogInfo("Finished loading LethalUtils!");

        }

        void Update()
        {

        }



    }
}
