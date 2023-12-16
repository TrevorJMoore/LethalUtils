using BepInEx.Configuration;

namespace LethalUtils
{
    // Class to hold all necessary settings set by the player
    internal static class ServerChangeables
    {
        // Establish Global Variables for Settings
        public static ConfigEntry<string> serverName { get; set; }  // Enables editting of server name
        public static ConfigEntry<float> serverTime { get; set; }   // Enables editting of current time in game
        public static ConfigEntry<float> serverTimeSpeed { get; set; }  // Enables editting of the speed in which the day cycle completes
        public static ConfigEntry<int> companyDeadline { get; set; } // Enables editting of the deadline (Days to Reach Quota)
        public static ConfigEntry<int> companyCurrentQuota { get; set; } // Enables editting of the current profits (ServerCurrentQuota/ServerQuota)
        public static ConfigEntry<int> companyQuota { get; set; }    // Enables editting of the profit quota (ServerCurrentQuota/ServerQuota)
        public static ConfigEntry<int> companyStartingCredits { get; set; } // Enables editting of starting credits
        public static ConfigEntry<int> companyCredits {  get; set; } // Enables editting of current credits

        public static ConfigEntry<int> itemSlots { get; set; }  // Enables editting of the number of items a player can hold
        public static ConfigEntry<int> serverTimeHours { get; set; }
    }
}