using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdHacks : Command
    {
        public override string name { get { return "hacks"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdHacks() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }
            p.Kick("Votre IP a ete recupere et est livre au FBI contre les crimes internationnaux");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/hacks - HACK LE SERVEUR ET TOUT LE RESEAU INTERNET");
        }
    }

}
