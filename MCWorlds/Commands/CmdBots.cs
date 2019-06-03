using System;
using System.IO;

namespace MCWorlds
{
    public class CmdBots : Command
    {
        public override string name { get { return "bots"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBots() { }

        public override void Use(Player p, string message)
        {
            if (message != "")
            {
                Level lvl = null;
                if (message.IndexOf(' ') == -1)
                {
                    if (p != null)
                    { lvl = Level.Find(message, p.level.world); }
                    else
                    { lvl = Level.Find(message, Server.mainWorld); }
                }
                else
                { lvl = Level.Find(message.Split(' ')[0], message.Split(' ')[1]); }
                if (lvl == null) { Player.SendMessage(p, "Il n'existe pas de map '" + message + "' charge"); return; }

                message = "";
                foreach (PlayerBot Pb in PlayerBot.playerbots)
                {
                    if (lvl == Pb.level)
                    {
                        if (Pb.AIName != "") message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")[" + Pb.AIName + "]";
                        else if (Pb.hunt) message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")[Hunt]";
                        else message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")";

                        if (Pb.kill) message += "-kill";
                    }
                    if (message != "") Player.SendMessage(p, "&1Bots: " + Server.DefaultColor + message.Remove(0, 2));
                    else Player.SendMessage(p, "Il n'y a pas de bot en vie sur '" + lvl.name + "'.");
                }
            }
            else
            {
                message = "";
                foreach (PlayerBot Pb in PlayerBot.playerbots)
                {
                    if (Pb.AIName != "") message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")[" + Pb.AIName + "]";
                    else if (Pb.hunt) message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")[Hunt]";
                    else message += ", " + Pb.name + "(" + Pb.level.name + ", " + Pb.level.world + ")";

                    if (Pb.kill) message += "-kill";
                }

                if (message != "") Player.SendMessage(p, "&1Bots: " + Server.DefaultColor + message.Remove(0, 2));
                else Player.SendMessage(p, "Il n'y a pas de bot en vie.");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/bots - Affiche la liste des bot, leurs IA et la map ou ils sont");
            Player.SendMessage(p, "/bots [map] - Affiche les bots present sur une map de votre monde");
            Player.SendMessage(p, "/bots [map] [monde] - Affiche les bots present sur une map du monde demande");
        }
    }
}