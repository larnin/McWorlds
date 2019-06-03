using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdRules : Command
    {
        public override string name { get { return "rules"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdRules() { }

        public override void Use(Player p, string message)
        {
            if (!File.Exists("text/rules.txt"))
            {
                File.WriteAllText("text/rules.txt", "No rules entered yet!");
            }
            if (!Directory.Exists("text/rules")) { Directory.CreateDirectory("text/rules"); }

            if (message.Split(' ').Length > 2) { Help(p); return; }

            string fichier = "";
            Player who = p ;

            if (message != "")
            {
                if (message.Split(' ').Length == 1)
                {
                    if (File.Exists("text/rules/" + message + ".txt"))
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
                    if (File.Exists("text/rules/" + message.Split(' ')[0] + ".txt"))
                    { fichier = message.Split(' ')[0]; }
                    else { Player.SendMessage(p, "Fichier introuvable"); return; }

                    who = Player.Find(message.Split(' ')[1]);
                    if (who == null)
                    { Player.SendMessage(p, "Le joueur '" + message.Split(' ')[1] + "' est introuvable"); return; }
                }
            }

            string[] lignes;

            if (fichier == "")
            { lignes = File.ReadAllLines("text/rules.txt"); }
            else
            { lignes = File.ReadAllLines("text/rules/" + fichier + ".txt"); }

            if (p != who)
            { Player.SendMessage(p, "Rules envoye a " + who.name); }

            foreach (string l in lignes)
            { Player.SendMessage(who, l); }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/rules <file> <joueur>- Affiche les rules");
            Player.SendMessage(p, "Si un joueur est indique, vous lui envoyez les rules");
        }
    }
}
