using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdAfk : Command
    {
        public override string name { get { return "afk"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdAfk() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "list")
            {
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
                    IRCBot.Say(p.name + " is AFK " + message);
                    p.afkStart = DateTime.Now;
                    return;

                }
                else
                {
                    Server.afkset.Remove(p.name);
                    Player.GlobalMessage("-" + p.color + p.Name() + Server.DefaultColor + "- n'est plus AFK");
                    IRCBot.Say(p.name + " n'est plus AFK");
                    return;
                }
            }
            else
            {
                foreach (string s in Server.afkset) Player.SendMessage(p, s);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/afk <raison> - mettez vous AFK (absent).");
            Player.SendMessage(p, "/afk list - liste tous les joueurs afk.");
        }
    }
}
