using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdCuboid : Command
    {
        public override string name { get { return "cuboid"; } }
        public override string shortcut { get { return "z"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdCuboid() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            int number = message.Split(' ').Length;
            if (number > 2) { Help(p); return; }
            if (number == 2)
            {
                int pos = message.IndexOf(' ');
                string t = message.Substring(0, pos).ToLower();
                string s = message.Substring(pos + 1).ToLower();
                byte type = Block.Byte(t);
                if (type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + t + "\"."); return; }

                if (!Block.canPlace(p, type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }

                SolidType solid;
                if (s == "solid") { solid = SolidType.solid; }
                else if (s == "hollow") { solid = SolidType.hollow; }
                else if (s == "walls") { solid = SolidType.walls; }
                else if (s == "holes") { solid = SolidType.holes; }
                else if (s == "wire") { solid = SolidType.wire; }
                else if (s == "random") { solid = SolidType.random; }
                else { Help(p); return; }
                CatchPos cpos; cpos.solid = solid; cpos.type = type;
                cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            }
            else if (message != "")
            {
                SolidType solid = SolidType.solid;
                message = message.ToLower();
                byte type; unchecked { type = (byte)-1; }
                if (message == "solid") { solid = SolidType.solid; }
                else if (message == "hollow") { solid = SolidType.hollow; }
                else if (message == "walls") { solid = SolidType.walls; }
                else if (message == "holes") { solid = SolidType.holes; }
                else if (message == "wire") { solid = SolidType.wire; }
                else if (message == "random") { solid = SolidType.random; }
                else
                {
                    byte t = Block.Byte(message);
                    if (t == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + message + "\"."); return; }

                    if (!Block.canPlace(p, t)) { Player.SendMessage(p, "Impossible de placer ca."); return; }

                    type = t;
                } CatchPos cpos; cpos.solid = solid; cpos.type = type;
                cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            }
            else
            {
                CatchPos cpos; cpos.solid = SolidType.solid; unchecked { cpos.type = (byte)-1; }
                cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            }
            Player.SendMessage(p, "Place 2 blocs pour determiner la taille.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            if (message == "param" || message == "parametre")
            {
                Player.SendMessage(p, "solid : le cuboid plein classic (par defaut)");
                Player.SendMessage(p, "hollow : Juste les 6 faces du cube seront dessine, le cube sera creu");
                Player.SendMessage(p, "walls : Dessine seulement les 4 faces verticales");
                Player.SendMessage(p, "holes : Seulement un cube sur 2 sera place (mosaique)");
                Player.SendMessage(p, "wire : Dessine seulement les arettes du cube");
                Player.SendMessage(p, "random : place des blocs aleatoirement dans le cuboid");
                return;
            }
            Player.SendMessage(p, "/cuboid [type] <solid/hollow/walls/holes/wire/random> - Cree un cuboid de bloc.");
            Player.SendMessage(p, "Pour plus d'infos sur les parametres du cuboid '/help cuboid param'");
        }
        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange2);
        }
        public void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;
            unchecked { if (cpos.type != (byte)-1) type = cpos.type; else type = p.bindings[type]; }
            List<Pos> buffer = new List<Pos>();

            ushort xx; ushort yy; ushort zz;

            switch (cpos.solid)
            {
                case SolidType.solid:
                    buffer.Capacity = Math.Abs(cpos.x - x) * Math.Abs(cpos.y - y) * Math.Abs(cpos.z - z);
                    for (xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                        for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                            for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                            {
                                if (p.level.GetTile(xx, yy, zz) != type) { BufferAdd(buffer, xx, yy, zz); }
                            }
                    break;
                case SolidType.hollow:
                    //todo work out if theres 800 blocks used before making the buffer
                    for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                        for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                        {
                            if (p.level.GetTile(cpos.x, yy, zz) != type) { BufferAdd(buffer, cpos.x, yy, zz); }
                            if (cpos.x != x) { if (p.level.GetTile(x, yy, zz) != type) { BufferAdd(buffer, x, yy, zz); } }
                        }
                    if (Math.Abs(cpos.x - x) >= 2)
                    {
                        for (xx = (ushort)(Math.Min(cpos.x, x) + 1); xx <= Math.Max(cpos.x, x) - 1; ++xx)
                            for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                            {
                                if (p.level.GetTile(xx, cpos.y, zz) != type) { BufferAdd(buffer, xx, cpos.y, zz); }
                                if (cpos.y != y) { if (p.level.GetTile(xx, y, zz) != type) { BufferAdd(buffer, xx, y, zz); } }
                            }
                        if (Math.Abs(cpos.y - y) >= 2)
                        {
                            for (xx = (ushort)(Math.Min(cpos.x, x) + 1); xx <= Math.Max(cpos.x, x) - 1; ++xx)
                                for (yy = (ushort)(Math.Min(cpos.y, y) + 1); yy <= Math.Max(cpos.y, y) - 1; ++yy)
                                {
                                    if (p.level.GetTile(xx, yy, cpos.z) != type) { BufferAdd(buffer, xx, yy, cpos.z); }
                                    if (cpos.z != z) { if (p.level.GetTile(xx, yy, z) != type) { BufferAdd(buffer, xx, yy, z); } }
                                }
                        }
                    }
                    break;
                case SolidType.walls:
                    for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                        for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                        {
                            if (p.level.GetTile(cpos.x, yy, zz) != type) { BufferAdd(buffer, cpos.x, yy, zz); }
                            if (cpos.x != x) { if (p.level.GetTile(x, yy, zz) != type) { BufferAdd(buffer, x, yy, zz); } }
                        }
                    if (Math.Abs(cpos.x - x) >= 2)
                    {
                        if (Math.Abs(cpos.z - z) >= 2)
                        {
                            for (xx = (ushort)(Math.Min(cpos.x, x) + 1); xx <= Math.Max(cpos.x, x) - 1; ++xx)
                                for (yy = (ushort)(Math.Min(cpos.y, y)); yy <= Math.Max(cpos.y, y); ++yy)
                                {
                                    if (p.level.GetTile(xx, yy, cpos.z) != type) { BufferAdd(buffer, xx, yy, cpos.z); }
                                    if (cpos.z != z) { if (p.level.GetTile(xx, yy, z) != type) { BufferAdd(buffer, xx, yy, z); } }
                                }
                        }
                    }
                    break;
                case SolidType.holes:
                    bool Checked = true, startZ, startY;

                    for (xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                    {
                        startY = Checked;
                        for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                        {
                            startZ = Checked;
                            for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                            {
                                Checked = !Checked;
                                if (Checked && p.level.GetTile(xx, yy, zz) != type) BufferAdd(buffer, xx, yy, zz);
                            } Checked = !startZ;
                        } Checked = !startY;
                    }
                    break;
                case SolidType.wire:
                    for (xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                    {
                        BufferAdd(buffer, xx, y, z);
                        BufferAdd(buffer, xx, y, cpos.z);
                        BufferAdd(buffer, xx, cpos.y, z);
                        BufferAdd(buffer, xx, cpos.y, cpos.z);
                    }
                    for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                    {
                        BufferAdd(buffer, x, yy, z);
                        BufferAdd(buffer, x, yy, cpos.z);
                        BufferAdd(buffer, cpos.x, yy, z);
                        BufferAdd(buffer, cpos.x, yy, cpos.z);
                    }
                    for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    {
                        BufferAdd(buffer, x, y, zz);
                        BufferAdd(buffer, x, cpos.y, zz);
                        BufferAdd(buffer, cpos.x, y, zz);
                        BufferAdd(buffer, cpos.x, cpos.y, zz);
                    }
                    break;
                case SolidType.random:
                    Random rand = new Random();
                    for (xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                        for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                            for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                            {
                                if (rand.Next(1, 11) <= 5 && p.level.GetTile(xx, yy, zz) != type) { BufferAdd(buffer, xx, yy, zz); }
                            }
                    break;
            }

            if (Server.forceCuboid)
            {
                int counter = 1;
                buffer.ForEach(delegate(Pos pos)
                {
                    if (counter <= p.maxblocsbuild())
                    {
                        counter++;
                        p.level.Blockchange(p, pos.x, pos.y, pos.z, type);
                    }
                });
                if (counter >= p.maxblocsbuild())
                {
                    Player.SendMessage(p, "Vous essayez de faire un cuboid de " + buffer.Count + " blocs, votre limite est de " + p.maxblocsbuild() + ".");
                    Player.SendMessage(p, "Execution d'un cuboid superieur a la limite.");
                }
                else
                {
                    Player.SendMessage(p, buffer.Count.ToString() + " blocks.");
                }
                if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
                return;
            }

            if (buffer.Count > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayer de faire un cuboid de " + buffer.Count + " blocs.");
                Player.SendMessage(p, "Vous n'avez pas le droit a plus de " + p.maxblocsbuild() + ".");
                return;
            }

            Player.SendMessage(p, buffer.Count.ToString() + " blocs.");

            buffer.ForEach(delegate(Pos pos)
            {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, type);
            });

            buffer.Clear();

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        void BufferAdd(List<Pos> list, ushort x, ushort y, ushort z)
        {
            Pos pos; pos.x = x; pos.y = y; pos.z = z; list.Add(pos);
        }
        struct Pos
        {
            public ushort x, y, z;
        }
        struct CatchPos
        {
            public SolidType solid;
            public byte type;
            public ushort x, y, z;
        }
        enum SolidType { solid, hollow, walls, holes, wire, random };
    }
}