using System;
using System.IO;

namespace MCWorlds
{
    public class CmdSummon : Command
    {
        public override string name { get { return "summon"; } }
        public override string shortcut { get { return "s"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSummon() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }
            if (message.ToLower() == "all")
            {
                foreach (Player pl in Player.players)
                {
                    if (pl.level == p.level && pl != p)
                    {
                        unchecked { pl.SendPos((byte)-1, p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0); }
                        pl.SendMessage("Vous avez ete appele par " + p.color + p.Name() + Server.DefaultColor + ".");
                    }
                }
                return;
            }

            Player who = Player.Find(message);
            if (who == null || who.hidden) { Player.SendMessage(p, "Il n'y a pas de joueur \"" + message + "\"!"); return; }
            if (p.level != who.level) 
            { 
                Player.SendMessage(p, who.name + " est dans une map differente, changement de map ...");
                Command.all.Find("goto").Use(who, p.level.name + " " + p.level.world);
                while (p.Loading) { }
            }
            unchecked { who.SendPos((byte)-1, p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0); }
            who.SendMessage("Vous avez ete appele par " + p.color + p.name + Server.DefaultColor + ".");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/summon [joueur] - Deplace un joueur a votre position.");
            Player.SendMessage(p, "/summon all - Deplace tous les joueurs presents sur la map");
        }
    }
}