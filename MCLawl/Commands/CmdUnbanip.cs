using System;
using System.Data;
using System.Text.RegularExpressions;

namespace MCWorlds
{
    public class CmdUnbanip : Command
    {
        Regex regex = new Regex(@"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\." +
                                "([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$");
        public override string name { get { return "unbanip"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdUnbanip() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            if (message[0] == '@')
            {
                message = message.Remove(0, 1).Trim();
                Player who = Player.Find(message);
                if (who == null)
                {
                    DataTable ip;
                    int tryCounter = 0;
             rerun: try
                    {
                        ip = MySQL.fillData("SELECT IP FROM Players WHERE Name = '" + message + "'");
                    }
                    catch (Exception e)
                    {
                        tryCounter++;
                        if (tryCounter < 10)
                        {
                            goto rerun;
                        }
                        else
                        {
                            Server.ErrorLog(e);
                            Player.SendMessage(p, "Il y avait une erreur de base de donnees pour chercher l'adresse IP. Il a ete enregistré.");
                            return;
                        }
                    }
                    if (ip.Rows.Count > 0)
                    {
                        message = ip.Rows[0]["IP"].ToString();
                    }
                    else
                    {
                        Player.SendMessage(p, "Impossible de trouver l'adresse ip du joueur.");
                        return;
                    }
                    ip.Dispose();
                }
                else
                {
                    message = who.ip;
                }
            }

            if (!regex.IsMatch(message)) { Player.SendMessage(p, "Ce n'est pas une ip valide!"); return; }
            if (p != null) if (p.ip == message) { Player.SendMessage(p, "Vous ne pouvez pas utiliser cette commande."); return; }
            if (!Server.bannedIP.Contains(message)) { Player.SendMessage(p, message + " ne semble pas etre banni."); return; }
            Player.GlobalMessage(message + " a sont&8 ip debanni" + Server.DefaultColor + "!");
            Server.bannedIP.Remove(message); Server.bannedIP.Save("banned-ip.txt", false);
            Server.s.Log("IP-UNBANNED: " + message.ToLower());
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/unbanip [ip] - Deban une ip.");
            Player.SendMessage(p, "/unbanip @[joueur] - Deban l'ip du joueur");
        }
    }
}