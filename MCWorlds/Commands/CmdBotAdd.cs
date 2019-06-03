using System;
using System.IO;

namespace MCWorlds
{
    public class CmdBotAdd : Command
    {
        public override string name { get { return "botadd"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBotAdd() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }
            if (!PlayerBot.ValidName(message)) { Player.SendMessage(p, "Nom de bot " + message + " non valide !"); return; }
            if (message.Length > 32) { Player.SendMessage(p, "Nom de bot trop long"); return; }
            PlayerBot.playerbots.Add(new PlayerBot(message, p.level, p.level.world, p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0));            
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/botadd [nom] - Ajoute un nouveau bot a votre position.");
        }
    }
}