using System;
using System.IO;

namespace MCWorlds
{
    public class CmdTnt : Command
    {
        public override string name { get { return "tnt"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdTnt() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!p.level.pergun)
            { Player.SendMessage(p, "Vous ne pouvez pas utiliser la tnt dans cette map"); return; }

            if (message.Split(' ').Length > 1) { Help(p); return; }

            if (p.BlockAction == 13 || p.BlockAction == 14)
            {
                p.BlockAction = 0; Player.SendMessage(p, "Le mode TNT est &cdesactive" + Server.DefaultColor + ".");
            }
            else if (message.ToLower() == "small" || message == "")
            {
                p.BlockAction = 13; Player.SendMessage(p, "Le mode TNT est &aactive" + Server.DefaultColor + ".");
            }
            else if (message.ToLower() == "big")
            {
                if (p.group.Permission > LevelPermission.Builder)
                {
                    p.BlockAction = 14; Player.SendMessage(p, "Le mode TNT est &aactive" + Server.DefaultColor + ".");
                }
                else
                {
                    Player.SendMessage(p, "Ce mode est reserve aux OP+.");
                }
            }
            else
            {
                Help(p);
            }

            p.painting = false;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/tnt <big> - Cree des explosion de tnt (physics 3 ou 4).");
            Player.SendMessage(p, "Big TNT est reserve aux OP+.");
        }
    }
}