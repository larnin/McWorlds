using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdAward : Command
    {
        public override string name { get { return "award"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdAward() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }

            bool give = true;
            if (message.Split(' ')[0].ToLower() == "give")
            {
                give = true;
                message = message.Substring(message.IndexOf(' ') + 1);
            }
            else if (message.Split(' ')[0].ToLower() == "take")
            {
                give = false;
                message = message.Substring(message.IndexOf(' ') + 1);
            }
            
            string foundPlayer = message.Split(' ')[0];
            Player who = Player.Find(message);
            if (who != null) foundPlayer = who.name;
            string awardName = message.Substring(message.IndexOf(' ') + 1);
            if (!Awards.awardExists(awardName))
            {
                Player.SendMessage(p, "Ce trophee n'existe pas");
                Player.SendMessage(p, "Utilise /awards pour avoir la liste des trophees");
                return;
            }

            if (give)
            {
                if (Awards.giveAward(foundPlayer, awardName))
                {
                    Player.GlobalChat(p, Server.FindColor(foundPlayer) + foundPlayer + Server.DefaultColor + " a eu le trophe: &b" + Awards.camelCase(awardName), false);
                }
                else
                {
                    Player.SendMessage(p, "Ce joueur a deja ce trophe");
                }
            }
            else
            {
                if (Awards.takeAward(foundPlayer, awardName))
                {
                    Player.GlobalChat(p, Server.FindColor(foundPlayer) + foundPlayer + Server.DefaultColor + " a perdu le trophe &b" + Awards.camelCase(awardName) + Server.DefaultColor , false);
                }
                else
                {
                    Player.SendMessage(p, "Ce joueur n'a pas le trophe que vous voulez lui enlever");
                }
            }

            Awards.Save();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/award give [joueur] [trophe] - Donne le trophe au joueur");
            Player.SendMessage(p, "/award take [joueur] [trophe] - Enleve le trophe au joueur");
            Player.SendMessage(p, "[trophe] a besoin du nom complet du trophee");
        }
    }
}