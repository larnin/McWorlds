using System;

namespace MCWorlds
{
    class CmdHeartbeat : Command
    {
        public override string name { get { return "heartbeat"; } }
        public override string shortcut { get { return "beat"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Nobody; } }
        public CmdHeartbeat() { }

        public override void Use(Player p, string message)
        {
            try
            {
                Heartbeat.Pump(Beat.Minecraft);
            }
            catch (Exception e)
            {
                Server.s.Log("Error with MCWorlds pump.");
                Server.ErrorLog(e);
            }
            Player.SendMessage(p, "Heartbeat pump sent.");
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/heartbeat - Forces a pump for the MCWorlds heartbeat.  DEBUG PURPOSES ONLY.");
        }
    }
}
