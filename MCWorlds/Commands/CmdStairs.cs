using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdStairs : Command
    {
        public override string name { get { return "stairs"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdStairs() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;
            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
            Player.SendMessage(p, "Place 2 blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/stairs - cree un escalier en spirale.");
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

        public void Swap(ref int a, ref int b)
        {
            int c;
            c = a; a = b; b = c;
        }

        public void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;

            if (cpos.y == y)
            {
                Player.SendMessage(p, "Impossible de creer un escalier de 1 bloc de haut.");
                return;
            }

            ushort xx, zz; int currentState = 0;
            xx = cpos.x; zz = cpos.z;

            if (cpos.x > x && cpos.z > z) currentState = 0;
            else if (cpos.x > x && cpos.z < z) currentState = 1;
            else if (cpos.x < x && cpos.z > z) currentState = 2;
            else currentState = 3;

            for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
            {
                if (currentState == 0)
                {
                    xx++; p.level.Blockchange(p, xx, yy, zz, Block.staircasestep);
                    xx++; p.level.Blockchange(p, xx, yy, zz, Block.staircasefull);
                    currentState = 1;
                }
                else if (currentState == 1)
                {
                    zz++; p.level.Blockchange(p, xx, yy, zz, Block.staircasestep);
                    zz++; p.level.Blockchange(p, xx, yy, zz, Block.staircasefull);
                    currentState = 2;
                }
                else if (currentState == 2)
                {
                    xx--; p.level.Blockchange(p, xx, yy, zz, Block.staircasestep);
                    xx--; p.level.Blockchange(p, xx, yy, zz, Block.staircasefull);
                    currentState = 3;
                }
                else
                {
                    zz--; p.level.Blockchange(p, xx, yy, zz, Block.staircasestep);
                    zz--; p.level.Blockchange(p, xx, yy, zz, Block.staircasefull);
                    currentState = 0;
                }
                /*
                if (cpos.x == xx && cpos.z == zz || cpos.x == xx + 1 && cpos.z == zz) xx++;
                else if (cpos.x == xx + 2 && cpos.z == zz || cpos.x == xx + 2 && cpos.z == zz + 1) zz++;
                else if (cpos.x == xx + 2 && cpos.z == zz + 2 || cpos.x == xx + 1 && cpos.z == zz + 2) xx--;
                else zz--;
                */
            }

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        struct CatchPos { public ushort x, y, z; }

        public ushort z { get; set; }
    }
}