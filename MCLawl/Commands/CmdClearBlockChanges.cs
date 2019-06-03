using System;
using System.Data;
//using MySql.Data.MySqlClient;
//using MySql.Data.Types;

namespace MCWorlds
{
    public class CmdClearBlockChanges : Command
    {
        public override string name { get { return "clearblockchanges"; } }
        public override string shortcut { get { return "cbc"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdClearBlockChanges() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.Split(' ').Length != 2 || message != "") { Help(p); return; }
            string lvl = "";
            string world = "";
            if (message == "")
            { lvl = p.level.name; world = p.level.world; }
            else if (message.IndexOf(' ') == -1)
            { lvl = message; world = p.level.world; }
            else { lvl = message.Split(' ')[0]; world = message.Split(' ')[1]; }
            
            Level l = Level.Find(lvl,world);
            if (l == null) { Player.SendMessage(p, "Ne trouve pas la map."); return; }

            MySQL.executeQuery("TRUNCATE TABLE `Block" + l.name + "." + l.world + "`");
            Player.SendMessage(p, "Supression de &cTOUTES" + Server.DefaultColor + " les informations de changement de bloc sur : &d" + l.name);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/clearblockchanges [map] [monde] - Efface les informations sur le changements de bloc stocke dans /about pour la map.");
            Player.SendMessage(p, "&cA utiliser avec precaution, annulation impossible !");
        }
    }
}