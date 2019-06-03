using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdLastCmd : Command
    {
        public override string name { get { return "lastcmd"; } }
        public override string shortcut { get { return "last"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdLastCmd() { }

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                foreach (Player pl in Player.players)
                {
                    if (pl.lastCMD == "") { continue; }
                    Player.SendMessage(p, pl.color + pl.name + Server.DefaultColor + " a utilise dernierement \"" + pl.lastCMD + "\"");
                }
            }
            else
            {
                Player who = Player.Find(message);
                if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur entre"); return; }
                Player.SendMessage(p, who.color + who.name + Server.DefaultColor + " a utilise dernierement \"" + who.lastCMD + "\"");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/last - Liste la derniere commande utilisee de chaque joueur");
            Player.SendMessage(p, "/last [joueur] - Affiche la derniere commande utilise par le joueur");
        }
    }
}