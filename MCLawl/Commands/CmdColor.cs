using System;

namespace MCWorlds
{
    public class CmdColor : Command
    {
        public override string name { get { return "color"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdColor() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            int pos = message.IndexOf(' ');
            if (pos != -1)
            {
                Player who = Player.Find(message.Substring(0, pos));
                if (who == null) { Player.SendMessage(p, "Il n'existe pas de joueur \"" + message.Substring(0, pos) + "\"!"); return; }
                if (p.group.Permission <= LevelPermission.Vip && p != who) { Player.SendMessage(p, "Vous ne pouvez pas changer la couleur d'un autre joueur"); return; }
                if (message.Substring(pos + 1) == "del")
                {
                    MySQL.executeQuery("UPDATE Players SET color = '' WHERE name = '" + who.name + "'");
                    Player.GlobalChat(who, "* La couleur de " + who.color + Name(who.Name()) + " est revenu " + who.group.color + "a la couleur par default" + Server.DefaultColor + ".", false);
                    who.color = who.group.color;

                    Player.GlobalDie(who, false);
                    Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                    who.SetPrefix();
                    return;
                }
                string color = c.Parse(message.Substring(pos + 1));
                if (color == "") { Player.SendMessage(p, "Il n'existe pas de couleur \"" + message + "\"."); }
                else if (color == who.color) { Player.SendMessage(p, who.name + " a deja cette couleur."); }
                else
                {
                    MySQL.executeQuery("UPDATE Players SET color = '" + c.Name(color) + "' WHERE name = '" + who.name + "'");

                    Player.GlobalChat(who,  "* La couleur de " + who.color + Name(who.Name()) + " a change en " + color + c.Name(color) + Server.DefaultColor + ".", false);
                    who.color = color;

                    Player.GlobalDie(who, false);
                    Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                    who.SetPrefix();
                }
            }
            else
            {
                if (message == "del")
                {
                    MySQL.executeQuery("UPDATE Players SET color = '' WHERE name = '" + p.name + "'");

                    Player.GlobalChat(p, p.color + "*La couleur de " + Name(p.Name()) + " est revenu " + p.group.color + "a la couleur par default" + Server.DefaultColor + ".", false);
                    p.color = p.group.color;

                    Player.GlobalDie(p, false);
                    Player.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
                    p.SetPrefix();
                    return;
                }
                string color = c.Parse(message);
                if (color == "") { Player.SendMessage(p, "Il n'existe pas de couleur \"" + message + "\"."); }
                else if (color == p.color) { Player.SendMessage(p, "Vous avez deja cette couleur."); }
                else
                {
                    MySQL.executeQuery("UPDATE Players SET color = '" + c.Name(color) + "' WHERE name = '" + p.name + "'");

                    Player.GlobalChat(p, "*La couleur de " + p.color + Name(p.Name()) + " a change en " + color + c.Name(color) + Server.DefaultColor + ".", false);
                    p.color = color;

                    Player.GlobalDie(p, false);
                    Player.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
                    p.SetPrefix();
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/color [joueur] <couleur/del>- Change la couleur du nom.");
            Player.SendMessage(p, "/color [joueur] del - Remet la couleur par defaut au joueur");
            Player.SendMessage(p, "&0black &1navy &2green &3teal &4maroon &5purple &6gold &7silver");
            Player.SendMessage(p, "&8gray &9blue &alime &baqua &cred &dpink &eyellow &fwhite");
        }
        static string Name(string name)
        {
            string ch = name[name.Length - 1].ToString().ToLower();
            return name + Server.DefaultColor;
        }
    }
}