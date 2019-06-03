using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdChangeName : Command
    {
        public override string name { get { return "changename"; } }
        public override string shortcut { get { return "name"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdChangeName() { }

        public override void Use(Player p, string message)
        {
            if (message.IndexOf(' ') != -1) { Help(p); return; }

            if (message == "")
            {
                if (p.skin == "") { Player.SendMessage(p, "Vous avez deja votre pseudo normal"); return; }
                p.falseName = "";
                Player.SendMessage(p, "Retour au pseudo normal");
            }
            else
            {
                if (message.Length > 16) { Player.SendMessage(p, "Pseudo trop long, 16 caracteres maximum"); return; }
                p.falseName = message;
                Player.SendMessage(p, "Vous avez maintenant le pseudo " + message + " dans le chat");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/changename [nom] - Change votre pseudo dans le chat.");
            Player.SendMessage(p, "/changename - Pour reprendre votre pseudo normal");
        }
    }
}
