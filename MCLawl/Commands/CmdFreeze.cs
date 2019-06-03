using System;
using System.IO;

namespace MCWorlds
{
    public class CmdFreeze : Command
    {
        public override string name { get { return "freeze"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdFreeze() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player who = Player.Find(message);
            if (who == null) { Player.SendMessage(p, "Joueur introuvable."); return; }
            else if (who == p) { Player.SendMessage(p, "Impossible de vous glacer vous meme."); return; }
            else if (p != null) { if (who.group.Permission >= p.group.Permission) { Player.SendMessage(p, "Impossible de glacer un joueur de rang egal ou superieur."); return; } }

            if (!who.frozen)
            {
                who.frozen = true;
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " est &bgele.", false);
            }
            else
            {
                who.frozen = false;
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " a &adegele.", false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/freeze [nom] - Arrette le joueur , reutiliser pour degeler");
        }
    }
}