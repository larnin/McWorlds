
/* format Waitpoint :
 * valeur posx posy posz rotx roty map world
 */

using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdSavePoint2 : Command
    {
        public override string name { get { return "savepoint2"; } }
        public override string shortcut { get { return "sp2"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSavePoint2() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/savepoints2")) { Directory.CreateDirectory("extra/savepoints2"); }
            if (!File.Exists("extra/savepoints2/" + p.name.ToLower() + ".txt")) { FileStream fs = File.Create("extra/savepoints2/" + p.name.ToLower() + ".txt"); fs.Close(); }
            
            if (message == "") { message = "liste"; }
            if (message.Split(' ').Length > 2) { Help(p); return; }

            if (message.Split(' ')[0] == "add")
            {
                if (message.Split(' ').Length != 2) { Help(p); return; }
                string value = message.Split(' ')[1];

                ushort x = (ushort)(p.pos[0] / 32);
                ushort y = (ushort)((p.pos[1] / 32) - 1);
                ushort z = (ushort)(p.pos[2] / 32);

                string[] lignes = File.ReadAllLines("extra/savepoints2/" + p.name.ToLower() + ".txt");
                foreach (string l in lignes)
                {
                    if (l == "") { continue; }
                    if (l[0] == '#') { continue; }

                    if (l.Split(' ')[0] == value) { Player.SendMessage(p, "Ce point existe deja, choisissez une autre valeur"); return; }
                }

                StreamWriter SW = new StreamWriter(File.Create("extra/savepoints2/" + p.name.ToLower() + ".txt"));
                foreach (string l in lignes)
                { SW.WriteLine(l); }

                string point = value + " " +  x + " " + y + " " + z + " " + p.rot[0] + " " + p.rot[1] + " " + p.level.name + " " + p.level.world;
                SW.WriteLine(point);

                SW.Flush();
                SW.Close();
                SW.Dispose();

                Player.SendMessage(p, "Nouveau point cree : [&2" + value + Server.DefaultColor + "] &2" + x + " " + y + " " + z);
                Player.SendMessage(p, "Map : &2" + p.level.name + Server.DefaultColor + ", monde : &2" + p.level.world);

                return;
            }

            if (message.Split(' ')[0] == "liste" || message.Split(' ')[0] == "list") 
            {
                int nbPoints = 0;

                string[] lignes = File.ReadAllLines("extra/savepoints2/" + p.name.ToLower() + ".txt");

                foreach (string l in lignes)
                { if (l[0] != '#' && l != "") { nbPoints++; } }

                if (nbPoints == 0) { Player.SendMessage(p, "Aucun points existant"); return; }

                Player.SendMessage(p, "* " + nbPoints + " trouve");

                int x = 0, y = 0, z = 0;
                string value = "", map = "", world = "";

                foreach (string l in lignes)
                {
                    if (l[0] == '#' || l == "") { continue; }

                    if (l.Split(' ').Length != 8) { Player.SendMessage(p, "Point invalide"); continue; }

                    try
                    {
                        value = l.Split(' ')[0];

                        x = int.Parse(l.Split(' ')[1]);
                        y = int.Parse(l.Split(' ')[2]);
                        z = int.Parse(l.Split(' ')[3]);

                        map = l.Split(' ')[4];
                        world = l.Split(' ')[5];

                        Player.SendMessage(p, "[&2" + value + Server.DefaultColor + "] Position : &2" + x + " " + y + " " + z + Server.DefaultColor + " Map : &2" + map + " (" + world + ")");
                    }
                    catch { Player.SendMessage(p, "Point invalide"); continue; }
                }
                return;
            }

            if (message.Split(' ')[0] == "del")
            {
                if (message.Split(' ')[1] == "all")
                {
                    File.Delete("extra/savepoints2/" + p.name.ToLower() + ".txt");
                    Player.SendMessage(p, "Tous les points sont supprimees");
                    return;
                }

                bool findpoint = false;
                string valuePoint = "";

                valuePoint = message.Split(' ')[1];

                string[] lignes = File.ReadAllLines("extra/savepoints2/" + p.name.ToLower() + ".txt");

                StreamWriter SW = new StreamWriter(File.Create("extra/savepoints2/" + p.name.ToLower() + ".txt"));

                foreach (string l in lignes)
                {
                    if (l != "")
                    {
                        if (l[0] != '#')
                        {
                            if (valuePoint == l.Split(' ')[0] && !findpoint)
                            {
                                findpoint = true;
                                Player.SendMessage(p, "Point supprime");
                            }
                            else { SW.WriteLine(l); }
                            continue;
                        }
                    }
                    SW.WriteLine(l);
                }

                SW.Flush();
                SW.Close();
                SW.Dispose();

                if (findpoint == false)
                { Player.SendMessage(p, "Aucun point a cet indice"); }
                return;
            }

            bool sended = false;
            string valuepoint = message.Split(' ')[0];

            string[] allLines = File.ReadAllLines("extra/savepoints2/" + p.name.ToLower() + ".txt");

            foreach (string l in allLines)
            {
                if (l == "") { continue; }
                if (l[0] == '#') { continue; }

                if (valuepoint == l.Split(' ')[0] && !sended)
                {
                    sended = true;
                    sendPlayer(p, l);
                }
            } 
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/savepoint2 [valeur] - Permet de se teleporter vers un point definit precedement");
            Player.SendMessage(p, "/savepoint2 [add] [valeur] - Cree un point a l'endroit ou vous etes");
            Player.SendMessage(p, "/savepoint2 [del] [valeur/all] - Supprime un ou tous les points");
            Player.SendMessage(p, "/savepoint2 [liste] - Liste tous vos points");
            Player.SendMessage(p, "N'oubliez pas de supprimer les points devenu innutile");
        }

        private void sendPlayer(Player p, string point)
        {
            if (point.Split(' ').Length != 8) { Player.SendMessage(p, "Point invalide"); return;}

            int x = 0, y = 0, z = 0, rotx = 0, roty = 0;
            int xx = 0, yy = 0, zz = 0;
            string map = "", world = "";

            try
            {
                x = int.Parse(point.Split(' ')[1]);
                y = int.Parse(point.Split(' ')[2]);
                z = int.Parse(point.Split(' ')[3]);

                xx = x * 32 + 16;
                yy = y * 32 + 32;
                zz = z * 32 + 16;
                
                rotx = int.Parse(point.Split(' ')[4]);
                roty = int.Parse(point.Split(' ')[5]);

                map = point.Split(' ')[6];
                world = point.Split(' ')[7];
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