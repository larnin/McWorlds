using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdMeasure : Command
    {
        public override string name { get { return "measure"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMeasure() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') != -1) { Help(p); return; }

            CatchPos cpos;
            cpos.toIgnore = Block.Byte(message);
            if (cpos.toIgnore == Block.Zero && message != "") { Player.SendMessage(p, "Impossible de trouver le bloc"); return; }

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;

            Player.SendMessage(p, "Place 2 blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/measure [ignore] - Compte le nombre de blocs entre les 2 points");
            Player.SendMessage(p, " [ignore] - Entrez un bloc a ignorer");
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

            ushort xx, yy, zz; int foundBlocks = 0;

            for (xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
                for (yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                    for (zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    {
                        if (p.level.GetTile(xx, yy, zz) != cpos.toIgnore) foundBlocks++;
                    }

            Player.SendMessage(p, "Il y a " + foundBlocks + " blocs entre (" + cpos.x + ", " + cpos.y + ", " + cpos.z + ") et (" + x + ", " + y + ", " + z + ")");
            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        struct CatchPos
        {
            public ushort x, y, z; public byte toIgnore;
        }
    }
}