using System;

namespace MCWorlds
{
    public class CmdMove : Command
    {
        public override string name { get { return "move"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdMove() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            // /move name map
            // /move name map world
            // /move name x y z

            if (message.Split(' ').Length < 2 || message.Split(' ').Length > 4) { Help(p); return; }

            string lvl = "";
            string world = "";

            if (message.Split(' ').Length == 2 || message.Split(' ').Length == 3)     // /move name map || /move name map world
            {
                Player who = Player.Find(message.Split(' ')[0]);
                if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur."); return; }
                if (message.Split(' ').Length == 2)
                { lvl = message.Split(' ')[1]; }
                else { lvl = message.Split(' ')[1]; world = message.Split(' ')[2]; }

                Level where = Level.Find(lvl,world);
                if (where == null) { Player.SendMessage(p, "Impossible de trouver la map."); return; }
                if (p != null && who.group.Permission > p.group.Permission) { Player.SendMessage(p, "Impossible de deplacer une personne d'un rang superieur au votre."); return; }

                Command.all.Find("goto").Use(who, where.name + " " + where.world);
                if (who.level == where)
                    Player.SendMessage(p, "Le joueur " + who.color + who.name + Server.DefaultColor + " est envoye sur " + where.name + " du monde " + world + ".");
                else
                    Player.SendMessage(p, where.name + " n'est pas charge.");
            }
            else
            {
                // /move name x y z

                Player who;

                if (message.Split(' ').Length == 4)
                {
                    who = Player.Find(message.Split(' ')[0]);
                    if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur"); return; }
                    if (p != null && who.group.Permission > p.group.Permission) { Player.SendMessage(p, "Impossible de deplacer une personne d'un rang superieur au votre."); return; }
                    message = message.Substring(message.IndexOf(' ') + 1);
                }
                else
                {
                    who = p;
                }

                try
                {
                    ushort x = System.Convert.ToUInt16(message.Split(' ')[0]);
                    ushort y = System.Convert.ToUInt16(message.Split(' ')[1]);
                    ushort z = System.Convert.ToUInt16(message.Split(' ')[2]);
                    x *= 32; x += 16;
                    y *= 32; y += 32;
                    z *= 32; z += 16;
                    unchecked { who.SendPos((byte)-1, x, y, z, p.rot[0], p.rot[1]); }
                    if (p != who) Player.SendMessage(p, who.color + who.name + "deplace" );
                }
                catch { Player.SendMessage(p, "coordone invalides"); }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/move [joueur] [x] [y] [z] - Pour teleporter un joueur sur d'autres coordonees");
            Player.SendMessage(p, "/move [joueur] [map] <monde> - Permet de deplacer un joueur");
        }
    }
}