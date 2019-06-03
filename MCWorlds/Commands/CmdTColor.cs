using System;

namespace MCWorlds
{
    public class CmdTColor : Command
    {
        public override string name { get { return "tcolor"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdTColor() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            string[] args = message.Split(' ');
            Player who = Player.Find(args[0]);
            if (p != null)
            {
                if (p.group.Permission <= LevelPermission.Vip && p != who) { Player.SendMessage(p, "Vous ne pouvez pas changer la couleur de titre d'un autre joueur"); return; }
            }
                
            if (who == null)
            {
                Player.SendMessage(p, "Joueur introuvable.");
                return;
            }
            if (args.Length == 1)
            {
                who.titlecolor = "";
                Player.GlobalChat(who, who.color + who.Name() + Server.DefaultColor + " a perdu sa couleur de titre.", false);
                MySQL.executeQuery("UPDATE Players SET title_color = '' WHERE Name = '" + who.name + "'");
                who.SetPrefix();
                return;
            }
            else
            {
                string color = c.Parse(args[1]);
                if (color == "") { Player.SendMessage(p, "Il n'existe pas de couleur \"" + args[1] + "\"."); return; }
                else if (color == who.titlecolor) { Player.SendMessage(p, who.name + " a deja cette couleur."); return; }
                else
                {
                    MySQL.executeQuery("UPDATE Players SET title_color = '" + c.Name(color) + "' WHERE Name = '" + who.name + "'");
                    Player.GlobalChat(who, who.color + who.Name() + Server.DefaultColor + " a sa couleur de titre qui a change en " + color + c.Name(color) + Server.DefaultColor + ".", false);
                    who.titlecolor = color;
                    who.SetPrefix();
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/tcolor [joueur] <couleur> - Donne une coupeur au titre de <joueur>.");
            Player.SendMessage(p, "Si pas de couleur entre, la couleur est supprime.");
            Player.SendMessage(p, "&0black &1navy &2green &3teal &4maroon &5purple &6gold &7silver");
            Player.SendMessage(p, "&8gray &9blue &alime &baqua &cred &dpink &eyellow &fwhite");
        }
    }
}
