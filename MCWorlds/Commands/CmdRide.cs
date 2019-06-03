using System;
using System.IO;
using System.Threading;

namespace MCWorlds
{
    public class CmdRide : Command
    {
        public override string name { get { return "ride"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRide() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.onTrain = !p.onTrain;

            if (!p.onTrain) return;

            Thread trainThread = new Thread(new ThreadStart(delegate
            {
                while (p.onTrain)
                {
                    Thread.Sleep(3);

                    ushort x = (ushort)(p.pos[0] / 32);
                    ushort y = (ushort)(p.pos[1] / 32);
                    ushort z = (ushort)(p.pos[2] / 32);

                    for (ushort xx = (ushort)(x - 1); xx <= x + 1; xx++)
                    {
                        for (ushort yy = (ushort)(y - 1); yy <= y + 1; yy++)
                        {
                            for (ushort zz = (ushort)(z - 1); zz <= z + 1; zz++)
                            {
                                if (p.level.GetTile(xx, yy, zz) == Block.train)
                                {
                                    p.invincible = true; p.trainGrab = true;
                                    byte newY = 0;

                                    if (y - yy == -1) newY = 240;
                                    else if (y - yy == 0) newY = 0;
                                    else newY = 8;

                                    unchecked
                                    {
                                        if (x - xx == -1)
                                            if (z - zz == -1) p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)96, newY);
                                            else if (z - zz == 0) p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)64, newY);
                                            else p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)32, newY);
                                        else if (x - xx == 0)
                                            if (z - zz == -1) p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)128, newY);
                                            else if (z - zz == 0) { }
                                            else p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)0, newY);
                                        else
                                            if (z - zz == -1) p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)160, newY);
                                            else if (z - zz == 0) p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)192, newY);
                                            else p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)224, newY);
                                    }
                                    goto skip;
                                }
                            }
                        }
                    }

                    if (p.disconnected) { p.onTrain = false; }

                    Thread.Sleep(3);
                    p.invincible = false;
                    p.trainGrab = false;
            skip:   ;                 
                }

                Player.SendMessage(p, "Desactive");
                Thread.Sleep(1000);
                p.invincible = false;
                p.trainGrab = false;
            }));
            trainThread.Start();
            Player.SendMessage(p, "Approchez vous d'un train pour monter");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/ride - Permet de faire des balades en train.");
        }
    }
}