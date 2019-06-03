using System;

namespace MCWorlds
{
    public class CmdWhitelist : Command
    {
        public override string name { get { return "whitelist"; } }
        public override string shortcut { get { return "w"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdWhitelist() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            int pos = message.IndexOf(' ');
            if (pos != -1)
            {
                string action = message.Substring(0, pos);
                string player = message.Substring(pos + 1);

                switch (action)
                {
                    case "add":
                        if (Server.whiteList.Contains(player))
                        {
                            Player.SendMessage(p, "Le joueur &f" + player + Server.DefaultColor + " est deja dans la listeblanche !");
                            break;
                        }
                        Server.whiteList.Add(player);
                        Player.GlobalMessageOps(p.color + p.prefix + p.name + Server.DefaultColor + " ajoute &f" + player + Server.DefaultColor + " dans la listeblanche.");
                        Server.whiteList.Save("whitelist.txt");
                        Server.s.Log("WHITELIST: Added " + player);
                        break;
                    case "del":
                        if (!Server.whiteList.Contains(player))
                        {
                            Player.SendMessage(p, "&f" + player + Server.DefaultColor + " n'est pas dans la liste blanche!");
                            break;
                        }
                        Server.whiteList.Remove(player);
                        Player.GlobalMessageOps(p.color + p.prefix + p.name + Server.DefaultColor + " enleve &f" + player + Server.DefaultColor + " de la listeblanche.");
                        Server.whiteList.Save("whitelist.txt");
                        Server.s.Log("WHITELIST: Removed " + player);
                        break;
                    case "list":
                        string output = "Whitelist:&f";
                        foreach (string wlName in Server.whiteList.All())
                        {
                            output += " " + wlName + ",";
                        }
                        output = output.Substring(0, output.Length - 1);
                        Player.SendMessage(p, output);
                        break;
                    default:
                        Help(p);
                        return;
                }
            }
            else
            {
                if (message == "list")
                {
                    string output = "Whitelist:&f";
                    foreach (string wlName in Server.whiteList.All())
                    {
                        output += " " + wlName + ",";
                    }
                    output = output.Substring(0, output.Length - 1);
                    Player.SendMessage(p, output);
                }
                else
                {
                    Help(p);
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/whitelist list - Liste les joueurs present dans la liste blache");
            Player.SendMessage(p, "/whitelist add [joueur] - Ajoute un joueur dans la liste blache");
            Player.SendMessage(p, "/whitelist del [joueur] - Enleve un joueur de la liste blache.");
        }
    }
}
