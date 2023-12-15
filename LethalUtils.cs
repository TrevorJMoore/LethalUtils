using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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


        private static ServerChangeables serverChangeables;

        public static bool guiEnabled = false;

        // Changed flags
        private static bool changedDeadline = false;
        private static bool changedServerName = false;
        private static bool enabledInfiniteSprint = true;
        private static bool changedItemSlots = false;



        // Change the server's name (private methods need string literals)
        [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyCreated")]
        [HarmonyPrefix]
        static void UpdateServerName(ref HostSettings ___lobbyHostSettings, ref LobbySlot lobby)
        {
            if (changedServerName) 
            { 
                ___lobbyHostSettings.lobbyName = serverChangeables.GetServerName();
            }
            
        }

        // Change the server's deadline (public methods access through method calling)
        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.MoveGlobalTime))]
        [HarmonyPrefix]
        static void ChangeDeadline(ref int ___daysUntilDeadline)
        {
            // If the server's deadline is changed, change the deadline
            if (changedDeadline) 
            { 
                ___daysUntilDeadline = serverChangeables.GetServerDeadline();
            }
            
        }

        

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        static void infiniteSprintPatch(ref float ___sprintMeter)
        {
            if (enabledInfiniteSprint)
            {
                ___sprintMeter = 1f;
                
            }

            
        }

        
        // GUI Open will freeze player camera movement
        // GUI Close will resume player camera movement
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void stopCameraMovement(ref PlayerActions ___playerActions)
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
            mls.LogInfo("LethalUtils.Awake()");
            harmony.PatchAll(typeof(LethalUtils));

        }

        void Update()
        {

        }



    }
}
