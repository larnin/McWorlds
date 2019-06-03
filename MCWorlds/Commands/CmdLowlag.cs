using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdLowlag : Command
    {
        public override string name { get { return "lowlag"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdLowlag() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }

            if (Server.updateTimer.Interval > 1000)
            {
                Server.updateTimer.Interval = 100;
                Player.GlobalChat(null, "Le mode &dLow lag " + Server.DefaultColor + "a ete &cdesactive" + Server.DefaultColor + ".", false);
            }
            else
            {
                Server.updateTimer.Interval = 10000;
                Player.GlobalChat(null, "Le mode &dLow lag " + Server.DefaultColor + "a ete &aACTIVE" + Server.DefaultColor + ".", false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/lowlag - Active/desactive le mode lowlag");
            Player.SendMessage(p, "Utile pour les serveur sur des machines a petites performances");
        }
    }
}