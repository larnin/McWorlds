using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdReplaceNot : Command
    {
        public override string name { get { return "replacenot"; } }
        public override string shortcut { get { return "rn"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdReplaceNot() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            int number = message.Split(' ').Length;
            CatchPos cpos = new CatchPos(); byte btype;
            if (number < 2) { Help(p); return; }

            btype = Block.Byte(message.Split(' ')[0]);
            if (btype == 255) { Player.SendMessage(p, "Le bloc " +  message.Split(' ')[0] + " n'existe pas."); return; }

            cpos.type = btype;

            if (Block.Byte(message.Split(' ')[1]) == 255) { Player.SendMessage(p,"le bloc " +  message.Split(' ')[1] + " n'existe pas."); return; }

            cpos.type2 = Block.Byte(message.Split(' ')[1]);
            if (!Block.canPlace(p, cpos.type2)) { Player.SendMessage(p, "Impossible de placer ce bloc."); return; }

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            Player.SendMessage(p, "Place deux blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/rn [type] [type2] - Remplace tous les blocs sauf [type] en [type2]");
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

            for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
            {
                for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                {
                    for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    {
                        if (p.level.GetTile(xx, yy, zz) != cpos.type) { BufferAdd(buffer, xx, yy, zz); }
                    }
                }
            }

            if (buffer.Count > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Yous essayez de remplacer " + buffer.Count + " blocs.");
                Player.SendMessage(p, "Vous ne pouvez pas remplacer plus de " + p.maxblocsbuild() + " blocs.");
                return;
            }

            Player.SendMessage(p, buffer.Count.ToString() + " blocs.");

            buffer.ForEach(delegate(Pos pos)
            {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, cpos.type2);                  //update block for everyone
            });

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
            public byte type2;
            public ushort x, y, z;
        }

    }
}
