using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdTeam : Command
    {
        public override string name { get { return "team"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public CmdTeam() { }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            BaseGame game = Server.allGames.Find(g => g.lvl == p.level);
            

            if (game == null) { Player.SendMessage(p, "Il n'y a pas de jeu en cours sur cette map"); return; }

            if (game.typeGame != "ctf")
            {
                Player.SendMessage(p, "Le CTF n'est pas active sur cette map.");
                return;
            }
            CTFGame2 ctfGame = (CTFGame2)game;
            if (message.Split(' ')[0].ToLower() == "set")
            {
                if (game.gameOn) { Player.SendMessage(p, "Vous le pouvez pas rejoindre modifier les equipes en cours de partie"); }

                if (p.group.Permission >= LevelPermission.Operator)
                {
                    string name = message.Split(' ')[1].ToLower();
                    string team = message.Split(' ')[2].ToLower();
                    if (team == "none")
                    {
                        Player pl = Player.Find(name);
                        if (pl == null || pl.level != p.level) { Player.SendMessage(p, "Ce joueur n'est pas sur la meme map que vous."); }
                        if (pl.team == null) { Player.SendMessage(p, "Ce joueur n'est pas dans une equipe."); }
                        pl.team.RemoveMember(pl);
                        return;
                    }
                    string color = c.Parse(team);
                    if (color == "") { Player.SendMessage(p, "Couleur de d'equipe invalide."); return; }
                    Player who = Player.Find(name);
                    if (who == null || who.level != p.level) { Player.SendMessage(p, "Ce joueur n'est pas sur la meme map que vous."); }
                    char teamCol = (char)color[1];
                    if (ctfGame.teams.Find(team1 => team1.color == teamCol) == null) { Player.SendMessage(p, "Ce joueur n'est pas dans une equipe."); return; }
                    Team workTeam = ctfGame.teams.Find(team1 => team1.color == teamCol);
                    if (who.team != null) { who.team.RemoveMember(who);}
                    workTeam.AddMember(who);
                }
            }
            if (message.Split(' ')[0].ToLower() == "join")
            {
                if (game.gameOn) { Player.SendMessage(p, "Vous ne pouvez pas rejoindre le ctf en cours de partie"); }
                string color = c.Parse(message.Split(' ')[1]);
                if (color == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                char teamCol = (char)color[1];
                if (ctfGame.teams.Find(team => team.color == teamCol) == null) { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                Team workTeam = ctfGame.teams.Find(team => team.color == teamCol);
                if (p.team != null) { p.team.RemoveMember(p); }
                workTeam.AddMember(p);
            }
            else if (message.Split(' ')[0].ToLower() == "leave")
            {
                if (p.team != null)
                {
                    p.team.RemoveMember(p);
                }
                else
                {
                    Player.SendMessage(p, "Vous n'etent pas dans une equipe.");
                    return;
                }
            }
            else if (message.Split(' ')[0].ToLower() == "chat")
            {
                if (p.team == null) { Player.SendMessage(p, "Vous devez etre dans une equipe pour pouvoir utiliser le tchat d'equipe."); return; }
                p.teamchat = !p.teamchat;
                if (p.teamchat)
                {
                    Player.SendMessage(p, "Le tchat d'equipe est active.");
                    return;
                }
                else
                {
                    Player.SendMessage(p, "Le tchat d'equipe est desactive.");
                    return;
                }

            }
            else if (message.Split(' ')[0].ToLower() == "scores")
            {
                foreach (Team t in ctfGame.teams)
                {
                    Player.SendMessage(p, t.teamstring + " a " + t.points + " point(s).");
                }
            }

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/team join [couleur] - Rejoint l'equipe de couleur [couleur].");
            Player.SendMessage(p, "/team leave - Quitte l'equipe ou vous etes.");
            Player.SendMessage(p, "/team set [nom] [couleur] - Change le joueur [nom] d'equipe. (MODO+)");
            Player.SendMessage(p, "/team set [nom] - Enleve le joueur de l'equipe. (MODO+)");
            Player.SendMessage(p, "/team scores - Affiche les scores de toutes les equipes.");
        }
    }
}
