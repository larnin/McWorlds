using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdResetBot : Command
    {
        public override string name { get { return "resetbot"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdResetBot() { }

        public override void Use(Player p, string message)
        {
            IRCBot.Reset();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/resetbot - Redemarre le bot IRC. Seulement pour les nouveaux arrivants !");
        }
    }
}
