using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdFAQ : Command
    {
        public override string name { get { return "faq"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdFAQ() { }

        public override void Use(Player p, string message)
        {
            if (!File.Exists("text/faq.txt"))
            {
                File.WriteAllText("text/faq.txt", "Pas de FAQ existante !");
            }
            
            if (!Directory.Exists("text/faq")) { Directory.CreateDirectory("text/faq"); }

            if (message.Split(' ').Length > 2) { Help(p); return; }

            string fichier = "";
            Player who = p;

            if (message != "")
            {
                if (message.Split(' ').Length == 1)
                {
                    if (File.Exists("text/faq/" + message + ".txt"))
                    { fichier = message; who = p; }
                    else
                    {
                        who = Player.Find(message); ;
                        if (who == null)
                        { Player.SendMessage(p, "Le joueur '" + message + "' est introuvable"); return; }
                    }
                }
                else if (message.Split(' ').Length == 2)
                {
                    if (File.Exists("text/faq/" + message.Split(' ')[0] + ".txt"))
                    { fichier = message.Split(' ')[0]; }
                    else { Player.SendMessage(p, "Fichier introuvable"); return; }

                    who = Player.Find(message.Split(' ')[1]);
                    if (who == null)
                    { Player.SendMessage(p, "Le joueur '" + message.Split(' ')[1] + "' est introuvable"); return; }
                }
            }

            string[] lignes;

            if (fichier == "")
            { lignes = File.ReadAllLines("text/faq.txt"); }
            else
            { lignes = File.ReadAllLines("text/faq/" + fichier + ".txt"); }

            if (p != who)
            { Player.SendMessage(p, "FAQ envoye a " + who.name); }

            foreach (string l in lignes)
            { Player.SendMessage(who, l); }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/faq <file> <joueur> - Affiche la FAQ (foire aux questions)");
            Player.SendMessage(p, "Si un joueur est indique, vous lui envoyez la FAQ");
            Player.SendMessage(p, "Lisez bien la FAQ avant de poser une question.");
        }
    }
}
