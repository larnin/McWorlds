using System;

namespace MCWorlds
{
    public class CmdBan : Command
    {
        public override string name { get { return "ban"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBan() { }

        public override void Use(Player p, string message)
        {
            try
            {
                if (message == "") { Help(p); return; }

                bool stealth = false; bool totalBan = false;
                if (message[0] == '#')
                {
                    message = message.Remove(0, 1).Trim();
                    stealth = true;
                    Server.s.Log("Stealth Ban Attempted");
                }
                else if (message[0] == '@')
                {
                    totalBan = true;
                    message = message.Remove(0, 1).Trim();
                }

                Player who = Player.Find(message);

                if (who == null)
                {
                    if (!Player.ValidName(message))
                    {
                        Player.SendMessage(p, "Nom invalide \"" + message + "\".");
                        return;
                    }

                    Group foundGroup = Group.findPlayerGroup(message);

                    if (foundGroup.Permission >= LevelPermission.Operator)
                    {
                        Player.SendMessage(p, "Impossible de bannir un " + foundGroup.name + "!");
                        return;
                    }
                    if (foundGroup.Permission == LevelPermission.Banned)
                    {
                        Player.SendMessage(p, message + " est deja banni.");
                        return;
                    }

                    foundGroup.playerList.Remove(message);
                    foundGroup.playerList.Save();

                    Player.GlobalMessage(message + " &f(offline)" + Server.DefaultColor + " est maintenant &8banni" + Server.DefaultColor + "!");
                    Group.findPerm(LevelPermission.Banned).playerList.Add(message);
                }
                else
                {
                    if (!Player.ValidName(who.name))
                    {
                        Player.SendMessage(p, "nom invalide \"" + who.name + "\".");
                        return;
                    }

                    if (who.group.Permission >= LevelPermission.Operator)
                    {
                        Player.SendMessage(p, "Impossible de bannir un " + who.group.name + "!");
                        return;
                    }
                    if (who.group.Permission == LevelPermission.Banned)
                    {
                        Player.SendMessage(p, message + " est deja banni.");
                        return;
                    }

                    who.group.playerList.Remove(message);
                    who.group.playerList.Save();

                    if (stealth) Player.GlobalMessageOps(who.color + who.name + Server.DefaultColor + "  est maintenant &8banni discretement" + Server.DefaultColor + "!");
                    else Player.GlobalChat(who, who.color + who.name + Server.DefaultColor + " est maintenant &8banni" + Server.DefaultColor + "!", false);

                    who.group = Group.findPerm(LevelPermission.Banned);
                    who.color = who.group.color;
                    Player.GlobalDie(who, false);
                    Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                    Group.findPerm(LevelPermission.Banned).playerList.Add(who.name);
                }
                Group.findPerm(LevelPermission.Banned).playerList.Save();

                IRCBot.Say(message + " was banned.");
                Server.s.Log("BANNED: " + message.ToLower());

                if (totalBan == true)
                {
                    Command.all.Find("undo").Use(p, message + " 0");
                    Command.all.Find("banip").Use(p, "@ " + message);
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/ban <joueur> - Banni un joueur.");
            Player.SendMessage(p, "Ajouter # devant le nom pour un bannissement discret.");
            Player.SendMessage(p, "Ajouter @ devant le nom pour un bannissement total.");
        }
    }
}