using System;

namespace MCWorlds
{
    public class CmdXban : Command
    {
        public override string name { get { return "xban"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdXban() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            Player who = Player.Find(message.Split(' ')[0]);
            if (who != null)
            {
                Command.all.Find("ban").Use(p, message);
                Command.all.Find("undo").Use(p, message + " all");
                Command.all.Find("banip").Use(p, "@" + message);
                Command.all.Find("kick").Use(p, message);
                Command.all.Find("undo").Use(p, message + " all");
            }
            else
            {
                Command.all.Find("ban").Use(p, message);
                Command.all.Find("banip").Use(p, "@" + message);
                Command.all.Find("undo").Use(p, message + " all");
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/xban [nom] - undo, banip, ban et kick le joueur.");
        }
    }
}
