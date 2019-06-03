
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdExit : Command
    {
        public override string name { get { return "exit"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdExit() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "" || message.Split(' ').Length > 1 ) { Help(p); return; }

            Player who = Player.Find(message);

            if (p == who)
            { Player.SendMessage(p, "Pourquoi vous virer vous meme ?"); return; }

            if (who == null)
            { Player.SendMessage(p, "Le joueur est introuvable"); return; }

            if (who.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Le joueur " + who.name + " n'est pas dans votre monde"); return; }

            if (who.group.Permission == LevelPermission.Admin)
            {
                Player.SendMessage(p, "Un message lui demandant de partir est envoyer a " + who.name);
                Player.SendMessage(who, "Le joueur " + p.Name() + " voudrais que vous sortiez de sa map");
                return;
            }
            else
            {
                Command.all.Find("main").Use(who, "");
                Player.SendMessage(who, "Le joueur " + p.Name() + " vous a sortie de son monde");
                Player.SendMessage(p, "Vous avez renvoyer le joueur " + who.name + " sur la map principale du serveur");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/exit [joueur] - Permet de faire sortir un joueur de son monde (il sera renvoye au spawn).");
            Player.SendMessage(p, "Si ce joueur est un admin, un message lui sera envoye");
        }
    }
}
