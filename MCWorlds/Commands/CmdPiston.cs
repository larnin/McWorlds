
using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    class CmdPiston : Command
    {
        public override string name { get { return "piston"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPiston() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Vous ne pouvez pas utiliser ceci depuis la console ou l'irc"); return;}
            if (message != "" && message != "stick") { Help(p); return; }

            if (!p.canBuild) { Player.SendMessage(p, "Vous ne pouvez pas construire sur cette map"); }

            p.posePiston = !p.posePiston;

            if (!p.posePiston) { Player.SendMessage(p, "Vous ne posez plus de pistons"); p.ClearBlockchange(); return; }
            else
            {
                Player.SendMessage(p, "Vous posez maintenant des pistons");
                Player.SendMessage(p, "Equipez vous des blocs jaune et vert clair !");
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/piston - permet de poser des pistons complet.");
            Player.SendMessage(p, "Placez un bloc jaune pour un piston normal");
            Player.SendMessage(p, "Placez un bloc vert clair pour un piston collant");
            Player.SendMessage(p, "Le piston collant ramenne un bloc quand il se referme");
            Player.SendMessage(p, "&cSysteme non termine !");
        }
    }
}
