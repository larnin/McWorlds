using System;
using System.Collections.Generic;
using System.Data;

namespace MCWorlds
{
    public class CmdWhowas : Command
    {
        public override string name { get { return "whowas"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdWhowas() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player pl = Player.Find(message); 
            if (pl != null)
            {
                if (!pl.hidden)
                {
                    Player.SendMessage(p, pl.color + pl.name + Server.DefaultColor + " est en ligne, utilise /whois.");
                    Command.all.Find("whois").Use(p, message);
                    return;
                }
            }

            if (!Player.ValidName(message)) { Player.SendMessage(p, "Impossible d'analyser la demande."); return; }

            string FoundRank = Group.findPlayer(message.ToLower());

            DataTable playerDb = MySQL.fillData("SELECT * FROM Players WHERE Name='" + message + "'");
            if (playerDb.Rows.Count == 0) { Player.SendMessage(p, Group.Find(FoundRank).color + message + Server.DefaultColor + " as le rang de " + Group.Find(FoundRank).color + FoundRank); return; }

            Player.SendMessage(p, Group.Find(FoundRank).color + playerDb.Rows[0]["Title"] + " " + message + Server.DefaultColor + " a :");
            Player.SendMessage(p, "> > le rang de \"" + Group.Find(FoundRank).color + FoundRank);
            try
            {
                if (!Group.Find("Nobody").commands.Contains("pay") && !Group.Find("Nobody").commands.Contains("give") && !Group.Find("Nobody").commands.Contains("take")) Player.SendMessage(p, "> > &a" + playerDb.Rows[0]["Money"] + Server.DefaultColor + " " + Server.moneys);
            }
            catch { }
            Player.SendMessage(p, "> > &cest mort &a" + playerDb.Rows[0]["TotalDeaths"] + Server.DefaultColor + " fois");
            Player.SendMessage(p, "> > &ba modifie &a" + playerDb.Rows[0]["totalBlocks"] + Server.DefaultColor + " blocs.");
            Player.SendMessage(p, "> > &ba le droit a &a" + playerDb.Rows[0]["nbMapsMax"] + Server.DefaultColor + " maps.");
            Player.SendMessage(p, "> > a ete vu la derniere foit le &a" + playerDb.Rows[0]["LastLogin"]);
            Player.SendMessage(p, "> > premiere connection le &a" + playerDb.Rows[0]["FirstLogin"]);
            TimeSpan up = TimeSpan.Parse(playerDb.Rows[0]["totalTimePlayed"].ToString());
            string upTime = "> > A joue pendant : &b";
            if (up.Days == 1) upTime += up.Days + " jour, ";
            else if (up.Days > 0) upTime += up.Days + " jours, ";
            if (up.Hours == 1) upTime += up.Hours + " heure, ";
            else if (up.Days > 0 || up.Hours > 0) upTime += up.Hours + " heures, ";
            if (up.Minutes == 1) upTime += up.Minutes + " minute et ";
            else if (up.Hours > 0 || up.Days > 0 || up.Minutes > 0) upTime += up.Minutes + " minutes et ";
            if (up.Seconds == 1) upTime += up.Seconds + " seconde";
            else upTime += up.Seconds + " secondes";
            Player.SendMessage(p, upTime);
            Player.SendMessage(p, "> > s'est connecte &a" + playerDb.Rows[0]["totalLogin"] + Server.DefaultColor + " fois, et a ete kick &c" + playerDb.Rows[0]["totalKicked"] + Server.DefaultColor + "fois.");
            Player.SendMessage(p, "> > " + Awards.awardAmount(message) + " trophes");
            bool skip = false;
            if (p != null) if (p.group.Permission <= LevelPermission.Builder) skip = true;

            if (!skip)
            {
                if (Server.bannedIP.Contains(playerDb.Rows[0]["IP"].ToString()))
                    playerDb.Rows[0]["IP"] = "&8" + playerDb.Rows[0]["IP"] + ", est banni";
                Player.SendMessage(p, "> > a l'IP " + playerDb.Rows[0]["IP"]);
                if (Server.useWhitelist)
                {
                    if (Server.whiteList.Contains(message.ToLower()))
                    {
                        Player.SendMessage(p, "> > Ce joueur est dans la &fliste blanche");
                    }
                }
                if (Server.devs.Contains(message.ToLower()) || Server.devs.Contains("[mclawl]" + message.ToLower()))
                {
                    Player.SendMessage(p, Server.DefaultColor + "> > Ce joueur est un &9Developper");
                }
            }
            playerDb.Dispose();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/whowas <name> - Donne des informations sur un joueur non connecte.");
        }
    }
}