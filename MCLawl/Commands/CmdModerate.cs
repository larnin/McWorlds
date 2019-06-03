using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdModerate : Command
    {
        public override string name { get { return "moderate"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdModerate() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }

            if (Server.chatmod)
            {
                Server.chatmod = false;
                Player.GlobalChat(null, Server.DefaultColor + "La moderation du tchat est desactive. Vous pouvez parler.", false);
            }
            else
            {
                Server.chatmod = true;
                Player.GlobalChat(null, Server.DefaultColor + "La moderation du tchat est active ! Silence de mort ...", false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/moderate - Active/desactive la moderation du tchat");
            Player.SendMessage(p, "Seulement ceux qui ont la voix peuvent parler, voir /voice.");
        }
    }
}