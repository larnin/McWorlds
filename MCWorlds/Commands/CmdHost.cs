﻿using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdHost : Command
    {
        public override string name { get { return "host"; } }
        public override string shortcut { get { return "zall"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdHost() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }

            Player.SendMessage(p, "L'hote est &3" + Server.ZallState + ".");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/host - Montre l'hote du serveur (console).");
        }
    }
}