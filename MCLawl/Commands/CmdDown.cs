
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdDown : Command
    {
        public override string name { get { return "down"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdDown() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.Split(' ').Length > 1) { Help(p); return; }

            ushort x = (ushort)(p.pos[0] / 32);
            ushort y = (ushort)((p.pos[1] / 32) - 1), y2 = 0;
            ushort z = (ushort)(p.pos[2] / 32);
            int hauteur = 0;

            if (message == "")
            {
                bool sol = true;
                bool sol2 = true;
                byte bloc = Block.Zero;

                for (int i = y; i >= 0; i--)
                {
                    bloc = p.level.GetTile(x, (ushort)i, z);

                    if (!sol && !sol2 && bloc != Block.air)
                    {
                        hauteur = (ushort)i;
                        i = -1;
                    }

                    sol = sol2;

                    if (bloc == Block.air)
                    { sol2 = false; }
                    else { sol2 = true; }
                }

                if (hauteur == 0)
                { Player.SendMessage(p, "Aucune galerie en dessous"); return; }

                try
                {
                    y2 = (ushort)((hauteur + 1) * 32 + 32);
                    unchecked { p.SendPos((byte)-1, p.pos[0], y2, p.pos[2], p.rot[0], p.rot[1]); }
                }
                catch { Player.SendMessage(p, "coordone invalides"); }

                Player.SendMessage(p, "Descente de " + (y - hauteur - 1));
            }
            else
            {
                try { hauteur = int.Parse(message); }
                catch { Player.SendMessage(p, "Valeur invalide"); return; }

                if (hauteur <= 0)
                { Player.SendMessage(p, "Valeur invalide"); return; }

                if (y - hauteur < 0)
                { hauteur = y; }

                y2 = (ushort)((y - hauteur) * 32 + 32);

                if (y - hauteur > 1)
                {
                    if (p.level.GetTile(x, (ushort)(y - hauteur - 1), z) == Block.air)
                    { p.SendBlockchange(x, (ushort)(y - hauteur - 1), z, Block.glass); }
                }

                try
                { unchecked { p.SendPos((byte)-1, p.pos[0], y2, p.pos[2], p.rot[0], p.rot[1]); } }
                catch { Player.SendMessage(p, "coordone invalides"); }

                Player.SendMessage(p, "Descente de " + hauteur);
            }

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/down - Permet de descendre sur un sol inferieur.");
            Player.SendMessage(p, "/down [nombre blocs] - Permet de descendre du nombre de blocs demande");
        }
    }
}
