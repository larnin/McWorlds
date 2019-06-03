using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdGamemode : Command
    {
        public override string name { get { return "gamemode"; } }
        public override string shortcut { get { return "gm"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdGamemode() { }

        public override void Use(Player p, string message)
        {
            if (message == "creative")
            {
                if (!p.gamemode) { Player.SendMessage(p, "Vous etes deja en creative"); }
                p.gamemode = false;
                Player.SendMessage(p, "Vous pouvez maintenant casser et placer des blocs");
                return;
            }
            if (message == "aventure")
            {
                if (p.gamemode) { Player.SendMessage(p, "Vous etes deja en mode aventure"); }
                p.gamemode = true;
                Player.SendMessage(p, "Vous ne pouvez qu'interagir avec les doors et les mbs");
                return;
            }
            Help(p);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/gamemode <creative/aventure> - Permet de changer votre mode de jeu");
            Player.SendMessage(p, "En mode aventure, vous ne pouvez pas casser de bloc");
        }
    }
}
