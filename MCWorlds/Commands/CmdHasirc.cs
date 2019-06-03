using System;

namespace MCWorlds
{
    public class CmdHasirc : Command
    {
        public override string name { get { return "hasirc"; } }
        public override string shortcut { get { return "irc"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdHasirc() { }

        public override void Use(Player p, string message)
        {
            if (message != "")
            {
                Help(p);
                return;
            }
            else
            {
                string hasirc;
                string ircdetails = "";
                if (Server.irc)
                {
                    hasirc = "&aActif" + Server.DefaultColor + ".";
                    ircdetails = Server.ircServer + " > " + Server.ircChannel;
                }
                else
                {
                    hasirc = "&cDesactive" + Server.DefaultColor + ".";
                }
                Player.SendMessage(p, "l'IRC est " + hasirc);
                if (ircdetails != "")
                {
                    Player.SendMessage(p, "Adresse: " + ircdetails);
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/hasirc - Affiche si l'IRC est actif ou non");
            Player.SendMessage(p, "Si l'IRC est active , son adresse est affiche.");
        }
    }
}
