using System;

namespace MCWorlds
{
    public class CmdEmote : Command
    {
        public override string name { get { return "emote"; } }
        public override string shortcut { get { return "<3"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdEmote() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.parseSmiley = !p.parseSmiley;
            p.smileySaved = false;

            if (p.parseSmiley) Player.SendMessage(p, "Les icones ont ete active.");
            else Player.SendMessage(p, "Les icones ont ete desactive.");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/emote - Active ou desactive les icones");
        }
    }
}