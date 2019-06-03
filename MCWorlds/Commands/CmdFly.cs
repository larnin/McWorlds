using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    public class CmdFly : Command
    {
        public override string name { get { return "fly"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdFly() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.isFlying = !p.isFlying;
            if (!p.isFlying) return;

            Player.SendMessage(p, "Vous pouvez maintenant voler. &cSautez!");

            Thread flyThread = new Thread(new ThreadStart(delegate
            {
                Pos pos;
                List<Pos> buffer = new List<Pos>();
                while (p.isFlying)
                {
                    Thread.Sleep(20);
                    try
                    {
                        List<Pos> tempBuffer = new List<Pos>();

                        ushort x = (ushort)((p.pos[0]) / 32);
                        ushort y = (ushort)((p.pos[1] - 60) / 32);
                        ushort z = (ushort)((p.pos[2]) / 32);

                        try
                        {
                            for (ushort xx = (ushort)(x - 2); xx <= x + 2; xx++)
                            {
                                for (ushort yy = (ushort)(y - 1); yy <= y; yy++)
                                {
                                    for (ushort zz = (ushort)(z - 2); zz <= z + 2; zz++)
                                    {
                                        if (p.level.GetTile(xx, yy, zz) == Block.air)
                                        {
                                            pos.x = xx; pos.y = yy; pos.z = zz;
                                            tempBuffer.Add(pos);
                                        }
                                    }
                                }
                            }

                            List<Pos> toRemove = new List<Pos>();
                            foreach (Pos cP in buffer)
                            {
                                if (!tempBuffer.Contains(cP))
                                {
                                    p.SendBlockchange(cP.x, cP.y, cP.z, Block.air);
                                    toRemove.Add(cP);
                                }
                            }

                            foreach (Pos cP in toRemove)
                            {
                                buffer.Remove(cP);
                            }

                            foreach (Pos cP in tempBuffer)
                            {
                                if (!buffer.Contains(cP))
                                {
                                    buffer.Add(cP);
                                    p.SendBlockchange(cP.x, cP.y, cP.z, Block.glass);
                                }
                            }

                            tempBuffer.Clear();
                            toRemove.Clear();
                        }
                        catch { }
                    }
                    catch { }
                }

                foreach (Pos cP in buffer)
                {
                    p.SendBlockchange(cP.x, cP.y, cP.z, Block.air);
                }

                Player.SendMessage(p, "Vous ne savez plus voler !");
            }));
            flyThread.Start();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/fly - Permet de voler");
        }

        struct Pos { public ushort x, y, z; }
    }
}