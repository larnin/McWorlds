using System;
using System.IO;

namespace MCWorlds
{
    public class CmdSetspawn : Command
    {
        public override string name { get { return "setspawn"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSetspawn() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.group.Permission < LevelPermission.Admin && p.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Vous ne pouvez pas changer de place le spawn de ce monde"); return; }

            if (message != "") { Help(p); return; }
            Player.SendMessage(p, "Le spawn a change de position.");
            p.level.spawnx = (ushort)(p.pos[0] / 32);
            p.level.spawny = (ushort)(p.pos[1] / 32);
            p.level.spawnz = (ushort)(p.pos[2] / 32);
            p.level.rotx = p.rot[0];
            p.level.roty = 0;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/setspawn - Change la position du spawn.");
        }
    }
}