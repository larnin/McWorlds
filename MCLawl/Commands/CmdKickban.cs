using System;

namespace MCWorlds
{
    public class CmdKickban : Command
    {
        public override string name { get { return "kickban"; } }
        public override string shortcut { get { return "kb"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdKickban() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            Command.all.Find("ban").Use(p, message.Split(' ')[0]);
            Command.all.Find("kick").Use(p, message);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/kickban [joueur] <message> - Kicks et banni le joueur.");
        }
    }
}