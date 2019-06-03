using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdVips : Command
    {
        public override string name { get { return "vips"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdVips() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("text")) { Directory.CreateDirectory("text"); }

            if (!File.Exists("text/vips.txt")) { File.Create("text/vips.txt"); }

            if (message != "" || message.Split(' ').Length != 2) { Help(p); return; }

            if (message.Split(' ')[0] == "end")
            {
                string name = message.Split(' ')[1].ToLower();

                foreach (string l in File.ReadAllLines("text/vips.txt"))
                {
                    if (l == "") { continue; }
                    if (l[0] == '#') { continue; }

                    if (l.Split(' ')[0].ToLower().IndexOf(name) != -1)
                    { Player.SendMessage(p, l); }
                }
            }
            else if (message != "") { Help(p); return; }

            string vips = "";

            foreach (string l in File.ReadAllLines("text/vips.txt"))
            {
                if (l == "") { continue; }
                if (l[0] == '#') { continue; }

                vips += l.Split(' ')[0] + ", ";
            }

            if (vips.Length > 2)
            { Player.SendMessage(p, "Joueurs vip : &2" + vips.Remove(vips.Length - 2)); }
            else
            { Player.SendMessage(p, "Il n'y a pas de vips sur le serveur"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/vips - Liste tous les joueurs vips.");
            Player.SendMessage(p, "/vips end <name> - Donne la date de fin du compte vip du joueur");
            Player.SendMessage(p, "C'est grace a leurs dons que le serveur reste ouvert");
        }
    }
}