
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdTempBan : Command
    {
        public override string name { get { return "tempban"; } }
        public override string shortcut { get { return "tb"; } }
        public override string type { get { return "moderation"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdTempBan() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            if (message.IndexOf(' ') == -1) message = message + " 30";

            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur"); return; }
            if (who.group.Permission >= p.group.Permission) { Player.SendMessage(p, "Impossible de bannir un joueur qui a un rang superieur au votre"); return; }

            int minutes;
            try
            {
                minutes = int.Parse(message.Split(' ')[1]);
            } catch { Player.SendMessage(p, "Temps invalide"); return; }
            if (minutes > 60) { Player.SendMessage(p, "Impossible de bannir plus d'une heure"); return; }
            if (minutes < 1) { Player.SendMessage(p, "Impossible de bannir moins d'une minute"); return; }
            
            Server.TempBan tBan;
            tBan.name = who.name;
            tBan.allowedJoin = DateTime.Now.AddMinutes(minutes);
            Server.tempBans.Add(tBan);
            who.Kick("Banni pour " + minutes + " minutes!");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/tempban [joueur] - banni un joueur pour 30 minutes");
            Player.SendMessage(p, "/tempban [joueur] [minutes] - banni un joueur temporairement ");
            Player.SendMessage(p, "Le temps maximum est de 60 min");
            Player.SendMessage(p, "La liste des ban temporaire est remis a zero au demarrage du serveur");
        }
    }
}