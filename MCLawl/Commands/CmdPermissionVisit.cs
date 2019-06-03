
using System;

namespace MCWorlds
{
    public class CmdPermissionVisit : Command
    {
        public override string name { get { return "pervisit"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPermissionVisit() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.Split(' ').Length > 1) { Help(p); return; }

            if (p.level.world.ToLower() != p.name.ToLower() && p.group.Permission < LevelPermission.Admin)
            { Player.SendMessage(p, "Vous n'etes pas autoriser a utiliser cette commande dans ce monde"); return; }

            Level lvl = p.level;

            if (message != "")
            {
                lvl = Level.Find(message, p.level.world);
                if (lvl == null)
                { Player.SendMessage(p, "La map \"" + lvl + "\" n'est pas chargee"); return; }
            }

            lvl.pervisit = !lvl.pervisit;

            if (lvl.pervisit)
            { Player.SendMessage(p, "Vous autorisez tout le monde a visiter la map " + lvl.name); }
            else { Player.SendMessage(p, "Vous interdisez le passage sur la map " + lvl.name); }
            
            Server.s.Log("Pervisit " + lvl.pervisit + " sur " + lvl.name + " (" + lvl.world + ")");

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pervisit <map> - Modifie la permition de passage sur une map.");
            Player.SendMessage(p, "Attention : si vous activez la pervisit plus personne ne pourra passer sur votre map");
        }
    }
}