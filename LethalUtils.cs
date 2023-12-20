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
        private const string modVersion = "0.0.2";
        private readonly Harmony harmony = new Harmony(modGUID);
        static internal ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        private static LethalGUI gui;

        public static bool guiEnabled = false;

        // Changed flags
        private static bool changedDeadline = false;
        private static bool changedDeadlineLock = false;
        private static bool changedServerName = false;
        private static bool enabledInfiniteSprint = false;

        private static bool enabledCredit = false;
        private static bool enabledCreditLock = false;

        private static bool changedItemSlots = false;

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~~~~~~~~~~~~~ PATCH METHODS ~~~~~~~~~~~~~~~~~
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        // ChangeDeadline() - Change profit quota deadline
        // Prefix Patches TimeOfDay.UpdateProfitQuotaCurrentTime() Method
        // Targets instance variables totalTime and timeUntilDeadline
        // It appears totalTime is the time of 1 day
        // It appears timeUntilDeadline multiplies the totalTime by the number of days
        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.UpdateProfitQuotaCurrentTime))]
        [HarmonyPrefix]
        static void ChangeDeadline(ref float ___totalTime, ref float ___timeUntilDeadline)
        {
            if (changedDeadline)
            {
                ___timeUntilDeadline = ___totalTime * ServerChangeables.companyDeadline.Value;
                changedDeadline = changedDeadlineLock;
            }
            
            
        }

        /* ChangeDayTime()
        // Prefix Patches TimeOfDay.Start() Method
        //
        [HarmonyPatch(typeof(TimeOfDay), "Start")]
        [HarmonyPrefix]
        static void ChangeDayTime(ref float ___lengthOfHours, ref int ___numberOfHours)
        {
            ___lengthOfHours = ServerChangeables.serverTimeSpeed.Value;
            ___numberOfHours = ServerChangeables.serverTimeHours.Value;
        }*/
        

        // EnableInfiniteSprint() - Enables and disables infinite character sprint
        // Prefix Patches PlayerControllerB.Update() Method
        // Targets instance variable sprintMeter
        // sprintMeter is calculated every frame, prefix sets value to 1.0f (maximum) each frame.
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        static void EnableInfiniteSprint(ref float ___sprintMeter)
        {
            if (enabledInfiniteSprint)
            {
                ___sprintMeter = 1f;
                
            }
        }



        // SetCredits() - Change lobby credits and/or lock them to a specific value
        // Postfix Patches Terminal.RunTerminalEvents() Method
        // Targets instance variable groupCredits
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

        // -----------------------------------------------

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~~~~~~~~~~~~~ GUI Methods ~~~~~~~~~~~~~~~~~~~
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // StopCameraMovement() - Disable camera movement when custom GUI menu is opened
        // Postfix Patches PlayerControllerB.Update() Method
        // Targets instance variable playerActions
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void StopCameraMovement(ref PlayerActions ___playerActions)
        {
            // GUI Open will freeze player camera movement
            if (guiEnabled)
            {
                ___playerActions.Movement.Disable();                
            }
            // GUI Close will resume player camera movement
            else if (!guiEnabled)
            {
                ___playerActions.Movement.Enable();

            }

        }

        // GUI() - Creation of the GUI GameObject
        // Prefix Patches StartOfRound.Awake() Method
        // Runs as soon as a game is started.
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPrefix]
        static void GUI()
        {
            if (StartOfRound.Instance == null)
            {
                mls.LogInfo("LethalUtils.GUI()");
                // Create the LethalGUI GameObject at start of round
                var gameObject = new UnityEngine.GameObject("LethalGUI");
                // Ensure that the GameObject stays between scenes so we don't have to recreate it
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                // Add the LethalGUI object to our gameobject
                gameObject.AddComponent<LethalGUI>();
                // Not necessary, remove later
                gui = (LethalGUI)gameObject.GetComponent("LethalGUI");
            }
            
        }

        // -----------------------------------------------


        // Injection point

        private static LethalUtils Instance;
        void Awake()
        {
            Instance = this;
            var totalTimeElapsed = System.Diagnostics.Stopwatch.StartNew();
            mls.LogInfo("Loading LethalUtils...");

            mls.LogInfo("Reading config file values.");

            // Bind config file entries
            ServerChangeables.serverName = Config.Bind("Server", "ServerName", "LethalServer", "Set the name of the server.");

            ServerChangeables.serverTimeHours = Config.Bind("Server.Time", "ServerTimeHours", 7, "Set the number of hours for the day.");
            ServerChangeables.serverTimeSpeed = Config.Bind("Server.Time", "ServerTimeSpeed", 100.0f, "Set the length of hours for the day.");

            ServerChangeables.companyDeadline = Config.Bind("Company", "Deadline", 3, "Set the number of days until deadline for the server.");
            ServerChangeables.companyCurrentQuota = Config.Bind("Company", "CurrentQuota", 0, "Set the currently obtained quota for the server.");
            ServerChangeables.companyQuota = Config.Bind("Company", "ReachableQuota", 300, "Set the reachable quota for the server.");
            ServerChangeables.companyStartingCredits = Config.Bind("Company", "StartingCredits", 60, "Set the starting credits for the server.");

            ServerChangeables.menuWidth = Config.Bind("GUI", "MenuWidth", 75.0f, "Set the width of the GUI menu. [0-100]%");
            ServerChangeables.menuHeight = Config.Bind("GUI", "MenuHeight", 75.0f, "Set the height of the GUI menu between. [0-100]%");

            // Print to the console all current values in the config file
            mls.LogInfo("ServerName: " + ServerChangeables.serverName.Value);
            mls.LogInfo("ServerTimeHours: " + ServerChangeables.serverTimeHours.Value);
            mls.LogInfo("ServerTimeSpeed: " + ServerChangeables.serverTimeSpeed.Value);
            mls.LogInfo("Deadline: " + ServerChangeables.companyDeadline.Value);
            mls.LogInfo("CurrentQuota: " + ServerChangeables.companyCurrentQuota.Value);
            mls.LogInfo("ReachableQuota: " + ServerChangeables.companyQuota.Value);
            mls.LogInfo("StartingCredits: " + ServerChangeables.companyStartingCredits.Value);

            mls.LogInfo("Finished reading config file.");

            mls.LogInfo("Begin patching...");
            harmony.PatchAll(typeof(LethalUtils));
            mls.LogInfo("Finished patching!");
            totalTimeElapsed.Stop();
            mls.LogInfo("Finished loading LethalUtils! (" + totalTimeElapsed.ElapsedMilliseconds + "ms)");


        }

        void Update()
        {

        }



    }
}
