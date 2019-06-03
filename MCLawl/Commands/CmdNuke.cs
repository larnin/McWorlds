using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    class CmdNuke : Command
    {
        public override string name { get { return "nuke"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdNuke() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!p.level.pergun)
            { Player.SendMessage(p, "Vous ne pouvez pas utiliser le gun dans cette map"); return; }

            ushort x = p.lastClick[0], y = p.lastClick[1], z = p.lastClick[2] ;

            if (p.nuke)
            {
                Player.SendMessage(p, "Mode nuke desactive");
                p.nuke = false;
            }
            else
            {
                p.nuke = true;
                Player.GlobalMessageLevel(p.level, "Le joueur " + p.Name() + " a active le mode nuke");
                p.painting = false;

                Thread Threadnuke = new Thread(new ThreadStart(delegate
                {
                    while (p.nuke)
                    {
                        if (!p.level.pergun)
                        { p.nuke = false; }

                        if (x != p.lastClick[0] || y != p.lastClick[1] || z != p.lastClick[2])
                        {
                            x = p.lastClick[0];
                            y = p.lastClick[1];
                            z = p.lastClick[2];
                            p.level.MakeExplosion(x, y, z, 7); 
                        }
                        Thread.Sleep(50);
                    }
                }));
                Threadnuke.Start();
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/nuke - permet de creer une enorme explosion.");
        }
    }
}
