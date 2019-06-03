using System;

namespace MCWorlds
{
    public class CmdTime : Command
    {
        public override string name { get { return "time"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdTime() { }

        public override void Use(Player p, string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            message = "L'heure du serveur est " + time;
            Player.SendMessage(p, message);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/time - Donne l'heure du serveur.");
        }
    }
}