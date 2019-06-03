using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdNotes : Command
    {
        public override string name { get { return "notes"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdNotes() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/notes")) { Directory.CreateDirectory("extra/notes"); }
            if (!File.Exists("extra/notes/" + p.name.ToLower() + ".txt")) { FileStream fs = File.Create("extra/notes/" + p.name.ToLower() + ".txt"); fs.Close(); }

            string fichier = p.name.ToLower();

            if (message.IndexOf(' ') == -1)
            {
                if (p == null)
                {
                    if (message == "") { Player.SendMessage(p, "Vous ne pouvez pas avoir de notes"); return; }
                    else { fichier = message.ToLower(); }
                }
                else if (message != "")
                {
                    if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Vous devez etre modo pour pouvoir lire les notes des autres"); return; }
                    if (!File.Exists("extra/notes/" + message.ToLower() + ".txt")) { Player.SendMessage(p, "Le joueur " + message + " n'a pas de notes"); return; }
                    fichier = message.ToLower();
                }

                string[] lignes = File.ReadAllLines("extra/notes/" + fichier + ".txt");

                bool contenu = false;
                int numLigne = 0;

                foreach (string l in lignes)
                {
                    if (l == "") { continue; }

                    contenu = true;
                    Player.SendMessage(p, "[" + numLigne + "] " + l);

                    numLigne++;
                }

                if (contenu == false)
                {
                    if (p == null) { Player.SendMessage(p, "Le joueur " + message + " n'a pas de notes"); return; }
                    if (fichier == p.name.ToLower()) { Player.SendMessage(p, "Vous n'avez pas de notes"); }
                    else { Player.SendMessage(p, "Le joueur " + message + " n'a pas de notes"); }
                }
                return;
            }

            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "del all")
            {
                File.Delete("extra/notes/" + p.name.ToLower() + ".txt");
                Player.SendMessage(p, "Notes supprimees");
                return;
            }
            string[] notes = File.ReadAllLines("extra/notes/" + p.name.ToLower() + ".txt");

            if (message.Split(' ')[0] == "add")
            {
                if (message.Split(' ').Length < 2) { Help(p); return; }
                StreamWriter SW = new StreamWriter(File.Create("extra/notes/" + p.name.ToLower() + ".txt"));
                foreach (string l in notes)
                { SW.WriteLine(l); }
                
                SW.WriteLine(message.Remove(0,message.IndexOf(' ')));

                SW.Flush();
                SW.Close();
                SW.Dispose();

                Player.SendMessage(p, "Notes ajoutees");
                return;
            }

            if (message.Split(' ')[0] == "del")
            {
                if (message.Split(' ').Length != 2) { Help(p); return; }
                int num = 0;
                try { num = int.Parse(message.Split(' ')[1]); }
                catch { Player.SendMessage(p, "Valeur incorecte"); return; }
                if ( num < 0) { Player.SendMessage(p, "Valeur incorecte"); return; }

                if (notes.Length < num) { Player.SendMessage(p, "Aucune note a cet indice"); return; }

                StreamWriter SW = new StreamWriter(File.Create("extra/notes/" + p.name.ToLower() + ".txt"));
                for (int i = 0; i < notes.Length; i++)
                {
                    if (i != num)
                    { SW.WriteLine(notes[i]); }
                }

                SW.Flush();
                SW.Close();
                SW.Dispose();
            }

            Help(p);
            return;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "Les notes est un pence bete personnel, pour marquer vos projets ou choses a faire");
            Player.SendMessage(p, "/notes - Permet de voir vos notes personnelles.");
            Player.SendMessage(p, "/notes [name] - Voir les notes d'un autre joueur (MODO+)");
            Player.SendMessage(p, "/notes add [message] - Ajoute une ligne a vos notes");
            Player.SendMessage(p, "/notes del [num] - Supprime la ligne indiquee");
            Player.SendMessage(p, "/notes del [all] - Supprime toutes les notes");
        }
    }
}