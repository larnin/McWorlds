using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdChangeSkin : Command
    {
        public override string name { get { return "changeskin"; } }
        public override string shortcut { get { return "skin"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdChangeSkin() { }

        public override void Use(Player p, string message)
        {
            if (message.IndexOf(' ') != -1) { Help(p); return; }

            if (message == "")
            {
                if (p.skin == "") { Player.SendMessage(p, "Vous avez deja votre skin normal"); return; }
                p.skin = "";
                Player.SendMessage(p, "Retour au skin normal");
            }
            else
            {
                if (!Player.ValidName(message)) { Player.SendMessage(p, "Nom '" + message + "' invalide"); return; }
                if (message.Length > 32) { Player.SendMessage(p, "Pseudo trop long, 32 caracteres maximum"); return; }
                p.skin = message;
                Player.SendMessage(p, "Vous avez maintenant le skin de " + message);
            }
            
            Player.GlobalDie(p, false);
            Player.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/changeskin [nom] - Change votre skin.");
            Player.SendMessage(p, "/changeskin - Remet votre skin par defaut");
        }
    }
}
