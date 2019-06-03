using System;

namespace MCWorlds
{
    public class CmdInfo : Command
    {
        public override string name { get { return "info"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdInfo() { }

        public override void Use(Player p, string message)
        {
            if (message != "") 
            { 
                Help(p); 
            }
            else
            {
                Player.SendMessage(p, "Ce serveur fonctionne avec &bMCWorlds V 1.2.8c" + Server.DefaultColor + ", developpe par nico69 et base sur un serveur &bMCLawl.");
                Player.SendMessage(p, "Ce serveur n'est pas disponible en telechargement" );

                TimeSpan up = DateTime.Now - Server.timeOnline;
                string upTime = "Temps en ligne: &b";
                if (up.Days == 1) upTime += up.Days + " jour, ";
                else if (up.Days > 0) upTime += up.Days + " jours, ";
                if (up.Hours == 1) upTime += up.Hours + " heure, ";
                else if (up.Days > 0 || up.Hours > 0) upTime += up.Hours + " heures, ";
                if (up.Minutes == 1) upTime += up.Minutes + " minute et ";
                else if (up.Hours > 0 || up.Days > 0 || up.Minutes > 0) upTime += up.Minutes + " minutes et ";
                if (up.Seconds == 1) upTime += up.Seconds + " seconde";
                else upTime += up.Seconds + " secondes";
                Player.SendMessage(p, upTime);

                        //Player.SendMessage(p, "Ce serveur a ouvert pour la 1ere fois le 9 septembre 2010");

                if (Server.updateTimer.Interval > 1000) Player.SendMessage(p, "Ce serveur est en mode &5Low Lag" + Server.DefaultColor + ".");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/info - Affiche des informations sur le serveur.");
        }
    }
}
