namespace LethalUtils
{
    // Class to hold all necessary settings set by the player
    internal class ServerChangeables
    {
        // Establish Global Variables for Settings
        private string serverName;  // Enables editting of server name
        private float serverTime;   // Enables editting of current time in game
        private float serverTimeSpeed;  // Enables editting of the speed in which the day cycle completes
        private int serverDeadline; // Enables editting of the deadline (Days to Reach Quota)
        private int serverCurrentQuota; // Enables editting of the current profits (ServerCurrentQuota/ServerQuota)
        private int serverQuota;    // Enables editting of the profit quota (ServerCurrentQuota/ServerQuota)
        private int itemSlots;  // Enables editting of the number of items a player can hold



        // Getters
        public string GetServerName() { return serverName; }
        public float GetServerTime() {  return serverTime; }
        public float GetServerTimeSpeed() {  return serverTimeSpeed; } 
        public int GetServerDeadline() {  return serverDeadline; }
        public int GetServerCurrentQuota() { return serverCurrentQuota; }
        public int GetServerQuota() {  return serverQuota; }
        public int GetItemSlots() { return itemSlots; }

        // Setters
        public void SetServerName(string serverName) {  this.serverName = serverName; }
        public void SetServerTime(float serverTime) { this.serverTime = serverTime; }
        public void SetServerTimeSpeed(float serverTimeSpeed) { this.serverTimeSpeed = serverTimeSpeed; }
        public void SetServerDeadline(int serverDeadline) { this.serverDeadline = serverDeadline; }
        public void SetServerCurrentQuota(int serverCurrentQuota) { this.serverCurrentQuota = serverCurrentQuota; }
        public void SetServerQuota(int serverQuota) { this.serverQuota = serverQuota; }
        public void SetItemSlots(int itemSlots) { this.itemSlots = itemSlots; }


    }
}