using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdExamine : Command
    {
        public override string name { get { return "examine"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdExamine() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!p.examine)
            {
                p.examine = true;
                Player.SendMessage(p, "Vous examinez maintenant les mbs");
            }
            else
            {
                p.examine = false;
                Player.SendMessage(p, "Vous executez maintenant les mbs");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/Examine - Permet de voir les mbs au lieux de les executer");
        }
    }
}
