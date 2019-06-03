using System;

namespace MCWorlds
{
    public class CmdCmdSet : Command
    {
        public override string name { get { return "cmdset"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdCmdSet() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }

            string foundBlah = Command.all.FindShort(message.Split(' ')[0]);

            Command foundCmd;
            if (foundBlah == "") foundCmd = Command.all.Find(message.Split(' ')[0]);
            else foundCmd = Command.all.Find(foundBlah);

            if (foundCmd == null) { Player.SendMessage(p, "Ne trouve pas la commande entree"); return; }
            if (p != null && !p.group.CanExecute(foundCmd)) { Player.SendMessage(p, "Cette commande est pour un groupe superieur au votre."); return; }

            LevelPermission newPerm = Level.PermissionFromName(message.Split(' ')[1]);
            if (newPerm == LevelPermission.Null) { Player.SendMessage(p, "Ne trouve pas le rang entre"); return; }
            if (p != null && newPerm > p.group.Permission) { Player.SendMessage(p, "Vous ne pouvez pas donner un rang a la commande superieur au votre."); return; }

            GrpCommands.rankAllowance newCmd = GrpCommands.allowedCommands.Find(rA => rA.commandName == foundCmd.name);
            newCmd.lowestRank = newPerm;
            GrpCommands.allowedCommands[GrpCommands.allowedCommands.FindIndex(rA => rA.commandName == foundCmd.name)] = newCmd;

            GrpCommands.Save(GrpCommands.allowedCommands);
            GrpCommands.fillRanks();
            Player.GlobalMessage("La permition de &d" + foundCmd.name + Server.DefaultColor + " a change pour " + Level.PermissionToName(newPerm));
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/cmdset [cmd] [rang] - Change le rang d'utilisation de la commande");
            Player.SendMessage(p, "Seulement les commandes que vous pouvez utiliser sont modifiable");
            Player.SendMessage(p, "Rangs possible: " + Group.concatList());
        }
    }
}