using System;

namespace MCWorlds
{
    public class CmdTitle : Command
    {
        public override string name { get { return "title"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdTitle() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            int pos = message.IndexOf(' ');
            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur."); return; }

            if (p != null)
            {
                if (p.group.Permission <= LevelPermission.Vip && p != who) { Player.SendMessage(p, "Vous ne pouvez pas changer le titre d'un autre joueur"); return; }
            }

            string query;
            string newTitle = "";
            if (message.Split(' ').Length > 1) newTitle = message.Substring(pos + 1);
            else
            {
                who.title = "";
                who.SetPrefix();
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " a perdu son titre.", false);
                query = "UPDATE Players SET Title = '' WHERE Name = '" + who.name + "'";
                MySQL.executeQuery(query);
                return;
            }

            if (newTitle != "")
            {
                newTitle = newTitle.ToString().Trim().Replace("[", "");
                newTitle = newTitle.Replace("]", "");
            }

            if (newTitle.Length > 17) { Player.SendMessage(p, "Le titre doit avoir 17 caracteres maximum."); return; }
            if (!Server.devs.Contains(who.name) && newTitle.ToLower() == "dev") { Player.SendMessage(p, "Vous ne pouvez pas faire ca."); return; }

            newTitle = newTitle.Replace("'", " ");
            if (newTitle != "")
                Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " a gagne le titre de &b[" + newTitle + "]", false);
            else Player.GlobalChat(null, who.color + who.prefix + who.Name() + Server.DefaultColor + " a perdu son titre.", false);

            if (newTitle == "")
            {
                query = "UPDATE Players SET Title = '' WHERE Name = '" + who.name + "'";
            }
            else
            {
                query = "UPDATE Players SET Title = '" + newTitle.Replace("'", "\'") + "' WHERE Name = '" + who.name + "'";
            }
            MySQL.executeQuery(query);
            who.title = newTitle;
            who.SetPrefix();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/title [joueur] <titre> - donne un titre a <joueur>.");
            Player.SendMessage(p, "Si pas de titre ecrit, celui ci est supprime.");
        }
    }
}
