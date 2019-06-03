using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdBienvenu : Command
    {
        public override string name { get { return "bienvenue"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBienvenu() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "delete")
            {
                if (!Directory.Exists("levels/" + p.name.ToLower()))
                { Player.SendMessage(p, "Votre monde n'existe pas, utilisez /home pour le creer"); return; }

                StreamWriter SW = new StreamWriter(File.Create("levels/" + p.name.ToLower() + "/bienvenu.txt"));
                SW.WriteLine("");
                SW.Flush();
                SW.Close();

                Player.SendMessage(p, "Message de bienvenue supprime");
                return;
            }

            if (message.Split(' ').Length <= 1)
            {
                string world = "" ;

                if (message == "")
                { world = p.level.world; }
                else
                { world = message; }

                if (!Directory.Exists("levels/" + world.ToLower()))
                { Player.SendMessage(p, "Le monde demande n'existe pas"); return; }

                if (!File.Exists("levels/" + world.ToLower() + "/bienvenu.txt"))
                { Player.SendMessage(p, "Ce monde n'a pas de message de bienvenue"); return; }

                foreach (string line in File.ReadAllLines("levels/" + world.ToLower() + "/bienvenu.txt"))
                {
                    if (line != "") { Player.SendMessage(p, line); }
                }
                return;
            }

            if (message.Split(' ')[0] == "add")
            {
                if (!Directory.Exists("levels/" + p.name.ToLower()))
                { Player.SendMessage(p, "Votre monde n'existe pas, utilisez /home pour le creer"); return; }

                string[] allLines = {""} ;

                if ( File.Exists("levels/" + p.name.ToLower() + "/bienvenu.txt"))
                { allLines = File.ReadAllLines("levels/" + p.name.ToLower() + "/bienvenu.txt");}

                StreamWriter SW = new StreamWriter(File.Create("levels/" + p.name.ToLower() + "/bienvenu.txt"));
                foreach (string l in allLines)
                {
                    SW.WriteLine(l);
                }
                SW.WriteLine(message.Substring(message.IndexOf(' ') + 1));
                SW.Flush();
                SW.Close();

                Player.SendMessage(p, "Message de bienvenue mis a jours");

                return;
            }
            
            if (message.Split(' ')[0] == "set")
            {
                if (!Directory.Exists("levels/" + p.name.ToLower()))
                { Player.SendMessage(p, "Votre monde n'existe pas, utilisez /home pour le creer"); return; }

                StreamWriter SW = new StreamWriter(File.Create("levels/" + p.name.ToLower() + "/bienvenu.txt"));
                SW.WriteLine(message.Substring(message.IndexOf(' ') + 1));
                SW.Flush();
                SW.Close();

                Player.SendMessage(p, "Message de bienvenue mis a jours");

                return;
            }

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/bienvenue - Permet de voir le message de bienvenue du monde actuel.");
            Player.SendMessage(p, "/bienvenue [monde] - Voir le message de bienvenue du monde demande");
            Player.SendMessage(p, "/bienvenue add [message] - Ajoute une ligne a ce message");
            Player.SendMessage(p, "/bienvenue set [message] - Change le message de bienvenue");
            Player.SendMessage(p, "/bienvenue delete - Supprime tout le contenu du message de bienvenue");
        }
    }
}