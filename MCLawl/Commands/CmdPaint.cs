using System;
using System.IO;

namespace MCWorlds
{
    public class CmdPaint : Command
    {
        public override string name { get { return "paint"; } }
        public override string shortcut { get { return "p"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPaint() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }
            p.painting = !p.painting; if (p.painting) { Player.SendMessage(p, "Mode peinture: &aON" + Server.DefaultColor + "."); }
            else { Player.SendMessage(p, "Mode peinture: &cOFF" + Server.DefaultColor + "."); }
            p.BlockAction = 0;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/paint - Active/desactive le mode peinture.");
            Player.SendMessage(p, "Tous les blocs casse seront remplace par celui en main");
        }
    }
}