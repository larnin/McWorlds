
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdViewRanks : Command
    {
        public override string name { get { return "viewranks"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdViewRanks() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            Group foundGroup = Group.Find(message);
            if (foundGroup == null)
            {
                Player.SendMessage(p, "Impossible de trouver le groupe");
                return;
            }


            string totalList = "";
            foreach (string s in foundGroup.playerList.All())
            {
                totalList += ", " + s;
            }

            if (totalList == "")
            {
                Player.SendMessage(p, "Personne a le rang de " + foundGroup.color + foundGroup.name);
                return;
            }
            
            Player.SendMessage(p, "Les joueurs qui on le rang de " + foundGroup.color + foundGroup.name + ":");
            Player.SendMessage(p, totalList.Remove(0, 2));
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/viewranks [rang] - Affiche tous les joueurs qui ont le rang [rang]");
            Player.SendMessage(p, "Rangs disponibles: " + Group.concatList());
        }
    }
}
