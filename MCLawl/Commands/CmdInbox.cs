using System;
using System.Collections.Generic;
using System.Data;

namespace MCWorlds
{
    public class CmdInbox : Command
    {
        public override string name { get { return "inbox"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdInbox() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            try
            {
                MySQL.executeQuery("CREATE TABLE if not exists `Inbox" + p.name + "` (PlayerFrom CHAR(20), TimeSent DATETIME, Contents VARCHAR(255));");
                if (message == "")
                {
                    DataTable Inbox = MySQL.fillData("SELECT * FROM `Inbox" + p.name + "` ORDER BY TimeSent");

                    if (Inbox.Rows.Count == 0) { Player.SendMessage(p, "Pas de messages."); Inbox.Dispose(); return; }

                    for (int i = 0; i < Inbox.Rows.Count; ++i)
                    {
                        Player.SendMessage(p, i + ": Par &5" + Inbox.Rows[i]["PlayerFrom"].ToString() + Server.DefaultColor + " a &a" + Inbox.Rows[i]["TimeSent"].ToString());
                    }
                    Inbox.Dispose();
                }
                else if (message.Split(' ')[0].ToLower() == "del" || message.Split(' ')[0].ToLower() == "delete")
                {
                    int FoundRecord = -1;

                    if (message.Split(' ')[1].ToLower() != "all")
                    {
                        try
                        {
                            FoundRecord = int.Parse(message.Split(' ')[1]);
                        }
                        catch { Player.SendMessage(p, "Nombre entre incorrect."); return; }

                        if (FoundRecord < 0) { Player.SendMessage(p, "Le numero du message doit etre positif."); return; }
                    }

                    DataTable Inbox = MySQL.fillData("SELECT * FROM `Inbox" + p.name + "` ORDER BY TimeSent");

                    if (Inbox.Rows.Count - 1 < FoundRecord || Inbox.Rows.Count == 0)
                    {
                        Player.SendMessage(p, "\"" + FoundRecord + "\" n'existe pas."); Inbox.Dispose(); return;
                    }

                    string queryString;
                    if (FoundRecord == -1)
                        queryString = "TRUNCATE TABLE `Inbox" + p.name + "`";
                    else
                        queryString = "DELETE FROM `Inbox" + p.name + "` WHERE PlayerFrom='" + Inbox.Rows[FoundRecord]["PlayerFrom"] + "' AND TimeSent='" + Convert.ToDateTime(Inbox.Rows[FoundRecord]["TimeSent"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                
                    MySQL.executeQuery(queryString);

                    if (FoundRecord == -1)
                        Player.SendMessage(p, "Tous les messages sont supprime.");
                    else
                        Player.SendMessage(p, "Message supprime.");

                    Inbox.Dispose();
                }
                else
                {
                    int FoundRecord;

                    try
                    {
                        FoundRecord = int.Parse(message);
                    }
                    catch { Player.SendMessage(p, "Nombre entre incorrect."); return; }

                    if (FoundRecord < 0) { Player.SendMessage(p, "Le numero du message doit etre positif."); return; }

                    DataTable Inbox = MySQL.fillData("SELECT * FROM `Inbox" + p.name + "` ORDER BY TimeSent");

                    if (Inbox.Rows.Count - 1 < FoundRecord || Inbox.Rows.Count == 0)
                    {
                        Player.SendMessage(p, "\"" + FoundRecord + "\" n'existe pas."); Inbox.Dispose(); return;
                    }

                    Player.SendMessage(p, "Message from &5" + Inbox.Rows[FoundRecord]["PlayerFrom"] + Server.DefaultColor + " sent at &a" + Inbox.Rows[FoundRecord]["TimeSent"] + ":");
                    Player.SendMessage(p, Inbox.Rows[FoundRecord]["Contents"].ToString());
                    Inbox.Dispose();
                }
            }
            catch
            {
                Player.SendMessage(p, "Erreur d'acces a l'inbox. Vous avez peut être pas de messages, reessayez.");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/inbox - Affiche tous les messages.");
            Player.SendMessage(p, "/inbox [num] - Affiche un message");
            Player.SendMessage(p, "/inbox del all - Supprime tous les messages.");
            Player.SendMessage(p, "/inbox del [num] - Supprime un message.");
        }
    }
}