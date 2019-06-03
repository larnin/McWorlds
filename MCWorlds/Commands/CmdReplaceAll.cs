using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdReplaceAll : Command
    {
        public override string name { get { return "replaceall"; } }
        public override string shortcut { get { return "ra"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdReplaceAll() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') == -1 || message.Split(' ').Length > 2) { Help(p); return; }

            byte b1, b2;

            b1 = Block.Byte(message.Split(' ')[0]);
            b2 = Block.Byte(message.Split(' ')[1]);

            if (b1 == Block.Zero || b2 == Block.Zero) { Player.SendMessage(p, "Impossible de trouver le bloc."); return; }
            ushort x, y, z; int currentBlock = 0;
            List<Pos> stored = new List<Pos>(); Pos pos;

            foreach (byte b in p.level.blocks)
            {
                if (b == b1)
                {
                    p.level.IntToPos(currentBlock, out x, out y, out z);
                    pos.x = x; pos.y = y; pos.z = z;
                    stored.Add(pos);
                }
                currentBlock++;
            }

            if (stored.Count > (p.maxblocsbuild() * 2)) { Player.SendMessage(p, "Impossible de remplacer plus de " + (p.maxblocsbuild() * 2) + " blocs."); return; }

            Player.SendMessage(p, stored.Count + " blocs sur " + currentBlock + " sont " + Block.Name(b1));

            foreach (Pos Pos in stored)
            {
                p.level.Blockchange(p, Pos.x, Pos.y, Pos.z, b2);
            }
            stored.Clear();

            Player.SendMessage(p, "&4/Remplacement finit!");
        }
        public struct Pos { public ushort x, y, z; }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/replaceall [bloc1] [bloc2] - Remplace tous les [bloc1] en [bloc2] de la map.");
        }
    }
}
