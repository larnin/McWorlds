
using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdNews : Command
    {
        public override string name { get { return "news"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdNews() { }

        public override void Use(Player p, string message)
        {
            List<string> rules = new List<string>();
            if (!File.Exists("text/news.txt"))
            {
                File.WriteAllText("text/news.txt", "Aucunes nouveautees");
            }
            StreamReader r = File.OpenText("text/news.txt");
            while (!r.EndOfStream)
                rules.Add(r.ReadLine());

            r.Close();
            r.Dispose();

            Player who = null;
            if (message != "")
            {
                if (p.group.Permission <= LevelPermission.Builder)
                { Player.SendMessage(p, "Vous ne pouvez pas envoyer les rules a d'autres joueurs!"); return; }
                who = Player.Find(message);
                if (who == null) { Player.SendMessage(p, "Il n'y a pas de joueur \"" + message + "\"!"); return; }
            }
            else
            {
                who = p;
            }


            if (p != null) { if (who != p) { Player.SendMessage(p, "News envoye a " + who.color + who.name); } }
            Player.SendMessage(who,"News :");
            foreach (string s in rules) { Player.SendMessage(who,s); }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/news <joueur> - Affiche les nouveautees du serveur");
        }
    }
}
