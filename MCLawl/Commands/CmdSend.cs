using System;
using System.Collections.Generic;
using System.Data;

namespace MCWorlds
{
    public class CmdSend : Command
    {
        public override string name { get { return "send"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSend() { }

        public override void Use(Player p, string message)
        {
            Player who = null;
            if (p == null) 
            {
                who = Player.Find(message.Split(' ')[0]);
                if (who == null) { Player.SendMessage(p, "joueur introuvable"); return; }
                Server.s.Log("[>] Mp " + who.name + " : " + message);
                Player.SendMessage(who, "&9[>]" + Server.DefaultColor + " Mp console: &f" + message);
                return;
            }

            if (message.Split(' ')[0] == "console") 
            {
                message = message.Substring(message.IndexOf(' ') + 1);
                Server.s.Log("[<] Mp " + p.name + " : " + message);
                Player.SendMessage(p, Server.DefaultColor + "[<] Mp console: &f" + message);
                return; 
            }

            if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }

            who = Player.Find(message.Split(' ')[0]);

            string whoTo;
            if (who != null) whoTo = who.name;
            else whoTo = message.Split(' ')[0];

            message = message.Substring(message.IndexOf(' ') + 1);

            //DB
            MySQL.executeQuery("CREATE TABLE if not exists `Inbox" + whoTo + "` (PlayerFrom CHAR(20), TimeSent DATETIME, Contents VARCHAR(255));");
            MySQL.executeQuery("INSERT INTO `Inbox" + whoTo + "` (PlayerFrom, TimeSent, Contents) VALUES ('" + p.name + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + message.Replace("'", "\\'") + "')");
            //DB

            Player.SendMessage(p, "Message envoye a &5" + whoTo + ".");
            if (who != null) who.SendMessage("Message recu de &5" + p.name + Server.DefaultColor + ".");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/send [nom] [message] - Envoie un message a [nom].");
            Player.SendMessage(p, "/send console [message] - Envoie un message a la console du serveur.");
        }
    }
}