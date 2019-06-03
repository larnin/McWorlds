using System;
using System.IO;

namespace MCWorlds
{
    public class CmdKill : Command
    {
        public override string name { get { return "kill"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdKill() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }

            Player who; string killMsg; int killMethod = 0;
            if (message.IndexOf(' ') == -1)
            {
                who = Player.Find(message);
                killMsg = " s'est fait tuer par " + p.color + p.Name();
            }
            else
            {
                who = Player.Find(message.Split(' ')[0]);
                message = message.Substring(message.IndexOf(' ') + 1);

                if (message.IndexOf(' ') == -1)
                {
                    if (message.ToLower() == "explode")
                    {
                        killMsg = " s'est fait exploser par " + p.color + p.Name();
                        killMethod = 1;
                    }
                    else
                    {
                        killMsg = " " + message;
                    }
                }
                else
                {
                    if (message.Split(' ')[0].ToLower() == "explode")
                    {
                        killMethod = 1;
                        message = message.Substring(message.IndexOf(' ') + 1);
                    }

                    killMsg = " " + message;
                }
            }

            if (who == null)
            {
                p.HandleDeath(Block.rock, " s'est tue dans sa confusion.");
                Player.SendMessage(p, "Impossible de trouver le joueur");
                return;
            }

            if (who.group.Permission > p.group.Permission)
            {
                p.HandleDeath(Block.rock, " s'est fait tue par " + who.color + who.Name());
                Player.SendMessage(p, "Impossible de tuer une personne de rang superieur");
                return;
            }

            if (killMethod == 1)
                who.HandleDeath(Block.rock, killMsg, true);
            else
                who.HandleDeath(Block.rock, killMsg);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/kill [pseudo] <message> - Tue un joueur");
            Player.SendMessage(p, "/kill [pseudo] explode <message> - Tue un joueur en creant une explosion");
        }
    }
}