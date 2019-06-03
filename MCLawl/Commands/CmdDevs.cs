using System;

namespace MCWorlds
{
    public class CmdDevs : Command
    {
        public override string name { get { return "devs"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdDevs() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }
            string devlist = "";
            string temp;
            foreach (string dev in Server.devs)
            {
                temp = dev.Substring(0, 1);
                temp = temp.ToUpper() + dev.Remove(0, 1);
                devlist += temp + ", ";
            }
            devlist = devlist.Remove(devlist.Length - 2);
            Player.SendMessage(p, "&9Développeurs du serveur : " + Server.DefaultColor + devlist);
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/devs - Donne la liste des developpeurs du serveur.");
        }
    }
}