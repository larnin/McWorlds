using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdCrashServer : Command
    {
        public override string name { get { return "crashserver"; } }
        public override string shortcut { get { return "crash"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdCrashServer() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }
            Player.GlobalMessageOps(p.color + Server.DefaultColor + " a utilise &b/crashserver");
            p.Kick("Server crash! Error code 0x0005A4");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/crashserver - Crash le serveur avec une erreur generique");
        }
    }

}
