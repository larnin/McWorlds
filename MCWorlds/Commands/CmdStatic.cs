using System;
using System.IO;

namespace MCWorlds
{
    public class CmdStatic : Command
    {
        public override string name { get { return "static"; } }
        public override string shortcut { get { return "t"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdStatic() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            p.staticCommands = !p.staticCommands;
            p.ClearBlockchange();
            p.BlockAction = 0;

            string staticname = "&4OFF";
            if (p.staticCommands) { staticname = "&2ON"; }
            Player.SendMessage(p, "Mode static : " + staticname);

            try
            {
                if (message != "")
                {
                    if (message.IndexOf(' ') == -1)
                    {
                        if (p.group.CanExecute(Command.all.Find(message)))
                            Command.all.Find(message).Use(p, "");
                        else
                            Player.SendMessage(p, "Impossible d'utiliser cette commande.");
                    }
                    else
                    {
                        if (p.group.CanExecute(Command.all.Find(message.Split(' ')[0])))
                            Command.all.Find(message.Split(' ')[0]).Use(p, message.Substring(message.IndexOf(' ') + 1));
                        else
                            Player.SendMessage(p, "Impossible d'utiliser cette commande.");
                    }
                }
            }
            catch { Player.SendMessage(p, "La commande demande est introuvable"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/static <commande> - Permet d'utiliser certaines commandes plusieurs fois sans les retape.");
        }
    }
}