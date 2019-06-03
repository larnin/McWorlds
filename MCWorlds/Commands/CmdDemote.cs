using System;

namespace MCWorlds
{
    public class CmdDemote : Command
    {
        public override string name { get { return "demote"; } }
        public override string shortcut { get { return "de"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdDemote() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') != -1) { Help(p); return; }
            Player who = Player.Find(message);
            string foundName;
            Group foundGroup;
            if (who == null)
            {
                foundName = message;
                foundGroup = Group.findPlayerGroup(message);
            }
            else
            {
                foundName = who.name;
                foundGroup = who.group;
            }

            Group nextGroup = null; bool nextOne = false;
            for (int i = Group.GroupList.Count - 1; i >= 0; i--)
            {
                Group grp = Group.GroupList[i];
                if (nextOne)
                {
                    if (grp.Permission <= LevelPermission.Banned) break;
                    nextGroup = grp;
                    break;
                }
                if (grp == foundGroup)
                    nextOne = true;
            }

            if (nextGroup != null)
                Command.all.Find("setrank").Use(p, foundName + " " + nextGroup.name);
            else
                Player.SendMessage(p, "Il n'existe pas de rang inferieur");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/demote [nom] - Passe le joueur au rang inferieur");
        }
    }
}