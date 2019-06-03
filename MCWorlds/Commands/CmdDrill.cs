using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdDrill : Command
    {
        public override string name { get { return "drill"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdDrill() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;
            cpos.distance = 20;

            if (message != "")
                try
                {
                    cpos.distance = int.Parse(message);
                }
                catch { Help(p); return; }

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;

            Player.SendMessage(p, "Cassez un bloc ou vous voulez creuser une galerie."); p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/drill - Creuse une galerie en 3*3 sur 20 blocs de long");
            Player.SendMessage(p, "/drill [distance] - creuse une galerie, casse tous les blocs semblables dans un carre de 3*3 devant vous.");
        }
        
        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            if (!p.staticCommands) p.ClearBlockchange();
            CatchPos cpos = (CatchPos)p.blockchangeObject;
            byte oldType = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, oldType);

            int diffX = 0, diffZ = 0;

            if (p.rot[0] <= 32 || p.rot[0] >= 224) { diffZ = -1; }
            else if (p.rot[0] <= 96) { diffX = 1; }
            else if (p.rot[0] <= 160) { diffZ = 1; }
            else diffX = -1;

            List<Pos> buffer = new List<Pos>();
            Pos pos;
            int total = 0;

            if (diffX != 0)
            {
                for (ushort xx = x; total < cpos.distance; xx += (ushort)diffX)
                {
                    for (ushort yy = (ushort)(y - 1); yy <= (ushort)(y + 1); yy++)
                        for (ushort zz = (ushort)(z - 1); zz <= (ushort)(z + 1); zz++)
                        {
                            pos.x = xx; pos.y = yy; pos.z = zz;
                            buffer.Add(pos);
                        }
                    total++;
                }
            }
            else
            {
                for (ushort zz = z; total < cpos.distance; zz += (ushort)diffZ)
                {
                    for (ushort yy = (ushort)(y - 1); yy <= (ushort)(y + 1); yy++)
                        for (ushort xx = (ushort)(x - 1); xx <= (ushort)(x + 1); xx++)
                        {
                            pos.x = xx; pos.y = yy; pos.z = zz;
                            buffer.Add(pos);
                        }
                    total++;
                }
            }

            if (buffer.Count > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Yous essayez de creuser une galerie de " + buffer.Count + " blocs.");
                Player.SendMessage(p, "Vous ne puvez pas creuser plus de " + p.maxblocsbuild() + " blocs.");
                return;
            }

            foreach (Pos pos1 in buffer)
            {
                if (p.level.GetTile(pos1.x, pos1.y, pos1.z) == oldType)
                    p.level.Blockchange(p, pos1.x, pos1.y, pos1.z, Block.air);
            }
            Player.SendMessage(p, buffer.Count + " blocs.");

            buffer.Clear();
        }

        struct CatchPos { public ushort x, y, z; public int distance; }
        struct Pos { public ushort x, y, z; }
    }
}