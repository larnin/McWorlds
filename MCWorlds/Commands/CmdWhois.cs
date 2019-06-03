using System;
using System.IO;

namespace MCWorlds
{
    public class CmdWhois : Command
    {
        public override string name { get { return "whois"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdWhois() { }

        public override void Use(Player p, string message)
        {
            Player who = null;
            if (message == "") { who = p; message = p.name; } else { who = Player.Find(message); }
            if (who != null && !who.hidden)
            {
                Player.SendMessage(p, who.color + who.name + Server.DefaultColor + " est sur &b" + who.level.name + " (" + who.level.world + ")");
                if (p.group.Permission >= LevelPermission.Operator && who.falseName != "") { Player.SendMessage(p, who.color + who.prefix + who.name + Server.DefaultColor + " : Connu sous le pseudo " + who.falseName); }
                else { Player.SendMessage(p, who.color + who.prefix + who.name + Server.DefaultColor + " :"); }
                Player.SendMessage(p, "> > Le rang de " + who.group.color + who.group.name);
                Player.SendMessage(p, "> > Nombre de maps : " + who.nbMaps + " / " + who.nbMapsMax );
                try
                {
                    if ( !(Group.Find("Nobody").commands.Contains("pay") || Group.Find("Nobody").commands.Contains("give") || Group.Find("Nobody").commands.Contains("take") ) )
                    { Player.SendMessage(p, "> > &a" + who.money + Server.DefaultColor + " " + Server.moneys); }
                }
                catch { }
                Player.SendMessage(p, "> > &cest mort &a" + who.overallDeath + Server.DefaultColor + " fois");
                Player.SendMessage(p, "> > &bmodifie &a" + who.overallBlocks + Server.DefaultColor + " blocs, &a" + who.loginBlocks + Server.DefaultColor + " a cette connection.");
                string storedTime = Convert.ToDateTime(DateTime.Now.Subtract(who.timeLogged).ToString()).ToString("HH:mm:ss");
                Player.SendMessage(p, "> > est connecte depuis &a" + storedTime);
                TimeSpan up = DateTime.Now - who.totalTimePlayed;
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
                Player.SendMessage(p, "> > premiere connection le &a" + who.firstLogin.ToString("yyyy-MM-dd") + " a " + who.firstLogin.ToString("HH:mm:ss"));
                Player.SendMessage(p, "> > s'est connecte &a" + who.totalLogins + Server.DefaultColor + " fois, et a ete kick &c" + who.totalKicked + Server.DefaultColor + " fois.");
                Player.SendMessage(p, "> > " + Awards.awardAmount(who.name) + " trophe.");
                bool skip = false;
                if (p != null) if (p.group.Permission <= LevelPermission.Operator) skip = true;
                if (!skip)
                    {
                        string givenIP;
                        if (Server.bannedIP.Contains(who.ip)) givenIP = "&8" + who.ip + ", est banni"; 
                        else givenIP = who.ip;
                        Player.SendMessage(p, "> > a l'IP " + givenIP);
                        if (Server.useWhitelist)
                        {
                            if (Server.whiteList.Contains(who.name))
                            {
                                Player.SendMessage(p, "> > Est dans la &fliste blanche");
                            }
                        }
                        if (Server.devs.Contains(who.name.ToLower()) || Server.devs.Contains("[mclawl]" + who.name.ToLower()))
                        {
                            Player.SendMessage(p, Server.DefaultColor + "> > est un &9Developpeur");
                        }
                    }
            }
            else { Player.SendMessage(p, "\"" + message + "\" n'est pas en ligne ! Utilisation de /whowas."); Command.all.Find("whowas").Use(p, message); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/whois <joueur> - Donne des informations sur le joueur.");
        }
    }
}