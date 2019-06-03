
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdMain : Command
    {
        public override string name { get { return "main"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdMain() { }

        public override void Use(Player p, string message)
        {
            Command.all.Find("goto").Use(p, Server.mainLevel.name + " " + Server.mainWorld);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/main - Permet d'aller a la map principale du serveur");
        }
    }
}
