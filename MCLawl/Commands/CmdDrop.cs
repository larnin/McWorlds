using System;
using System.Collections.Generic;


namespace MCWorlds
{
    public class CmdDrop : Command
    {
        public override string name { get { return "drop"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public CmdDrop() { }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CTFGame2 game = (CTFGame2)Server.allGames.Find(g => g.lvl == p.level);
            if (game == null) { Player.SendMessage(p, "Le ctf n'est pas en cours sur cette map"); return; }

            if (message != "") { Help(p); return; }
            if (p.hasflag != null)
            {
                game.DropFlag(p, p.hasflag);
                return;
            }
            else
            {
                Player.SendMessage(p, "Vous n'avez pas le drapeau.");
            }

        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/drop - Pose le drapeau a votre position.");
            Player.SendMessage(p, "Cette fonction n'est utilisable que lorsque le ctf est active");
        }
    }
}
