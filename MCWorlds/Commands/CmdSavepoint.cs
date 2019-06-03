
/* format Waitpoint :
 * posx posy posz rotx roty map world
 */

using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdSavePoint : Command
    {
        public override string name { get { return "savepoint"; } }
        public override string shortcut { get { return "sp"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSavePoint() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/savepoints")) { Directory.CreateDirectory("extra/savepoints"); }
            if (!File.Exists("extra/savepoints/" + p.name.ToLower() + ".txt")) { FileStream fs = File.Create("extra/savepoints/" + p.name.ToLower() + ".txt"); fs.Close(); }
            
            if (message == "") { message = "liste"; }
            if (message.Split(' ').Length > 2) { Help(p); return; }

            if (message.Split(' ')[0] == "add")
            {
                ushort x = (ushort)(p.pos[0] / 32);
                ushort y = (ushort)((p.pos[1] / 32) - 1);
                ushort z = (ushort)(p.pos[2] / 32);

                string[] lignes = File.ReadAllLines("extra/savepoints/" + p.name.ToLower() + ".txt");

                int numPoint = 0;
                StreamWriter SW = new StreamWriter(File.Create("extra/savepoints/" + p.name.ToLower() + ".txt"));
                foreach (string l in lignes)
                {
                    if (l[0] != '#' || l != "") { numPoint++; }

                    SW.WriteLine(l);
                }

                string point = x + " " + y + " " + z + " " + p.rot[0] + " " + p.rot[1] + " " + p.level.name + " " + p.level.world;
                SW.WriteLine(point);

                SW.Flush();
                SW.Close();
                SW.Dispose();

                Player.SendMessage(p, "Nouveau point cree : [&2" + numPoint + Server.DefaultColor + "] &2" + x + " " + y + " " + z);
                Player.SendMessage(p, "Map : &2" + p.level.name + Server.DefaultColor + ", monde : &2" + p.level.world);

                return;
            }

            if (message.Split(' ')[0] == "liste" || message.Split(' ')[0] == "list") 
            {
                int nbPoints = 0;

                string[] lignes = File.ReadAllLines("extra/savepoints/" + p.name.ToLower() + ".txt");

                foreach (string l in lignes)
                { if (l[0] != '#' && l != "") { nbPoints++; } }

                if (nbPoints == 0) { Player.SendMessage(p, "Aucun points existant"); return; }

                Player.SendMessage(p, "* " + nbPoints + " trouve");

                int x = 0, y = 0, z = 0, numPoint = 0;
                string map = "", world = "";

                foreach (string l in lignes)
                {
                    if (l[0] == '#' || l == "") { continue; }

                    if (l.Split(' ').Length != 7) { Player.SendMessage(p, "Point invalide"); continue; }

                    try
                    {
                        x = int.Parse(l.Split(' ')[0]);
                        y = int.Parse(l.Split(' ')[1]);
                        z = int.Parse(l.Split(' ')[2]);

                        map = l.Split(' ')[5];
                        world = l.Split(' ')[6];

                        Player.SendMessage(p, "[&2" + numPoint + Server.DefaultColor + "] Position : &2" + x + " " + y + " " + z + Server.DefaultColor + " Map : &2" + map + " (" + world + ")");
                        numPoint++;
                    }
                    catch { Player.SendMessage(p, "Point invalide"); continue; }
                }
                return;
            }

            if (message.Split(' ')[0] == "del")
            {
                if (message.Split(' ')[1] == "all")
                {
                    File.Delete("extra/savepoints/" + p.name.ToLower() + ".txt");
                    Player.SendMessage(p, "Tous les points sont supprimees");
                    return;
                }

                int numPoint = -1, numPointSuppr = 0 ;
                bool findpoint = false;

                try { numPointSuppr = int.Parse(message.Split(' ')[1]); }
                catch { Player.SendMessage(p, "Valeur incorrecte"); }

                string[] lignes = File.ReadAllLines("extra/savepoints/" + p.name.ToLower() + ".txt");

                StreamWriter SW = new StreamWriter(File.Create("extra/savepoints/" + p.name.ToLower() + ".txt"));

                foreach (string l in lignes)
                {
                    if (l != "" && l[0] != '#') { numPoint++; }

                    if (numPoint == numPointSuppr && !findpoint)
                    { 
                        findpoint = true;
                        Player.SendMessage(p, "Point supprime");
                    }
                    else { SW.WriteLine(l); }
                }

                SW.Flush();
                SW.Close();
                SW.Dispose();

                if (findpoint == false)
                { Player.SendMessage(p, "Aucun point a cet indice"); }
                return;
            }

            int nPoint = -1, wPoint = 0;
            bool sended = false;

            try { wPoint = int.Parse(message.Split(' ')[0]); }
            catch { Player.SendMessage(p, "valeur incorrecte"); return; }

            string[] allLines = File.ReadAllLines("extra/savepoints/" + p.name.ToLower() + ".txt");

            foreach (string l in allLines)
            {
                if (l != "" && l[0] != '#') { nPoint++; }

                if (nPoint == wPoint && !sended)
                {
                    sended = true;
                    sendPlayer(p, l);
                }
            } 
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/savepoint [num] - Permet de se teleporter vers un point definit precedement");
            Player.SendMessage(p, "/savepoint [add] - Cree un point a l'endroit ou vous etes");
            Player.SendMessage(p, "/savepoint [del] [num] - Supprime un point");
            Player.SendMessage(p, "/savepoint [del] all - Supprime tous les points");
            Player.SendMessage(p, "/savepoint [liste] - Liste tous vos points");
            Player.SendMessage(p, "N'oubliez pas de supprimer les points devenu innutile");
        }

        private void sendPlayer(Player p, string point)
        {
            if (point.Split(' ').Length != 7) { Player.SendMessage(p, "Point invalide"); return;}

            int x = 0, y = 0, z = 0, rotx = 0, roty = 0;
            int xx = 0, yy = 0, zz = 0;
            string map = "", world = "";

            try
            {
                x = int.Parse(point.Split(' ')[0]);
                y = int.Parse(point.Split(' ')[1]);
                z = int.Parse(point.Split(' ')[2]);

                xx = x * 32 + 16;
                yy = y * 32 + 32;
                zz = z * 32 + 16;
                
                rotx = int.Parse(point.Split(' ')[3]);
                roty = int.Parse(point.Split(' ')[4]);

                map = point.Split(' ')[5];
                world = point.Split(' ')[6];
            }
            catch { Player.SendMessage(p, "Point invalide"); return; }

            if (map != p.level.name || world != p.level.world)
            {
                try
                {
                    Command.all.Find("goto").Use(p, map + " " + world);

                    unchecked { p.SendPos((byte)-1, (ushort)xx, (ushort)yy, (ushort)zz, (byte)rotx, (byte)roty); }
                }
                catch { Player.SendMessage(p, "Coordonees invalides"); return; }

                Player.SendMessage(p, "Envoye vers &2 " + map + " (" + world + ") [" + x + ", " + y + ", " + z + "]");
            }
            else
            {
                try
                { unchecked { p.SendPos((byte)-1, (ushort)xx, (ushort)yy, (ushort)zz, (byte)rotx, (byte)roty); } }
                catch { Player.SendMessage(p, "Coordonees invalides"); return; }

                Player.SendMessage(p, "Envoye vers &2 " + map + " (" + world + ") [" + x + ", " + y + ", " + z + "]");
            }
        }
    }
}