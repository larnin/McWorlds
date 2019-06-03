using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdDelete : Command
    {
        public override string name { get { return "delete"; } }
        public override string shortcut { get { return "d"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdDelete() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }

            p.deleteMode = !p.deleteMode;

            string delName = "&4OFF";
            if (p.deleteMode) { delName = "&2ON"; }
            Player.SendMessage(p, "Mode delete: " + delName);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/delete - Supprime tous les blocs que vous cliquez");
            Player.SendMessage(p, "En incluant door_air, portals, mb's, etc");
        }
    }
}