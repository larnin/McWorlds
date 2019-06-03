using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdAccept : Command
    {
        public override string name { get { return "accept"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdAccept() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.rulesAccepted)
            { Player.SendMessage(p, "Vous avez deja accepte les rules"); return; }

            p.rulesAccepted = true;

            Player.SendMessage(p, "Rules accepte, relisez les de temps en temps, elle peuvent changer a tout moment");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/accept - accepte les rules.");
        }
    }
}
