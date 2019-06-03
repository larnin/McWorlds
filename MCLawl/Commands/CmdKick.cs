using System;

namespace MCWorlds
{
    public class CmdKick : Command
    {
        public override string name { get { return "kick"; } }
        public override string shortcut { get { return "k"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdKick() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur."); return; }
            if (message.Split(' ').Length > 1)
                message = message.Substring(message.IndexOf(' ') + 1);
            else
                if (p == null) message = "Vous avez ete kick par un joueur sur IRC."; else message = "Vous avez ete kick par " + p.Name() + "!";

            if (p != null)
                if (who == p)
                {
                    Player.SendMessage(p, "Vous ne pouvez pas vous kick vous meme!");
                    return;
                }
                else if (who.group.Permission >= p.group.Permission && p != null) 
                { 
                    Player.GlobalChat(p, p.color + p.Name() + Server.DefaultColor + " a essaye de kick " + who.color + who.Name() + " mais une erreur est arrive.", false); 
                    return; 
                }

            who.Kick(message);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/kick [joueur] <message> - Kick un joueur.");
        }
    }
}