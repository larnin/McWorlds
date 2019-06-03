
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdMaptchat : Command
    {
        public override string name { get { return "maptchat"; } }
        public override string shortcut { get { return "mchat"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMaptchat() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.tchatmap)
            {
                p.tchatmap = false;
                Player.SendMessage(p, "Vous parlez maintenant a tout le serveur.");
            }
            else
            {
                if (p.opchat)
                {
                    Player.SendMessage(p, "Vous avez l'optchat d'active, desactivez le avant.");
                    return;
                }
                p.tchatmap = true;
                Player.SendMessage(p, "Vous parlez exclusivement aux personnes de la map.");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/maptchat - Les messages ecrit seront seulement envoye a la map.");
        }
    }
}
