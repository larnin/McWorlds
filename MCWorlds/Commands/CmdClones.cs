using System;
using System.Collections.Generic;
using System.Data;

namespace MCWorlds
{
    class CmdClones : Command
    {

        public override string name { get { return "clones"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdClones() { }

        public override void Use(Player p, string message)
        {
            if (message == "" && p == null) { Player.SendMessage(p, "Donnez un nom de joueur"); return; }
            if (message == "") message = p.name;

            string originalName = message.ToLower();

            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "Ne trouve pas le joueur. Recherche joueur.");

                DataTable FindIP = MySQL.fillData("SELECT IP FROM Players WHERE Name='" + message + "'");

                if (FindIP.Rows.Count == 0) { Player.SendMessage(p, "Impossible de trouver un joueur par le nom entre."); FindIP.Dispose(); return; }

                message = FindIP.Rows[0]["IP"].ToString();
                FindIP.Dispose();
            }
            else
            {
                message = who.ip;
            }

            DataTable Clones = MySQL.fillData("SELECT Name FROM Players WHERE IP='" + message + "'");

            if (Clones.Rows.Count == 0) { Player.SendMessage(p, "Ne trouve pas de clones au joueur."); return; }

            List<string> foundPeople = new List<string>();
            for (int i = 0; i < Clones.Rows.Count; ++i)
            {
                if (!foundPeople.Contains(Clones.Rows[i]["Name"].ToString().ToLower()))
                    foundPeople.Add(Clones.Rows[i]["Name"].ToString().ToLower());
            }

            Clones.Dispose();
            if (foundPeople.Count <= 1) { Player.SendMessage(p, originalName + " n'a pas de clones."); return; }

            Player.SendMessage(p, "Ces personnes ont la meme adresse IP:");
            Player.SendMessage(p, string.Join(", ", foundPeople.ToArray()));
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/clones [nom] - Trouve les joueur qui ont la meme IP que [nom]");
        }
    }
}
