using System;
using System.IO;

namespace MCWorlds
{
    public class CmdInvincible : Command
    {
        public override string name { get { return "invincible"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdInvincible() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            Player who;
            if (message != "")
            {
                who = Player.Find(message);
            }
            else
            {
                who = p;
            }

            if (who == null)
            {
                Player.SendMessage(p, "Impossible de trouver le joueur.");
                return;
            }

            if (who.group.Permission > p.group.Permission)
            {
                Player.SendMessage(p, "Impossible de rendre invincible une personne de rang superieur au votre");
                return;
            }

            if (who.invincible == true)
            {
                who.invincible = false;
                if (Server.cheapMessage)
                    Player.GlobalChat(p, who.color + who.Name() + Server.DefaultColor + " " + Server.uncheapMessageGiven, false);
            }
            else
            {
                who.invincible = true;
                if (Server.cheapMessage)
                    Player.GlobalChat(p, who.color + who.Name() + Server.DefaultColor + " " + Server.cheapMessageGiven, false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/invincible - Vous rend immortel.");
            Player.SendMessage(p, "/invincible [nom] - Rend immortel un joueur.");
        }
    }
}