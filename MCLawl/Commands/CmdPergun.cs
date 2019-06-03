
using System;

namespace MCWorlds
{
    public class CmdPergun : Command
    {
        public override string name { get { return "pergun"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPergun() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') != -1) { Help(p); return; }

            if (p.level.world.ToLower() != p.name.ToLower() && p.group.Permission < LevelPermission.Admin)
            { Player.SendMessage(p, "Vous n'etes pas autoriser a utiliser cette commande dans ce monde"); return; }

            Level lvl = null;

            if (message == "")
            { lvl = p.level; }
            else
            {
                lvl = Level.Find(message, p.level.world);
                if (lvl == null)
                { Player.SendMessage(p, "La map \"" + lvl + "\" n'est pas chargee"); return; }
            }

            lvl.pergun= !lvl.pergun;

            if (lvl.pergun)
            {
                Player.GlobalMessageLevel(lvl, "Le gun est maintenant autorise");
                if (p.level != lvl)
                { Player.SendMessage(p, "Armes autorise sur " + lvl); }
            }
            else
            {
                Player.GlobalMessageLevel(lvl, "Vous ne pouvez plus utilisez le gun sur cette map");
                if (p.level != lvl)
                { Player.SendMessage(p, "Armes interdit sur " + lvl); }
            }

            Server.s.Log("Pergun " + lvl.pergun + " sur " + lvl.name + " (" + lvl.world + ")");

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pergun <map> - Permet ou non l'utilisation des armes sur la map.");
            Player.SendMessage(p, "Armes gere : gun, tnt, missile, nuke et lmissile");
        }
    }
}