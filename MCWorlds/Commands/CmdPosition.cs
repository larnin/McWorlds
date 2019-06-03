using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdPosition : Command
    {
        public override string name { get { return "position"; } }
        public override string shortcut { get { return "pos"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPosition() { }

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

                ushort x = (ushort)(p.pos[0] / 32);
                ushort y = (ushort)((p.pos[1] / 32) - 1);
                ushort z = (ushort)(p.pos[2] / 32);

            Player.SendMessage(p, "Votre position actuelle est : (" + x + ", " + y + ", " + z + ").");
            return;
            }

            if (message.Split(' ').Length != 1)
            { Help(p); return; }

            Player joueur = Player.Find(message);

            if (joueur == null)
            { Player.SendMessage(p, "Joueur introuvable"); return; }
            if (joueur.hidden)
            { Player.SendMessage(p, "Joueur introuvable"); return; }

            ushort x2 = (ushort)(joueur.pos[0] / 32);
            ushort y2 = (ushort)((joueur.pos[1] / 32) - 1);
            ushort z2 = (ushort)(joueur.pos[2] / 32);

            Player.SendMessage(p, "Le joueur " + joueur.color + joueur.name + Server.DefaultColor + " est sur la map %a" + joueur.level.name + Server.DefaultColor + " du monde &a" + joueur.level.world );
            Player.SendMessage(p, "Sa position actuelle est : (" + x2 + ", " + y2 + ", " + z2 + ").");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/position - Affiche votre position actuelle.");
            Player.SendMessage(p, "/position [joueur] - Affiche la map et la position du joueur.");
        }
    }
}
