
using System;

namespace MCWorlds
{
    public class CmdUnban : Command
    {
        public override string name { get { return "unban"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdUnban() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            bool totalUnban = false;
            if (message[0] == '@')
            {
                totalUnban = true;
                message = message.Remove(0, 1).Trim();
            }

            Player who = Player.Find(message);

            if (who == null)
            {
                if (Group.findPlayerGroup(message) != Group.findPerm(LevelPermission.Banned))
                {
                    foreach (Server.TempBan tban in Server.tempBans)
                    {
                        if (tban.name.ToLower() == message.ToLower())
                        {
                            Server.tempBans.Remove(tban);
                            Player.GlobalMessage("Le bannissement temporaire de " + message + " a ete arette.");
                            return;
                        }
                    }
                    Player.SendMessage(p, "Le joueur n'est pas banni.");
                    return;
                }
                Player.GlobalMessage(message + " &8(banni)" + Server.DefaultColor + " est maintenant " + Group.standard.color + Group.standard.name + Server.DefaultColor + "!");
                Group.findPerm(LevelPermission.Banned).playerList.Remove(message);
            }
            else
            {
                if (Group.findPlayerGroup(message) != Group.findPerm(LevelPermission.Banned))
                {
                    foreach (Server.TempBan tban in Server.tempBans)
                    {
                        if (tban.name == who.name)
                        {
                            Server.tempBans.Remove(tban);
                            Player.GlobalMessage("Le bannissement temporaire de " + who.color + who.prefix + who.Name() + Server.DefaultColor + " a ete arette.");
                            return;
                        }
                    }
                    Player.SendMessage(p, "Le joueur n'est pas banni.");
                    return;
                }
                Player.GlobalChat(who, who.color + who.prefix + who.Name() + Server.DefaultColor + " est maintenant " + Group.standard.color + Group.standard.name + Server.DefaultColor + "!", false);
                who.group = Group.standard; who.color = who.group.color; Player.GlobalDie(who, false);
                Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                Group.findPerm(LevelPermission.Banned).playerList.Remove(message);
            }

            Group.findPerm(LevelPermission.Banned).playerList.Save(); 
            if (totalUnban)
            {
                Command.all.Find("unbanip").Use(p, "@" + message);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/unban [joueur] - Deban un joueur. Inclue aussi les bannissements temporaires.");
        }
    }
}