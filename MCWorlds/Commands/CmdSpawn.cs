using System;

namespace MCWorlds
{
    public class CmdSpawn : Command
    {
        public override string name { get { return "spawn"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdSpawn() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "") { Help(p); return; }
            ushort x = (ushort)((0.5 + p.level.spawnx) * 32);
            ushort y = (ushort)((1 + p.level.spawny) * 32);
            ushort z = (ushort)((0.5 + p.level.spawnz) * 32);
            unchecked
            {
                p.SendPos((byte)-1, x, y, z,
                            p.level.rotx,
                            p.level.roty);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/spawn - Vous renvois au point de spawn.");
        }
    }
}
