using System;
using System.Collections.Generic;
using MCWorlds.Gui;

namespace MCWorlds
{
    public class CmdRestart : Command
    {
        public override string name { get { return "restart"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdRestart() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }
            MCWorlds_.Gui.Program.restartMe();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/restart - Redemarre le serveur!");
        }
    }
}
