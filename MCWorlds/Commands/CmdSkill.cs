
using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    class CmdSkill : Command
    {
        public override string name { get { return "seekill"; } }
        public override string shortcut { get { return "skill"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSkill() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible d'utiliser cette commande depuis la console ou l'irc"); }

            if (p.skill) { p.skill = false; return; }

            Player.SendMessage(p, "Tous les joueurs que vous regardez creverons");
            p.skill = true;
            Thread seekillThread = new Thread(new ThreadStart(delegate
            {
                
                while (p.skill)
                {
                    foreach (Player pl in Player.players)
                    {
                        if (pl.level != p.level) { continue; }
                        if (p == pl) { continue; }
                        if (p.pos[0] == pl.pos[0] && p.pos[1] == pl.pos[1] && p.pos[2] == pl.pos[2]) 
                        { pl.HandleDeath(4, " se fait foudroyer du regard par " + p.color + p.Name(), false); continue; }

                        double dist = Math.Sqrt((p.pos[0] - pl.pos[0]) * (p.pos[0] - pl.pos[0]) + (p.pos[1] - pl.pos[1]) * (p.pos[1] - pl.pos[1]) + (p.pos[2] - pl.pos[2]) * (p.pos[2] - pl.pos[2])) ;
                        if (dist > 32 * 6 ) { continue; } // a plus de 6 blocs
                        int x = pl.pos[0] - p.pos[0];
                        int y = pl.pos[1] - p.pos[1];
                        int z = pl.pos[2] - p.pos[2];
                        double dh = Math.Sqrt(x * x + z * z);

                        double angleH = Math.Atan2(z, x) / Math.PI * 128 + 64;
                        if (angleH > 255) { angleH -= 256; }
                        double angleV = Math.Atan2(y, dh) / Math.PI * 128;

                        double dAH = Math.Abs(angleH - p.rot[0]);
                        double dAV = Math.Abs(p.rot[1] - angleV);

                        if ((dAH < 32 || dAH > 224) && (dAV < 32 || dAV > 224)) { pl.HandleDeath(4, " se fait foudroyer du regard par " + p.color + p.Name(), false); }
                    }
                    Thread.Sleep(50);
                }
                Player.SendMessage(p, "Vous ne tuez plus du regard");
            }));
            seekillThread.Start();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/skill - Permet de tuer toutes les personnes tombant dans son champ de vision");
        }
    }
}
