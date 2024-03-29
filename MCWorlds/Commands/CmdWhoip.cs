using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace MCWorlds
{
    public class CmdWhoip : Command
    {
        public override string name { get { return "whoip"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdWhoip() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            if (message.IndexOf("'") != -1) { Player.SendMessage(p, "Impossible d'analyser la demande."); return; }

            DataTable playerDb = MySQL.fillData("SELECT Name FROM Players WHERE IP='" + message + "'");

            if (playerDb.Rows.Count == 0) { Player.SendMessage(p, "Impossible de trouver un joueur avec cette ip."); return; }

            string playerNames = "Les joueur qui ont cette IP: ";

            for (int i = 0; i < playerDb.Rows.Count; i++)
            {
                playerNames += playerDb.Rows[i]["Name"] + ", ";
            }
            playerNames = playerNames.Remove(playerNames.Length - 2);

            Player.SendMessage(p, playerNames);
            playerDb.Dispose();
        }
        public override void Help(Player p, string message = "")
        {
            p.SendMessage("/whoip [ip] - Affiche les joueur qui on l'adresse <ip>.");
        }
    }
}