using System;
using System.IO;

namespace MCWorlds
{
    public class CmdTree : Command
    {
        public override string name { get { return "tree"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdTree() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.ClearBlockchange();
            switch (message.ToLower())
            {
                case "2":
                case "cactus": p.Blockchange += new Player.BlockchangeEventHandler(AddCactus); break;
                case "3":
                case "sapin": p.Blockchange += new Player.BlockchangeEventHandler(AddSapin); break;
                default: p.Blockchange += new Player.BlockchangeEventHandler(AddTree); break;
            }
            Player.SendMessage(p, "Placez un bloc ou vous voulez que l'arbre pousse");
            p.painting = false;
        }

        void AddTree(Player p, ushort x, ushort y, ushort z, byte type)
        {
            Random Rand = new Random();

            byte height = (byte)Rand.Next(5, 8);
            for (ushort yy = 0; yy < height; yy++) p.level.Blockchange(p, x, (ushort)(y + yy), z, Block.trunk);

            short top = (short)(height - Rand.Next(2, 4));

            for (short xx = (short)-top; xx <= top; ++xx)
            {
                for (short yy = (short)-top; yy <= top; ++yy)
                {
                    for (short zz = (short)-top; zz <= top; ++zz)
                    {
                        short Dist = (short)(Math.Sqrt(xx * xx + yy * yy + zz * zz));
                        if (Dist < top + 1)
                        {
                            if (Rand.Next((int)(Dist)) < 2)
                            {
                                try
                                {
                                    p.level.Blockchange(p, (ushort)(x + xx), (ushort)(y + yy + height), (ushort)(z + zz), Block.leaf);
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            if (!p.staticCommands) p.ClearBlockchange();
        }
        void AddCactus(Player p, ushort x, ushort y, ushort z, byte type)
        {
            Random Rand = new Random();

            byte height = (byte)Rand.Next(3, 6);
            ushort yy;

            for (yy = 0; yy <= height; yy++) p.level.Blockchange(p, x, (ushort)(y + yy), z, Block.green);

            int inX = 0, inZ = 0;

            switch (Rand.Next(1, 3))
            {
                case 1: inX = -1; break;
                case 2:
                default: inZ = -1; break;
            }

            for (yy = height; yy <= Rand.Next(height + 2, height + 5); yy++) p.level.Blockchange(p, (ushort)(x + inX), (ushort)(y + yy), (ushort)(z + inZ), Block.green);
            for (yy = height; yy <= Rand.Next(height + 2, height + 5); yy++) p.level.Blockchange(p, (ushort)(x - inX), (ushort)(y + yy), (ushort)(z - inZ), Block.green);

            if (!p.staticCommands) p.ClearBlockchange();
        }

        void AddSapin(Player p, ushort x, ushort y, ushort z, byte type)
        {
            
            Circle(p, x, (ushort)(y + 3), z, 3);
            Circle(p, x, (ushort)(y + 4), z, 2);
            Circle(p, x, (ushort)(y + 5), z, 1);
            Circle(p, x, (ushort)(y + 6), z, 2);
            Circle(p, x, (ushort)(y + 7), z, 1);

            for (int i = 8; i < 11; i++)
            { p.level.Blockchange(p, x, (ushort)(y + i), z, Block.leaf); }

            p.level.Blockchange(p, (ushort)(x + 1), (ushort)(y + 9), z, Block.leaf);
            p.level.Blockchange(p, (ushort)(x - 1), (ushort)(y + 9), z, Block.leaf);
            p.level.Blockchange(p, x, (ushort)(y + 9), (ushort)(z + 1), Block.leaf);
            p.level.Blockchange(p, x, (ushort)(y + 9), (ushort)(z - 1), Block.leaf);
            
            for (int i = 0; i < 8; i++)
            { p.level.Blockchange(p, x, (ushort)(y + i), z, Block.trunk); }
            
            if (!p.staticCommands) p.ClearBlockchange();
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/tree [type] - Pour faire pousser un arbre.");
            Player.SendMessage(p, "Types - (1 : normal), (2 : cactus), (3 : sapin)");
        }

        private void Circle(Player p, ushort x, ushort y, ushort z, int size)
        {
            for (int i = -size; i <= size; i++)
            {
                for (int j = -size; j <= size; j++)
                {
                    int distcarre = i * i + j * j;

                    if (distcarre < (size + .5) * (size + .5))
                    { p.level.Blockchange(p, (ushort)(x + i), y, (ushort)(z + j), Block.leaf); }
                }
            }
        }
    }
}