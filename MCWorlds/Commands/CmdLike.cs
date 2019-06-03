
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MCWorlds
{
    public class CmdLike : Command
    {
        public override string name { get { return "like"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdLike() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("text")) { Directory.CreateDirectory("text"); }
            if (!File.Exists("text/like.txt")) { File.Create("text/like.txt"); }

            if (message == "top")
            {
                string[] lignes = File.ReadAllLines("text/like.txt");
                int numLigne = -1, rangMap = 0, nbLikes = 0;
                string name, world;
                
                Player.SendMessage(p,"Top 10 des maps (note par vous)");
                while (rangMap < 10 && numLigne < lignes.Length-1)
                {
                    numLigne++;
                    if ( lignes[numLigne] == "") { continue; }
                    if (lignes[numLigne][0] == '#' || lignes[numLigne].Split(' ').Length < 3) { continue; }
                    try 
                    {
                        name = lignes[numLigne].Split(' ')[0];
                        world = lignes[numLigne].Split(' ')[1];
                        nbLikes = int.Parse(lignes[numLigne].Split(' ')[2]);
                    }
                    catch { continue; }

                    rangMap++;

                    if (rangMap == 1) { Player.SendMessage(p, "1ere : Map &a" + name + Server.DefaultColor + " (&a" + world + Server.DefaultColor + "), score : " + nbLikes); }
                    else { Player.SendMessage(p, rangMap + "eme : Map &a" + name + Server.DefaultColor + " (&a" + world + Server.DefaultColor + "), score : " + nbLikes); }
                }

                if (rangMap == 0) { Player.SendMessage(p, "Aucunes maps dans le top 10"); }
                else { Player.SendMessage(p, "Pour aller dans une map utilisez '/g map world'"); }

                return;
            }

            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "list")
            {
                DataTable likeDB = MySQL.fillData("SELECT * FROM `Like" + p.level.name + "." + p.level.world + "`");
                
                string playersLike = "";

                for (int i = 0; i < likeDB.Rows.Count; i++)
                {playersLike += ", " + likeDB.Rows[i]["Name"].ToString();}

                if (playersLike == "") { Player.SendMessage(p, "Personne n'aime cette map"); }
                else { Player.SendMessage(p, likeDB.Rows.Count + " personnes aiment cette map : " + playersLike.Remove(0, 2)); }
                likeDB.Dispose();
                return;
            }

            if (message == "reset")
            {
                if (p.group.Permission < LevelPermission.Admin) { Player.SendMessage(p, "Vous n'avez pas le rang pour faire ca"); }
                MySQL.executeQuery("DELETE FROM `Like" + p.level.name + "." + p.level.world + "`");

                string[] lignes = File.ReadAllLines("text/like.txt");

                string name = "", world = "";
                int nbLikes = 0;

                StreamWriter SW = new StreamWriter(File.Create("text/like.txt"));
                
                foreach (string l in lignes)
                {
                    try
                    {
                        nbLikes = int.Parse(l.Split(' ')[2]);
                        name = l.Split(' ')[0];
                        world = l.Split(' ')[1];

                        if (name != p.level.name || world != p.level.world) { SW.WriteLine(l); }
                    }
                    catch { SW.WriteLine(l); }
                }
                SW.Flush();
                SW.Close();
                SW.Dispose();

                Player.SendMessage(p, "Compteurs remis a zero");
                return;
            }

            if (message != "") { Help(p); return; }

            DataTable Likes = MySQL.fillData("SELECT * FROM `Like" + p.level.name + "." + p.level.world + "` WHERE Name= '" + p.name.ToLower() + "'");

            if (Likes.Rows.Count != 0) { Player.SendMessage(p, "Vous aimez deja la map"); }
            else
            {
                MySQL.executeQuery("INSERT INTO `Like" + p.level.name + "." + p.level.world + "` (Name) VALUES ('" + p.name.ToLower() + "')");

                Player.GlobalMessageLevel(p.level, "* " + p.color + p.Name() + Server.DefaultColor + " aime cette map");
                DataTable DB = MySQL.fillData("SELECT * FROM `Like" + p.level.name + "." + p.level.world + "`");
                int nbLikes = DB.Rows.Count; //comptage du nombre de likes
                DB.Dispose();

                string[] lignes = File.ReadAllLines("text/like.txt");

                lvlinfo lvl; lvl.name = ""; lvl.world = ""; lvl.nbLikes = 0;
                lvlinfo[] topLike = { lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl };
                
                int numlvl = 0;

                foreach (string l in lignes)
                {
                    if (l == "") { continue; }
                    if (l[0] == '#' || l.Split(' ').Length != 3) { continue; }

                    if (numlvl < 10)
                    {
                        topLike[numlvl].name = l.Split(' ')[0];
                        topLike[numlvl].world = l.Split(' ')[1];
                        try { topLike[numlvl].nbLikes = int.Parse(l.Split(' ')[2]); }
                        catch { topLike[numlvl].nbLikes = 0; }
                        numlvl++;
                    }
                }

                int lastpos = -1;
                for(int i = 0 ; i < 10 ; i++)
                {
                    if (topLike[i].name == p.level.name && topLike[i].world == p.level.world)
                    { lastpos = i; }
                }

                if (lastpos != -1)
                {
                    for (int i = lastpos; i <= 8; i++)
                    {
                        topLike[i].name = topLike[i + 1].name;
                        topLike[i].world = topLike[i + 1].world;
                        topLike[i].nbLikes = topLike[i + 1].nbLikes;
                    }
                }
                
                int newpos = -1;
                for (int i = 0; i < 10; i++)
                {
                    if (nbLikes > topLike[i].nbLikes)
                    { newpos = i; break; }
                }

                if ( newpos != -1)
                {
                    for (int i = 8; i >= newpos; i--)
                    {
                        topLike[i + 1].name = topLike[i].name;
                        topLike[i + 1].world = topLike[i].world;
                        topLike[i + 1].nbLikes = topLike[i].nbLikes;
                    }
                    topLike[newpos].name = p.level.name; topLike[newpos].world = p.level.world; topLike[newpos].nbLikes = nbLikes;
                }

                StreamWriter SW = new StreamWriter(File.Create("text/like.txt"));

                foreach (string s in lignes)
                { 
                    if (s == "") { continue; }
                    if (s[0] == '#') { SW.WriteLine(s); } 
                }

                SW.WriteLine("");

                foreach (lvlinfo f in topLike)
                { if (f.nbLikes != 0) { SW.WriteLine(f.name + " " + f.world + " " + f.nbLikes); } }

                SW.Flush();
                SW.Close();
                SW.Dispose();

                if (newpos != lastpos)
                {
                    if (newpos == 1)
                    { Player.GlobalChat(p, "La map &a" + p.level.name + Server.DefaultColor + " (&a" + p.level.world + Server.DefaultColor + ") passe 1ere du top 10 des meilleurs maps.", false); }
                    else
                    { Player.GlobalChat(p, "La map &a" + p.level.name + Server.DefaultColor + " (&a" + p.level.world + Server.DefaultColor + ") passe " + newpos + "eme du top 10 des meilleurs maps.", false); }
                }
                
            }
            Likes.Dispose();

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/like - Permet d'aimer une map.");
            Player.SendMessage(p, "/like top - Permet de voir le top 10 des maps les plus aimees");
            Player.SendMessage(p, "/like list - Affiche le nom des joueurs aimant la map");
            Player.SendMessage(p, "/like reset - Reinitialise les compteurs de la map (ADMIN+)");
        }

        struct lvlinfo
        {
            public string name;
            public string world;
            public int nbLikes;
        }
    }
}