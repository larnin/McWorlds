
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MCWorlds
{
    public class CmdDislike : Command
    {
        public override string name { get { return "dislike"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdDislike() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }

            DataTable Likes = MySQL.fillData("SELECT * FROM `Like" + p.level.name + "." + p.level.world + "` WHERE Name= '" + p.name.ToLower() + "'");

            if (Likes.Rows.Count == 0) { Player.SendMessage(p, "Vous n'aimez pas cette map"); return; }
            else
            {
                MySQL.executeQuery("DELETE FROM `Like" + p.level.name + "." + p.level.world + "` WHERE Name='" + p.name.ToLower() + "'");
                
                Player.GlobalMessageLevel(p.level, "*" + p.color + p.Name() + Server.DefaultColor + " n'aime plus cette map");

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

                    if (numlvl <= 10)
                    {
                        topLike[numlvl].name = l.Split(' ')[0];
                        topLike[numlvl].world = l.Split(' ')[1];
                        try { topLike[numlvl].nbLikes = int.Parse(l.Split(' ')[2]); }
                        catch { topLike[numlvl].nbLikes = 0; }
                        numlvl++;
                    }
                }

                int lastpos = -1;
                for (int i = 0; i < 10; i++)
                {
                    if (topLike[i].name == p.level.name && topLike[i].world == p.level.world)
                    { lastpos = i;}
                } 
                
                if (lastpos != -1)
                {
                    for (int i = lastpos; i <= 8; i++)
                    {
                        topLike[i].name = topLike[i + 1].name;
                        topLike[i].world = topLike[i + 1].world;
                        topLike[i].nbLikes = topLike[i + 1].nbLikes;
                    }
                
                    int newpos = -1;
                    for (int i = 0; i < 10; i++)
                    {
                        if (nbLikes > topLike[i].nbLikes)
                        { newpos = i; break; }
                    }

                    if (newpos != -1)
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
                }
            }
            Likes.Dispose();

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/dislike - Permet de ne plus aimer une map.");
            Player.SendMessage(p, "Si vous aimez une map utilisez /like");
            Player.SendMessage(p, "/like top - Permet de voir le top 10 des maps les plus aimees");
            Player.SendMessage(p, "/like list - Affiche le nom des joueurs aimant la map");
        }
    }
    struct lvlinfo
    {
        public string name;
        public string world;
        public int nbLikes;
    }
}