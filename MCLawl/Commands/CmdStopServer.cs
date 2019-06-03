using System;

namespace MCWorlds
{
    public class CmdStopServer : Command
    {
        public override string name { get { return "stopserver"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdStopServer() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }
            MCWorlds_.Gui.Program.ExitProgram(false);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/stopserver - arette le serveur.");
        }
    }
}