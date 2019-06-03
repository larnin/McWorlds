using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdSalon : Command
    {
        public override string name { get { return "salon"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSalon() { }

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                Player.SendMessage(p, "Il y a " + Server.listeSalon.Count + " salons");

                Group g = null; 
                string gs = "";

                foreach (Server.salon s in Server.listeSalon)
                {
                    g = Group.findPerm(s.perm);
                    if (g == null) { gs = s.perm.ToString(); }
                    else { gs = g.name; }
                    if (s.closed) { Player.SendMessage(p, "> > &a" + s.name + Server.DefaultColor + ", ferme"); }
                    else { Player.SendMessage(p, "> > &a" + s.name + Server.DefaultColor + ", permition &a" + gs); }
                }
                return;
            }

            string action = message.Split(' ')[0];
            if ((action == "join" || action == "leave" || action == "speek") && p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p != null)
            {
                if (p.group.Permission < LevelPermission.Operator && (action == "create" || action == "del" || action == "modify"))
                { Player.SendMessage(p, "Vous n'avez pas le rang pour faire ca"); return; }
            }

            if (action == "join")
            {
                if (message.Split(' ').Length != 2) { Help(p); return; }

                Server.salon sa = Server.findSalon(message.Split(' ')[1]);
                if (sa.name == "") { Player.SendMessage(p, "Le salon '" + message.Split(' ')[1] + "' n'existe pas"); return; }

                if (p.salon == sa.name) { Player.SendMessage(p, "Vous etes deja dans ce salon"); return;}

                if (sa.closed) { Player.SendMessage(p, "Ce salon est ferme"); return; }

                if (p.group.Permission < sa.perm) { Player.SendMessage(p, "Vous ne pouvez pas rentrer dans ce salon"); return; }

                if (p.salon != "") { Use(p, "leave"); }
                
                Player.GlobalSalon(sa, p.color + "*" + p.Name() + Server.DefaultColor + " join le salon", false);
                Player.SendMessage(p, "Vous joignez le salon &5" + sa.name); 

                sa.pliste.Add(p);
                p.salon = sa.name; 
            }
            else if (action == "leave")
            {
                if (message.IndexOf(' ') != -1) { Help(p); return; }

                if (p.salon == "") { Player.SendMessage(p, "Vous n'etes dans aucun salon"); return; }
                
                Server.salon sa = Server.findSalon(p.salon);
                if (sa.name == "") { Player.SendMessage(p, "Salon actuel inconnu"); p.salon = ""; return; }

                p.salon = "";
                sa.pliste.Remove(p);
                p.salonSpeek = false;

                Player.SendMessage(p, "Vous quittez le salon &5" + sa.name); 
                Player.GlobalSalon(sa, p.color + "*" + p.Name() + Server.DefaultColor + " quitte le salon",false);
            }
            else if (action == "speek")
            {
                if (message.IndexOf(' ') != -1) { Help(p); return; }

                if (p.salon == "") { Player.SendMessage(p, "Vous n'etes pas dans un salon"); return; }

                p.salonSpeek = !p.salonSpeek;

                if (p.salonSpeek) { Player.SendMessage(p, "Tous les messages ecrit seront envoye au salon"); }
                else { Player.SendMessage(p, "Vous parlez maintenant a tout le serveur"); } 
            }
            else if (action == "who")
            {
                Server.salon sa;

                if (message.IndexOf(' ') == -1)
                {
                    if (p.salon == "") { Player.SendMessage(p, "Vous n'etes pas dans un salon"); }
                    sa = Server.findSalon(p.salon);
                    if (sa == null) { Player.SendMessage(p, "Salon actuel inconnu"); p.salon = ""; return; }
                }
                else
                {
                    sa = Server.findSalon(message.Split(' ')[1]);
                    if (sa == null) { Player.SendMessage(p, "Le salon '" + message.Split(' ')[1] + "' n'existe pas"); return; }
                }

                string allJoueurs = "";
                foreach (Player who in sa.pliste)
                {
                    if (who == null) { continue; }
                    allJoueurs += who.name + ", "; 
                }
                Player.SendMessage(p, "Il y a &a" + sa.pliste.Count + Server.DefaultColor + " joueurs dans le salon &a" + sa.name);
                if (allJoueurs != "") { Player.SendMessage(p, allJoueurs.Remove(allJoueurs.Length - 2)); }
            }
            else if (action == "create")
            {
                if (message.Split(' ').Length < 2) { Help(p); return; }

                string name = message.Split(' ')[1];
                string perm = "";
                if (message.Split(' ').Length == 3) { perm = message.Split(' ')[2]; }
                else { perm = "30"; }

                Server.salon sa = Server.findSalon(name);
                if (sa != null) { Player.SendMessage(p, "Un salon au meme nom existe deja"); return; }
                
                LevelPermission saPerm;
                try { saPerm = (LevelPermission)int.Parse(perm); }
                catch
                {
                    Group g = Group.Find(perm);
                    if (g == null) { Player.SendMessage(p, "Rang '" + perm + "' inconnu"); return; }
                    saPerm = g.Permission;
                }

                if (p != null) { if (p.group.Permission < sa.perm) { Player.SendMessage(p, "Vous ne pouvez pas creer un salon avec ces permitions"); return; } }

                Server.salon salon = new Server.salon();

                Server.listeSalon.Add(salon);
                save();

                Player.SendMessage(p, "Salon cree");
            }
            else if (action == "del")
            {
                bool inSalon = false;
                if (message.Split(' ').Length != 2) { Help(p); return; }

                Server.salon sa = Server.findSalon(message.Split(' ')[1]);
                if ( sa.name == "") {Player.SendMessage(p, "Le salon '" + message.Split(' ')[1] + "' n'existe pas"); return; }

                if (p != null) { if (p.group.Permission < sa.perm) { Player.SendMessage(p, "Vous ne pouvez pas supprimer ce salon"); return; } }

                if (sa.pliste != null)
                {
                    foreach (Player who in sa.pliste)
                    {
                        if (who == p) { inSalon = true; }
                        who.salon = ""; who.salonSpeek = false;
                        Player.SendMessage(who, "Salon supprime");
                    }
                }
                Server.listeSalon.Remove(sa);
                save();

                if (!inSalon) { Player.SendMessage(p, "Salon supprime"); }
            }
            else if (action == "modify")
            {
                if (message.Split(' ').Length != 4) { Help(p); return; }
                
                Server.salon sa = Server.findSalon(message.Split(' ')[1]);
                if (sa.name == "") { Player.SendMessage(p, "Le salon '" + message.Split(' ')[1] + "' n'existe pas"); return; }

                if (p != null) { if (p.group.Permission < sa.perm) { Player.SendMessage(p, "Vous ne pouvez pas modifier ce salon"); return; } }

                string propriete = message.Split(' ')[2];
                string value = message.Split(' ')[3];

                if (propriete == "name")
                {
                    Server.salon sal = Server.findSalon(value);
                    if (sal != null) { Player.SendMessage(p, "Un salon au meme nom existe deja"); return; }

                    sa.name = value;

                    if (sa.pliste != null)
                    {
                        foreach (Player who in sa.pliste)
                        { who.salon = value; }
                    }
                    save();

                    Player.GlobalSalon(sa, "Salon renome en &a" + value, false);
                    if (p != null) { if (p.salon != value) { Player.SendMessage(p, "Salon renome en &a" + value); } }
                    else { Player.SendMessage(p, "Salon renome en &a" + value); }
                }
                else if (propriete == "perm")
                {
                    LevelPermission saPerm;
                    try { saPerm = (LevelPermission)int.Parse(value); }
                    catch
                    {
                        Group g = Group.Find(value);
                        if (g == null) { Player.SendMessage(p, "Rang '" + value + "' inconnu"); return; }
                        saPerm = g.Permission;
                    }

                    if (p != null) { if (p.group.Permission < saPerm) { Player.SendMessage(p, "Vous ne pouvez pas appliquer ces permitions a ce salon"); return; } }

                    sa.perm = saPerm;

                    save();

                    Player.GlobalSalon(sa, "Permission change a &a" + value, false);
                    if (p != null) { if (p.salon != value) { Player.SendMessage(p, "Permission change a &a" + value); } }
                    else { Player.SendMessage(p, "Permission change a &a" + value); }
                }
                else if (propriete == "closed")
                {
                    bool c = false;
                    try { c = bool.Parse(value); }
                    catch { Player.SendMessage(p, "Valeur incorrecte"); }

                    sa.closed = c;

                    if (c) { Player.GlobalSalon(sa, "Le salon n'est plus joignable", false); }
                    else { Player.GlobalSalon(sa, "Le salon est maintenant joignable", false); }
                }
                else { Player.SendMessage(p, "Propriete inconnue"); Help(p); return; }
            }
            else { Player.SendMessage(p, "Action inconnue"); return; }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/salon - Liste tous les salons");
            Player.SendMessage(p, "/salon who <nom salon> - Liste les joueurs present dans le salon");
            Player.SendMessage(p, "/salon speek - Permet de ne parler qu'au salon");
            Player.SendMessage(p, "/salon join [name] - Joindre un salon");
            Player.SendMessage(p, "/salon leave - Quitter un salon");
            Player.SendMessage(p, "/salon create [name] <permition> - Creer un salon (MODO+)");
            Player.SendMessage(p, "/salon del [name] - Supprimer un salon (MODO+)");
            Player.SendMessage(p, "/salon modify [name] [propriete] [valeur] - Modifier un salon (MODO+)");
            Player.SendMessage(p, "Propriete : name - Changer le nom du salon");
            Player.SendMessage(p, "perm - Change la permition pour acceder au salon");
            Player.SendMessage(p, "closed - Fermer l'acces au salon");
        }

        public void save()
        {
            if (!Directory.Exists("text")) { Directory.CreateDirectory("text"); }
            if (!File.Exists("text/salon.txt")) { File.Create("text/salon.txt"); }

            string[] allLines = File.ReadAllLines("text/salon.txt");

            StreamWriter SW = new StreamWriter(File.Create("text/salon.txt"));
            foreach (string line in allLines)
            {
                if (line == "") { continue; }
                if (line[0] == '#') { SW.WriteLine(line); }
            }

            foreach (Server.salon sa in Server.listeSalon)
            {
                SW.WriteLine(sa.name + " " + (int)sa.perm);
            }
            SW.Flush();
            SW.Close();
            SW.Dispose();
        }
    }
}