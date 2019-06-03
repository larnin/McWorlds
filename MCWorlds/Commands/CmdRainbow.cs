using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdRainbow : Command
    {
        public override string name { get { return "rainbow"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRainbow() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;
            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            Player.SendMessage(p, "Place deux blocs por determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/rainbow - Cree un 'arc en ciel'");
            Player.SendMessage(p, "L'arc en ciel se fait sur des blocs deja existant");
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
            List<Pos> buffer = new List<Pos>();

            byte newType = Block.darkpink;

            int xdif = Math.Abs(cpos.x - x);
            int ydif = Math.Abs(cpos.y - y);
            int zdif = Math.Abs(cpos.z - z);

            if (xdif >= ydif && xdif >= zdif)
            {
                for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); xx++)
                {
                    newType += 1;
                    if (newType > Block.darkpink) newType = Block.red;
                    for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); yy++)
                    {
                        for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); zz++)
                        {
                            if (p.level.GetTile(xx, yy, zz) != Block.air)
                                BufferAdd(buffer, xx, yy, zz, newType);
                        }
                    }
                }
            }
            else if (ydif > xdif && ydif > zdif)
            {
                for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); yy++)
                {
                    newType += 1;
                    if (newType > Block.darkpink) newType = Block.red;
                    for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); xx++)
                    {
                        for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); zz++)
                        {
                            if (p.level.GetTile(xx, yy, zz) != Block.air)
                                BufferAdd(buffer, xx, yy, zz, newType);
                        }
                    }
                }
            }
            else if (zdif > ydif && zdif > xdif)
            {
                for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); zz++)
                {
                    newType += 1;
                    if (newType > Block.darkpink) newType = Block.red;
                    for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); yy++)
                    {
                        for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); xx++)
                        {
                            if (p.level.GetTile(xx, yy, zz) != Block.air)
                                BufferAdd(buffer, xx, yy, zz, newType);
                        }
                    }
                }
            }

            if (buffer.Count > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayez de remplacer " + buffer.Count + " blocs.");
                Player.SendMessage(p, "Vous ne pouvez pas remplacer plus de " + p.maxblocsbuild() + " blocs.");
                return;
            }

            Player.SendMessage(p, buffer.Count.ToString() + " blocs.");
            buffer.ForEach(delegate(Pos pos)
            {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.newType);                  //update block for everyone
            });

            buffer.Clear();

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        void BufferAdd(List<Pos> list, ushort x, ushort y, ushort z, byte newType)
        {
            Pos pos;
            pos.x = x; pos.y = y; pos.z = z; pos.newType = newType;
            list.Add(pos);
        }

        struct Pos { public ushort x, y, z; public byte newType; }
        struct CatchPos { public ushort x, y, z; }
    }
}
