
using System;
using System.IO;

namespace MCWorlds
{
    public class CmdWorld : Command
    {
        public override string name { get { return "world"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdWorld() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "" || message.Split(' ').Length > 1) { Help(p); return; }

            message = message.ToLower();

            if (!Directory.Exists("levels/" + message))
            { 
                Player.SendMessage(p, "Le monde demande n'existe pas");
                Command.all.Find("worlds").Use(p, "short " + message);
                return;
            }

            if ( !File.Exists("levels/" + message + "/spawn.lvl"))
            { Player.SendMessage(p, "Le spawn de ce monde est introuvable"); return; }

            if (p.level.world.ToLower() == message && p.level.name == "spawn")
            { Player.SendMessage(p, "Vous etes deja dans ce monde"); return; }

            Level foundLevel = Level.Find("spawn", message);

            if (p.level == foundLevel) { Player.SendMessage(p, "Vous etes deja dans ce monde."); return; }

            if (foundLevel == null)
            { Command.all.Find("load").Use(p, "spawn " + message ); }

            foundLevel = Level.Find("spawn", message);

            if (foundLevel == null)
            { Player.SendMessage(p, "Impossible de charger la map"); return; }

            Command.all.Find("goto").Use(p, "spawn " + message);

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/world [monde] - Permet de partir vers le spawn d'un autre monde");
        }
    }
}