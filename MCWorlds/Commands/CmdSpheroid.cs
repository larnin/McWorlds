using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdSpheroid : Command
    {
        public override string name { get { return "spheroid"; } }
        public override string shortcut { get { return "e"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSpheroid() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;

            cpos.x = 0; cpos.y = 0; cpos.z = 0;

            if (message == "")
            {
                cpos.type = Block.Zero;
                cpos.vertical = false;
            }
            else if (message.IndexOf(' ') == -1)
            {
                cpos.type = Block.Byte(message);
                cpos.vertical = false;
                if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                if (cpos.type == Block.Zero)
                {
                    if (message.ToLower() == "vertical")
                    {
                        cpos.vertical = true;
                    }
                    else
                    {
                        Help(p); return;
                    }
                }
            }
            else
            {
                cpos.type = Block.Byte(message.Split(' ')[0]);
                if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                if (cpos.type == Block.Zero || message.Split(' ')[1].ToLower() != "vertical")
                {
                    Help(p); return;
                }
                cpos.vertical = true;
            }

            if (!Block.canPlace(p, cpos.type) && cpos.type != Block.Zero) { Player.SendMessage(p, "Impossible de placer ce type de bloc !"); return; }

            p.blockchangeObject = cpos;

            Player.SendMessage(p, "Place 2 blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/spheroid <type> - Cree une sphere de bloc.");
            Player.SendMessage(p, "/spheroid <type> vertical - Cree un cylindre creu vertical");
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
            if (cpos.type != Block.Zero) { type = cpos.type; }
            List<Pos> buffer = new List<Pos>();

            if (!cpos.vertical)
            {
                // find start/end coordinates
                int sx = Math.Min(cpos.x, x);
                int ex = Math.Max(cpos.x, x);
                int sy = Math.Min(cpos.y, y);
                int ey = Math.Max(cpos.y, y);
                int sz = Math.Min(cpos.z, z);
                int ez = Math.Max(cpos.z, z);

                // find axis lengths
                double rx = (ex - sx + 1) / 2 + .25;
                double ry = (ey - sy + 1) / 2 + .25;
                double rz = (ez - sz + 1) / 2 + .25;

                double rx2 = 1 / (rx * rx);
                double ry2 = 1 / (ry * ry);
                double rz2 = 1 / (rz * rz);

                // find center points
                double cx = (ex + sx) / 2;
                double cy = (ey + sy) / 2;
                double cz = (ez + sz) / 2;
                int totalBlocks = (int)(Math.PI * 0.75 * rx * ry * rz);

                if (totalBlocks > p.maxblocsbuild())
                {
                    Player.SendMessage(p, "Vous essayez de faire une sphere de " + totalBlocks + " blocs.");
                    Player.SendMessage(p, "Vous ne pouvez pas faire une sphere de plus de " + p.maxblocsbuild() + " blocs.");
                    return;
                }

                Player.SendMessage(p, totalBlocks + " blocs.");

                for (int xx = sx; xx <= ex; xx += 8)
                    for (int yy = sy; yy <= ey; yy += 8)
                        for (int zz = sz; zz <= ez; zz += 8)
                            for (int z3 = 0; z3 < 8 && zz + z3 <= ez; z3++)
                                for (int y3 = 0; y3 < 8 && yy + y3 <= ey; y3++)
                                    for (int x3 = 0; x3 < 8 && xx + x3 <= ex; x3++)
                                    {
                                        // get relative coordinates
                                        double dx = (xx + x3 - cx);
                                        double dy = (yy + y3 - cy);
                                        double dz = (zz + z3 - cz);

                                        // test if it's inside ellipse
                                        if ((dx * dx) * rx2 + (dy * dy) * ry2 + (dz * dz) * rz2 <= 1)
                                        {
                                            p.level.Blockchange(p, (ushort)(x3 + xx), (ushort)(yy + y3), (ushort)(zz + z3), type);
                                        }
                                    }
            }
            else
            {
                int radius = Math.Abs(cpos.x - x) / 2;
                int f = 1 - radius;
                int ddF_x = 1;
                int ddF_y = -2 * radius;
                int xx = 0;
                int zz = radius;

                int x0 = Math.Min(cpos.x, x) + radius;
                int z0 = Math.Min(cpos.z, z) + radius;

                Pos pos = new Pos();
                pos.x = (ushort)x0; pos.z = (ushort)(z0 + radius); buffer.Add(pos);
                pos.z = (ushort)(z0 - radius); buffer.Add(pos);
                pos.x = (ushort)(x0 + radius); pos.z = (ushort)z0; buffer.Add(pos);
                pos.x = (ushort)(x0 - radius); buffer.Add(pos);

                while (xx < zz)
                {
                    if (f >= 0)
                    {
                        zz--;
                        ddF_y += 2;
                        f += ddF_y;
                    }
                    xx++;
                    ddF_x += 2;
                    f += ddF_x;

                    pos.z = (ushort)(z0 + zz);
                    pos.x = (ushort)(x0 + xx); buffer.Add(pos);
                    pos.x = (ushort)(x0 - xx); buffer.Add(pos);
                    pos.z = (ushort)(z0 - zz);
                    pos.x = (ushort)(x0 + xx); buffer.Add(pos);
                    pos.x = (ushort)(x0 - xx); buffer.Add(pos);
                    pos.z = (ushort)(z0 + xx);
                    pos.x = (ushort)(x0 + zz); buffer.Add(pos);
                    pos.x = (ushort)(x0 - zz); buffer.Add(pos);
                    pos.z = (ushort)(z0 - xx);
                    pos.x = (ushort)(x0 + zz); buffer.Add(pos);
                    pos.x = (ushort)(x0 - zz); buffer.Add(pos);
                }

                int ydiff = Math.Abs(y - cpos.y) + 1;

                if (buffer.Count * ydiff > p.maxblocsbuild())
                {
                    Player.SendMessage(p, "Vous essayez de faire une sphere de " + buffer.Count * ydiff + " blocs.");
                    Player.SendMessage(p, "Vous ne pouvez pas faire une sphere de plus de " + p.maxblocsbuild() + " blocs.");
                    return;
                }
                Player.SendMessage(p, buffer.Count * ydiff + " blocs.");

                foreach (Pos Pos in buffer)
                {
                    for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); yy++)
                    {
                        p.level.Blockchange(p, Pos.x, yy, Pos.z, type);
                    }
                }
            }
            buffer.Clear();

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        void BufferAdd(List<Pos> list, ushort x, ushort y, ushort z)
        {
            Pos pos; pos.x = x; pos.y = y; pos.z = z; list.Add(pos);
        }
        struct Pos { public ushort x, y, z; } 
        struct CatchPos
        {
            public byte type;
            public ushort x, y, z;
            public bool vertical;
        }
    }
}