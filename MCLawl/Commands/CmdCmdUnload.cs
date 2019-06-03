using System;
using System.IO;

namespace MCWorlds
{
    class CmdCmdUnload : Command
    {
        public override string name { get { return "cmdunload"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdCmdUnload() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            if (Command.core.Contains(message.Split(' ')[0]))
            {
                Player.SendMessage(p, "/" + message.Split(' ')[0] + " est une commande de base, vous ne pouvez pas la decharger!");
                return;
            }
            Command foundCmd = Command.all.Find(message.Split(' ')[0]);
            if(foundCmd == null)
            {
                Player.SendMessage(p, message.Split(' ')[0] + " n'est pas une commande valide ou chargee.");
                return;
            }
            Command.all.Remove(foundCmd);
            GrpCommands.fillRanks();
            Player.SendMessage(p, "La commande a ete correctement decharge.");
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/cmdunload [commande] - Decharge une commande du serveur.");
            Player.SendMessage(p, "La commande doit etre en dll externe pour pouvoir etre decharge");
        }
    }
}
