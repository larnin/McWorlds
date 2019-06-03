
using System;
using System.IO;

namespace MCWorlds
{
    public class CmdHome : Command
    {
        public override string name { get { return "home"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdHome() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if ( message != "" ) { Help(p); return;}

            if (!Directory.Exists("levels/" + p.name.ToLower())) { Directory.CreateDirectory("levels/" + p.name.ToLower()); }

            if ( !File.Exists("levels/" + p.name.ToLower() + "/spawn.lvl"))
            {

                if (!File.Exists("levels/spawn.lvl"))
                {
                    Server.s.Log("Fichier levels/spawn.lvl intouvable, création d'une map vierge");

                    Level lvl = new Level("spawn", 64, 64, 64, "flat");
                    lvl.world = p.name.ToLower();
                    lvl.Save(true);
                }
                else
                { File.Copy("levels/spawn.lvl", "levels/" + p.name.ToLower() + "/spawn.lvl"); }

                Server.s.Log("Creation du spawn de " + p.name.ToLower() + " termine");
                Player.GlobalMessage("Creation du monde de " + p.color + p.name + Server.DefaultColor + " termine");
                p.nbMaps++;
            }
            if ( !File.Exists("levels/" + p.name.ToLower() + "/bienvenu.txt"))
            {
                StreamWriter SW = new StreamWriter(File.Create("levels/" + p.name.ToLower() + "/bienvenu.txt"));
                SW.WriteLine("Bienvenue dans votre monde");
                SW.WriteLine("Ici vous pouvez faire tout ce que vous voulez");
                SW.WriteLine("Pour modifier ce message, utilisez la fonction /bienvenue");
                SW.Flush();
                SW.Close();
            }

            Level foundLevel = Level.Find("spawn", p.name.ToLower());

            if (foundLevel == null)
            { Command.all.Find("load").Use(p, "spawn " + p.name.ToLower()); }

            foundLevel = Level.Find("spawn", p.name.ToLower());

            if (foundLevel == null)
            { Player.SendMessage(p, "Une erreur est arrivee"); return; }

            Command.all.Find("goto").Use(p, "spawn " + p.name.ToLower());
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/home - Vous envoi au spawn de votre monde");
            Player.SendMessage(p, "Si vous n'avez pas encore de monde, il sera cree");
        }
    }
}