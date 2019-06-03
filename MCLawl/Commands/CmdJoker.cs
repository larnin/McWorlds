using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdJoker : Command
    {
        public override string name { get { return "joker"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdJoker() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            bool stealth = false;
            if (message[0] == '#')
            {
                message = message.Remove(0, 1).Trim();
                stealth = true;
                Server.s.Log("Stealth joker attempted");
            }

            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "Impossible de trouver le joueur.");
                return;
            }

            if (!who.joker)
            {
                who.joker = true;
                if (stealth) { Player.GlobalMessageOps(who.color + who.Name() + Server.DefaultColor + " est mis joker discretement. "); return; }
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " est maintenant un &aJ&bo&ck&5e&9r" + Server.DefaultColor + ".", false);
            }
            else
            {
                who.joker = false;
                if (stealth) { Player.GlobalMessageOps(who.color + who.Name() + Server.DefaultColor + " n'est plus joker. "); return; }
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " n'est plus un &aJ&bo&ck&5e&9r" + Server.DefaultColor + ".", false);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/joker [pseudo] - Met le joueur en joker!");
            Player.SendMessage(p, "/joker # [pseudo] - Met discretement le joueur en joker");
            return;
        }
    }
}
