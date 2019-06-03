using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdPaste : Command
    {
        public override string name { get { return "paste"; } }
        public override string shortcut { get { return "v"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public string loadname;
        public CmdPaste() { }
        
        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }

            CatchPos cpos;
            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;

            Player.SendMessage(p, "Placez un bloc pour determiner l'endroit ou vous voulez coller."); p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/paste - Colle ce que vous avez copier.");
            Player.SendMessage(p, "&4ATTENTION: " + Server.DefaultColor + "Les blocs sont colle dans la meme direction que l'objet copie");
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);

            Player.UndoPos Pos1;
            //p.UndoBuffer.Clear();
            p.CopyBuffer.ForEach(delegate(Player.CopyPos pos)
            {
                Pos1.x = (ushort)(Math.Abs(pos.x) + x);
                Pos1.y = (ushort)(Math.Abs(pos.y) + y);
                Pos1.z = (ushort)(Math.Abs(pos.z) + z);

                if (pos.type != Block.air || p.copyAir)
                    unchecked { if (p.level.GetTile(Pos1.x, Pos1.y, Pos1.z) != Block.Zero) p.level.Blockchange(p, (ushort)(Pos1.x + p.copyoffset[0]), (ushort)(Pos1.y + p.copyoffset[1]), (ushort)(Pos1.z + p.copyoffset[2]), pos.type); }
            });

            Player.SendMessage(p, p.CopyBuffer.Count + " blocs colles.");

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        struct CatchPos { public ushort x, y, z; }
    }
}