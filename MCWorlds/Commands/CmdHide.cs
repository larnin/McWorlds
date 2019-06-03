using System;
using System.IO;

namespace MCWorlds
{
    public class CmdHide : Command
    {
        public override string name { get { return "hide"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdHide() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') != -1) { Help(p); return; }

            Player who = p;

            if (message != "")
            {
                who = Player.Find(message);
                if (who == null)
                { Player.SendMessage(p, "Joueur introuvable"); return; }
            }

            if (who.possess != "")
            {
                if (p == who)
                { Player.SendMessage(p, "Vous ete en train de posseder une personne."); }
                else { Player.SendMessage(p, who.name + "est en train de posseder une personne."); }
                return;
            }
            who.hidden = !who.hidden;
            if (who.hidden)
            {
                Player.GlobalDie(who, true);
                Player.GlobalMessageOps("Aux Ops -" + who.color + who.Name() + Server.DefaultColor + " - est maintenant &finvisible" + Server.DefaultColor + ".");
                Player.GlobalChat(p, "&c- " + who.color + who.prefix + who.Name() + Server.DefaultColor + " s'est deconnecte.", false);
                //Player.SendMessage(p, "You're now &finvisible&e.");
            }
            else
            {
                Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                Player.GlobalMessageOps("To Ops -" + who.color + who.Name() + Server.DefaultColor + " - est maintenant &8visible" + Server.DefaultColor + ".");
                Player.GlobalChat(p, "&a+ " + who.color + who.prefix + who.Name() + Server.DefaultColor + " a rejoint le jeu.", false);
                //Player.SendMessage(p, "You're now &8visible&e.");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/hide - Permet de se rendre invisible.");
            Player.SendMessage(p, "/hide [nom] - Rend un autre joueur invisible.");
        }
    }
}