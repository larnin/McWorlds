using System;

namespace MCWorlds
{
    public class CmdMute : Command
    {
        public override string name { get { return "mute"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdMute() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "Le joueur entre n'est pas en ligne.");
                return;
            }
            if (who.muted)
            {
                who.muted = false;
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " n'est &bplus muet.", false);
            }
            else
            {
                if (p != null)
                {
                    if (who != p) if (who.group.Permission > p.group.Permission) { Player.SendMessage(p, "Impossible de mettre en sourdine une personne de rang superieur ou egal au votre."); return; }
                }
                who.muted = true;
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " a &8perdu la voix.", false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/mute [joueur] - Met en sourdine un joueur.");
        }
    }
}