using System;
using System.IO;

namespace MCWorlds
{
    public class CmdJumpTo : Command
    {
        public override string name { get { return "jumpto"; } }
        public override string shortcut { get { return "jt"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdJumpTo() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'IRC"); }

            double x = (ushort)(p.pos[0] / 32);
            double y = (ushort)((p.pos[1] / 32) - 1);
            double z = (ushort)(p.pos[2] / 32);
            double a = Math.Sin(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
            double b = Math.Cos(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
            double c = Math.Cos(((double)(p.rot[1] + 64) / 256) * 2 * Math.PI);

            ushort xx, yy, zz;

            bool blocAttein = false;

            while (!blocAttein)
            {
                x += a; y += c; z += b;

                if (x < 0 || x >= p.level.width || y < 0 || y >= p.level.depth || z < 0 || z >= p.level.height)
                { blocAttein = true; }
                else if (p.level.GetTile((ushort)x, (ushort)y, (ushort)z) != Block.air)
                { blocAttein = true; }
            }

            xx = (ushort)((x - a) * 32 + 16); yy = (ushort)((y - c) * 32 + 32); zz = (ushort)((z - b) * 32 + 16);

            if (!p.Loading) 
            { 
                unchecked { p.SendPos((byte)-1, xx, yy, zz, p.rot[0], 0); }
                Player.SendMessage(p, "Envoye en " + (int)x + " " + (int)y + " " + (int)z);
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/jumpto - Permet de se téléporter vers le point visé.");
            Player.SendMessage(p, "&cAttention le saut est assez aproximatif !");
        }
    }
}