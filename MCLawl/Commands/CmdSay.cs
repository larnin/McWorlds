using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdSay : Command
    {
        public override string name { get { return "say"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSay() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player.GlobalChat(p, message, false);
            message = message.Replace("&", ""); // converts the MC color codes to IRC. Doesn't seem to work with multiple colors
            IRCBot.Say(message);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/say - Envoie un message global.");
        }
    }
}
