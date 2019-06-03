using System;

namespace MCWorlds
{
    using System;
    public class CmdImpersonate : Command
    {
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool museumUsable { get { return true; } }
        public override string name { get { return "impersonate"; } }
        public override string shortcut { get { return "imp"; } }
        public override string type { get { return "other"; } }
        public CmdImpersonate() { }

        public void SendIt(Player p, string message, Player player)
        {
            if (message.Split(' ').Length > 1)
            {
                if (player != null)
                {
                    message = message.Substring(message.IndexOf(' ') + 1);
                    Player.GlobalMessage(player.color + player.voicestring + player.color + player.rang + player.prefix + player.name + ": &f" + message);
                }
                else
                {
                    string playerName = message.Split(' ')[0];
                    message = message.Substring(message.IndexOf(' ') + 1);
                    Player.GlobalMessage(playerName + ": &f" + message);
                }
            }
            else { Player.SendMessage(p, "Pas de message ecrit"); }
        }
        public override void Use(Player p, string message)
        {
            if ((message == "")) { Help(p); }
            else
            {
                Player player = Player.Find(message.Split(' ')[0]);
                if (player != null)
                {
                    if (p == null) { SendIt(p, message, player); }
                    else
                    {
                        if (player == p) { SendIt(p, message, player); }
                        else
                        {
                            if (p.group.Permission > player.group.Permission) { SendIt(p, message, player); }
                            else { Player.SendMessage(p, "Vous ne pouvez pas impersonate un joueur de rang egal ou superieur au votre."); }
                        }
                    }
                }
                else
                {
                    if (p != null)
                    {
                        if (p.group.Permission >= LevelPermission.Admin)
                        {
                            if (Group.findPlayerGroup(message.Split(' ')[0]).Permission < p.group.Permission) { SendIt(p, message, null); }
                            else { Player.SendMessage(p, "Vous ne pouvez pas impersonate un joueur de rang egal ou superieur au votre."); }
                        }
                        else { Player.SendMessage(p, "Vous ne pouvez pas impersonate un joueurs horsligne"); }
                    }
                    else { SendIt(p, message, null); }
                }
            }
        }

        public override void Help(Player p, string message = "")
        { Player.SendMessage(p, "/impersonate [joueur] [message] - Envois un message comme si c'est le joueur qui parle"); }
    }
}
