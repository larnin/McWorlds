using System;

namespace MCWorlds
{
    public class CmdFlipHeads : Command
    {
        public override string name { get { return "flipheads"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdFlipHeads() { }

        public override void Use(Player p, string message)
        {
            Server.flipHead = !Server.flipHead;

            if (Server.flipHead)
                Player.GlobalChat(p, "Toutes les tetes sont cassee", false);
            else
                Player.GlobalChat(p, "Toutes les tetes sont reparee", false);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/flipheads - Savez vous ce que ca fait ?");
        }
    }
}