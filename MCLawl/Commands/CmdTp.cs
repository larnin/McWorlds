using System;
using System.IO;

namespace MCWorlds
{
    public class CmdTp : Command
    {
        public override string name { get { return "tp"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdTp() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "")
            {
                Command.all.Find("spawn");
                return;
            }
            Player who = Player.Find(message);
            if (who == null || (who.hidden && p.group.Permission < LevelPermission.Admin)) { Player.SendMessage(p, "Il n'y a pas de joueur \"" + message + "\"!"); return; }
            if (p.level != who.level)
            {
                if(who.level.name.Contains("cMuseum"))
                {
                    Player.SendMessage(p, "Le joueur \"" + message + "\" est dans un musee!");
                    return;
                }
                else
                {
                    Command.all.Find("goto").Use(p, who.level.name + " " + who.level.world);
                }
            }
            if (p.level == who.level)
            {
                if (who.Loading)
                {
                    Player.SendMessage(p, "Patientez, le joueur " + who.color + who.name + Server.DefaultColor + " est en cours de chargement ...");
                    while (who.Loading) { }
                }
                while (p.Loading) { }  //Wait for player to spawn in new map
                unchecked { p.SendPos((byte)-1, who.pos[0], who.pos[1], who.pos[2], who.rot[0], 0); }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/tp <joueur> - Vous teleporte vers un joueur.");
            Player.SendMessage(p, "Si <joueur> est vide, /spawn est utilise.");
        }
    }
}