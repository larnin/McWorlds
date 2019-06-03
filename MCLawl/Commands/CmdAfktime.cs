using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    public class CmdAfktime : Command
    {
        public override string name { get { return "afktime"; } }
        public override string shortcut { get { return "afkt"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdAfktime() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            DateTime temps = DateTime.Now;

            if (p.joker || Server.chatmod)
            {
                message = "";
            }

            if (!Server.afkset.Contains(p.name))
            {
                Server.afkset.Add(p.name);
                if (p.muted || Server.chatmod && !p.voice)
                {
                    message = "";
                }
                Player.GlobalMessage("-" + p.color + p.Name() + Server.DefaultColor + "- est AFK " + message);

                Thread Threadafk = new Thread(new ThreadStart(delegate
                {
                    while (Server.afkset.Contains(p.name))
                    {
                        while ( (temps.AddMinutes(5) > DateTime.Now ) && ( Server.afkset.Contains(p.name)))
                        Thread.Sleep(1000);

                        if (Server.afkset.Contains(p.name))
                        Player.GlobalMessage("-" + p.color + p.Name() + Server.DefaultColor + "- est toujours AFK " + message);
                        temps = DateTime.Now;
                    }
                }));
                Threadafk.Start();
                
                return;
            }
            else
            {
                Server.afkset.Remove(p.name);
                Player.GlobalMessage("-" + p.color + p.Name() + Server.DefaultColor + "- n'est plus AFK");

                return;
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/afktime <raison> - mettez vous AFK (absent) avec la raison qui se repete toutes les 5min.");
        }
    }
}