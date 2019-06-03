
namespace MCWorlds
{
    using System;

    public class Cmdkillinv : Command
    {
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool museumUsable { get { return true; } }
        public override string name { get { return "killinv"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public Cmdkillinv() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "")
            {
                Help(p);
            }
            else
            {
                Player player;
                string str;
                int num = 0;
                if (message.IndexOf(' ') == -1)
                {
                    player = Player.Find(message);
                    str = " a ete tue par " + p.color + p.Name();
                }
                else
                {
                    player = Player.Find(message.Split(new char[] { ' ' })[0]);
                    message = message.Substring(message.IndexOf(' ') + 1);
                    if (message.IndexOf(' ') == -1)
                    {
                        if (message.ToLower() == "explode")
                        {
                            str = " a ete explose par " + p.color + p.Name();
                            num = 1;
                        }
                        else
                        {
                            str = " " + message;
                        }
                    }
                    else
                    {
                        if (message.Split(new char[] { ' ' })[0].ToLower() == "explode")
                        {
                            num = 1;
                            message = message.Substring(message.IndexOf(' ') + 1);
                        }
                        str = " " + message;
                    }
                }
                if (player == null)
                {
                    p.HandleDeath(1, " S'est tue lui meme dans sa confusion", false);
                    Player.SendMessage(p, "Joueur introuvable");
                }
                else if (player.group.Permission > p.group.Permission)
                {
                    p.HandleDeath(1, " a ete tue par " + player.color + player.Name(), false);
                    Player.SendMessage(p, "Impossible de tuer un joueur de rang superieur");
                }
                else if (num == 1)
                {
                    if (player.invincible)
                    {
                        player.invincible = false;
                    }
                    player.HandleDeath(1, str, true);
                }
                else
                {
                    if (player.invincible)
                    {
                        player.invincible = false;
                    }
                    player.HandleDeath(1, str, false);
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/kill [pseudo] <message> - Tue un joueur invincible");
            Player.SendMessage(p, "/kill [pseudo] explode <message> - Tue un joueur en creant une explosion");
        }
    }
}

