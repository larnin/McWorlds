using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdCopy : Command
    {
        public override string name { get { return "copy"; } }
        public override string shortcut { get { return "c"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public int allowoffset = 0;
        public CmdCopy() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }
            
            CatchPos cpos;
            cpos.ignoreTypes = new List<byte>();
            cpos.type = 0;
            p.copyoffset[0] = 0; p.copyoffset[1] = 0; p.copyoffset[2] = 0;
            allowoffset = (message.IndexOf('@'));
            if (allowoffset != -1) { message = message.Replace("@ ", ""); }
            if (message.ToLower() == "cut") { cpos.type = 1; message = ""; }
            else if (message.ToLower() == "air") { cpos.type = 2; message = ""; }
            else if (message == "@") { message = ""; }
            else if (message.IndexOf(' ') != -1)
            {
                if (message.Split(' ')[0] == "ignore")
                {
                    foreach (string s in message.Substring(message.IndexOf(' ') + 1).Split(' '))
                    {
                        if (Block.Byte(s) != Block.Zero)
                        {
                            cpos.ignoreTypes.Add(Block.Byte(s));
                            Player.SendMessage(p, "Ignore &b" + s);
                        }
                    }
                }
                else
                {
                    Help(p); return;
                }
                message = "";
            }

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;

            if (message != "") { Help(p); return; }

            Player.SendMessage(p, "Place 2 blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/copy - copie les blocs.");
            Player.SendMessage(p, "/copy cut - Copie puis suprime les blocs.");
            Player.SendMessage(p, "/copy air - Copie les blocs en incluant l'air.");
            Player.SendMessage(p, "/copy ignore <block1> <block2>.. - Ignore <bloc1> ... pendant la copie.");
            Player.SendMessage(p, "/copy @ - Permet de faire un 3e clic et de coller directement.");
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            p.copystart[0] = x;
            p.copystart[1] = y;
            p.copystart[2] = z;

            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;



            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange2);
        }

        public void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;

            p.CopyBuffer.Clear();
            int TotalAir = 0;
            if (cpos.type == 2) p.copyAir = true; else p.copyAir = false;

            for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                    for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    {
                        b = p.level.GetTile(xx, yy, zz);
                        if (Block.canPlace(p, b))
                        {
                            if (b == Block.air && cpos.type != 2 || cpos.ignoreTypes.Contains(b)) TotalAir++;

                            if (cpos.ignoreTypes.Contains(b)) BufferAdd(p, (ushort)(xx - cpos.x), (ushort)(yy - cpos.y), (ushort)(zz - cpos.z), Block.air);
                            else BufferAdd(p, (ushort)(xx - cpos.x), (ushort)(yy - cpos.y), (ushort)(zz - cpos.z), b);
                        }
                        else BufferAdd(p, (ushort)(xx - cpos.x), (ushort)(yy - cpos.y), (ushort)(zz - cpos.z), Block.air);
                    }

            if ((p.CopyBuffer.Count - TotalAir) > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayer de copier " + p.CopyBuffer.Count + " blocs.");
                Player.SendMessage(p, "Vous ne pouvez pas copier plus de " + p.maxblocsbuild() + ".");
                p.CopyBuffer.Clear();
                return;
            }

            if (cpos.type == 1)
                for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                    for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                        for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                        {
                            b = p.level.GetTile(xx, yy, zz);
                            if (b != Block.air && Block.canPlace(p, b))
                                p.level.Blockchange(p, xx, yy, zz, Block.air);
                        }

            Player.SendMessage(p, (p.CopyBuffer.Count - TotalAir) + " blocs copie.");
            if (allowoffset != -1)
            {
                Player.SendMessage(p, "Placez un bloc pour determiner ou coller");
                p.Blockchange += new Player.BlockchangeEventHandler(Blockchange3);
            }

        }

        public void Blockchange3(Player p, ushort x, ushort y, ushort z, byte type)
        {

            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;


            p.copyoffset[0] = (p.copystart[0] - x);
            p.copyoffset[1] = (p.copystart[1] - y);
            p.copyoffset[2] = (p.copystart[2] - z);

        }

        void BufferAdd(Player p, ushort x, ushort y, ushort z, byte type)
        {
            Player.CopyPos pos; pos.x = x; pos.y = y; pos.z = z; pos.type = type;
            p.CopyBuffer.Add(pos);
        }
        struct CatchPos { public ushort x, y, z; public int type; public List<byte> ignoreTypes; }
    }
}