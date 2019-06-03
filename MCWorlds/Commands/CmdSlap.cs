using System;

namespace MCWorlds
{
    public class CmdSlap : Command
    {
        public override string name { get { return "slap"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSlap() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }

            Player who = Player.Find(message);

            if (who == null)
            {
                Player.SendMessage(p, "Impossible de trouver le joueur");
                return;
            }

            ushort currentX = (ushort)(who.pos[0] / 32);
            ushort currentY = (ushort)(who.pos[1] / 32);
            ushort currentZ = (ushort)(who.pos[2] / 32);
            ushort foundHeight = 0;

            for (ushort yy = currentY; yy <= 1000; yy++)
            {
                if (!Block.Walkthrough(p.level.GetTile(currentX, yy, currentZ)) && p.level.GetTile(currentX, yy, currentZ) != Block.Zero)
                {
                    foundHeight = (ushort)(yy - 1);
                    who.level.ChatLevel(who.color + who.Name() + Server.DefaultColor + " est lance au plafond par " + p.color + p.Name());
                    break;
                }
            }

            if (foundHeight == 0)
            {
                who.level.ChatLevel(who.color + who.Name() + Server.DefaultColor + " est envoye tres haut dans le ciel par " + p.color + p.Name());
                foundHeight = 1000;
            }
            
            unchecked { who.SendPos((byte)-1, who.pos[0], (ushort)(foundHeight * 32), who.pos[2], who.rot[0], who.rot[1]); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/slap [nom] - envois en hauteur le joueur cible");
        }
    }
}