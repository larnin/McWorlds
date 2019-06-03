using System;

namespace MCWorlds
{
    public class CmdMe : Command
    {
        public override string name { get { return "me"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdMe() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Player.SendMessage(p, "Toi"); return; }

            if (p.muted) { Player.SendMessage(p, "Vous etes actuellement en sourdine, vous ne pouvez pas utiliser cette commande."); return; }
            if (Server.chatmod && !p.voice) { Player.SendMessage(p, "La moderation du tchat est active. Vous ne pouvez pas vous exprime."); return; }

            if (Server.worldChat)
            {
                Player.GlobalChat(p, p.color + "*" + p.Name() + " " + message, false);
            }
            else
            {
                Player.GlobalChatLevel(p, p.color + "*" + p.Name() + " " + message, false);
            }
            IRCBot.Say("*" + p.name + " " + message);


        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "Vous voulez vraiment de l'aide !? Vous etes coince dans un puit ?!");
        }
    }
}