
using System;
using System.Data;
using System.IO;

namespace MCWorlds
{
    class CmdPCount : Command
    {
        public override string name { get { return "pcount"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdPCount() { }

        public override void Use(Player p, string message)
        {
            int bancount = Group.findPerm(LevelPermission.Banned).playerList.All().Count;

            DataTable count = MySQL.fillData("SELECT COUNT(id) FROM players");
            Player.SendMessage(p, count.Rows[0]["COUNT(id)"] + " joueurs ont visite ce serveur.");
            Player.SendMessage(p, "Dont " + bancount + " qui sont banni.");
            count.Dispose();

            int playerCount = 0;
            int hiddenCount = 0;
           
            foreach (Player pl in Player.players)
            {
                if (!pl.hidden)
                { playerCount++; }
                else { hiddenCount++; } 
            }
            if (p != null)
            {
                if (playerCount == 1)
                {
                    if (hiddenCount == 0 || p.group.Permission < Server.opchatperm)
                    {
                        Player.SendMessage(p, "Il y a 1 joueur en ligne.");
                    }
                    else
                    {
                        Player.SendMessage(p, "Il y a 1 joueur en ligne (invisible : " + hiddenCount + ").");
                    }
                }
                else
                {
                    if (hiddenCount == 0 || p.group.Permission < Server.opchatperm)
                    {
                        Player.SendMessage(p, "Il y a " + playerCount + " joueurs en ligne.");
                    }
                    else
                    {
                        Player.SendMessage(p, "Il y a " + playerCount + " joueurs en ligne (invisibles : " + hiddenCount + ").");
                    }
                }
            }
            else
            {
                if (hiddenCount == 0)
                {
                    Player.SendMessage(p, "Il y a " + playerCount + " joueur(s) en ligne.");
                }
                else
                {
                    Player.SendMessage(p, "Il y a " + playerCount + " joueur(s) en ligne (invisibles : " + hiddenCount + ").");
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pcount - Donne le nombre de joueurs connecter et le nombre de joueurs qui sont passe sur le serveur.");
        }
    }
}
