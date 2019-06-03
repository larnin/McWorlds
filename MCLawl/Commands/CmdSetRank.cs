
using System;
namespace MCWorlds
{
    public class CmdSetRank : Command
    {
        public override string name { get { return "setrank"; } }
        public override string shortcut { get { return "rank"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSetRank() { }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length < 2) { Help(p); return; }
            Player who = Player.Find(message.Split(' ')[0]);
            Group newRank = Group.Find(message.Split(' ')[1]);
            string msgGave;

            if (message.Split(' ').Length > 2) msgGave = message.Substring(message.IndexOf(' ', message.IndexOf(' ') + 1)); else msgGave = "Felicitation !";
            if (newRank == null) { Player.SendMessage(p, "Impossible de trouver le rang specifie."); return; }

            Group bannedGroup = Group.findPerm(LevelPermission.Banned);
            if (who == null)
            {
                string foundName = message.Split(' ')[0];
                if (Group.findPlayerGroup(foundName) == bannedGroup || newRank == bannedGroup)
                {
                    Player.SendMessage(p, "Impossible de changer le rang vers \"" + bannedGroup.name + "\".");
                    return;
                }

                if (p != null)
                {
                    if (Group.findPlayerGroup(foundName).Permission >= p.group.Permission || newRank.Permission >= p.group.Permission)
                    {
                        Player.SendMessage(p, "Vous ne pouvez pas changer un rang egal ou superieur au votre"); return;
                    }
                }

                Group oldGroup = Group.findPlayerGroup(foundName);
                oldGroup.playerList.Remove(foundName);
                oldGroup.playerList.Save();

                newRank.playerList.Add(foundName);
                newRank.playerList.Save();

                Player.GlobalMessage("le rang de " + foundName + " &f(offline)" + Server.DefaultColor + " est change en " + newRank.color + newRank.name);
            }
            else if (who == p)
            {
                Player.SendMessage(p, "Impossible de changer votre propre rang."); return;
            }
            else
            {
                if (p != null)
                {
                    if (who.group == bannedGroup || newRank == bannedGroup)
                    {
                        Player.SendMessage(p, "Impossible de changer le rang d'un \"" + bannedGroup.name + "\".");
                        return;
                    }

                    if (who.group.Permission >= p.group.Permission || newRank.Permission >= p.group.Permission)
                    {
                        Player.SendMessage(p, "Impossible de changer le rang d'un joueur superieur au votre."); return;
                    }
                }

                who.group.playerList.Remove(who.name);
                who.group.playerList.Save();

                newRank.playerList.Add(who.name);
                newRank.playerList.Save();

                Player.GlobalChat(who, "Le rang de " + who.color + who.Name() + Server.DefaultColor + " a ete change en " + newRank.color + newRank.name, false);
                Player.GlobalChat(null, "&6" + msgGave, false);
                who.group = newRank;
                who.color = who.group.color;
                Player.GlobalDie(who, false);
                who.SendMessage("Vous avez ete promus " + newRank.color + newRank.name + Server.DefaultColor + ", tapez &5/help" + Server.DefaultColor + " pour connaitre vos nouvelles commandes.");
                Player.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                who.SetPrefix();

                who.rang = "";
                if (who.vip) { who.rang = "VIP "; }
                who.rang += who.group.statut + " ";
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/setrank [joueur] [rang] <yay> - Change le rang du joueur.");
            Player.SendMessage(p, "Les rangs valides sont: " + Group.concatList(true, true));
            Player.SendMessage(p, "<yay> est le message de felicitation!");
        }
    }
}
