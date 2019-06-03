using System;

namespace MCWorlds
{
    public class CmdOpChat : Command
    {
        public override string name { get { return "opchat"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdOpChat() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.opchat = !p.opchat;
            p.tchatmap = false;
            if (p.opchat) Player.SendMessage(p, "Tous les messages ecrit seront envoye aux OPs.");
            else Player.SendMessage(p, "L'OP tchat a ete desactive");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/opchat - permet d'envoyer tous les messages ecrit aux OPs");
        }
    }
}