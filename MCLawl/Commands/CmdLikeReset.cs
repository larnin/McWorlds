
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
//using MySql.Data.MySqlClient;
//using MySql.Data.Types;

namespace MCWorlds
{
    public class CmdLikeReset : Command
    {
        public override string name { get { return "likereset"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdLikeReset() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("text")) { Directory.CreateDirectory("text"); }
            if (!File.Exists("text/like.txt")) { File.Create("text/like.txt"); }

            DirectoryInfo di = new DirectoryInfo("levels/");
            DirectoryInfo[] dir = di.GetDirectories();

            lvlinfo lvl; lvl.name = ""; lvl.world = ""; lvl.nbLikes = 0;
            lvlinfo[] topLike = { lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl, lvl };
            int nblvls = 0;

            foreach (DirectoryInfo d in dir)
            {
                FileInfo[] lvlFiles = d.GetFiles("*.lvl");

                foreach (FileInfo file in lvlFiles)
                {
                    nblvls++;
                    lvl.name = file.Name.Remove(file.Name.Length - 4);
                    lvl.world = d.Name;

                    try { lvl.nbLikes = MySQL.fillData("SELECT * FROM  `Like" + lvl.name + "." + lvl.world + "`").Rows.Count; }
                    catch { lvl.nbLikes = 0; }

                    if (lvl.nbLikes != 0)
                    {
                        int n = 0;
                        while (lvl.nbLikes < topLike[n].nbLikes) 
                        {
                            n++;
                            if (n == 10) { break; }
                        }
                        if (n != 10)
                        {
                            for (int i = 8; i >= n; i--)
                            {
                                topLike[i + 1].name = topLike[i].name;
                                topLike[i + 1].world = topLike[i].world;
                                topLike[i + 1].nbLikes = topLike[i].nbLikes;
                            }
                            topLike[n].name = lvl.name; topLike[n].world = lvl.world; topLike[n].nbLikes = lvl.nbLikes;
                        }
                    }
                }
            }
            StreamWriter SW = new StreamWriter(File.Create("text/likereset.txt"));
            for (int i = 0; i < 10; i++)
            {
                if (topLike[i].name == "" || topLike[i].world == "" || topLike[i].nbLikes <= 0) { continue; }
                SW.WriteLine(topLike[i].name + " " + topLike[i].world + " " + topLike[i].nbLikes);
            }
            SW.Flush();
            SW.Close();
            SW.Dispose();

            Player.SendMessage(p, "Levels : " + nblvls);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/likereset - Reset la liste des likes.");
        }
    }

    struct lvlinfo
    {
        public string name;
        public string world;
        public int nbLikes;
    }
}