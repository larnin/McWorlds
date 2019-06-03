
using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdGame : Command
    {
        public override string name { get { return "game"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeux"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdGame() { }

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                string games = "";
                foreach (string g in BaseGame.NameGames)
                { games += g + ", "; }
                if (games == "") { Player.SendMessage(p, "Il n'y a aucun jeu disponible"); return; }

                Player.SendMessage(p, "Liste des jeux disponible :");
                Player.SendMessage(p, games.Remove(games.Length - 2));
                return;
            }

            if (message == "liste" || message == "list")
            {
                string gameinfos = "";
                List<string> gamesStarted = new List<string>();
                List<string> gamesPlayed = new List<string>();

                foreach (BaseGame g in Server.allGames)
                {
                    if (g == null) { Server.s.Log("Attention pointeur nul dans les jeux !"); continue; }
                    gameinfos = g.name + " (type : " + g.typeGame.ToLower() +") - map : " + g.lvl.name + " - monde : " + g.lvl.world;

                    if (g.gameOn) { gamesPlayed.Add(gameinfos); }
                    else { gamesStarted.Add(gameinfos); }
                }

                if (gamesPlayed.Count == 0 && gamesStarted.Count == 0)
                { Player.SendMessage(p, "Il y a aucun jeu en cours"); return; }
                    
                if (gamesPlayed.Count != 0)
                {
                    Player.SendMessage(p, "Jeux en cours :");
                    foreach (string g in gamesPlayed)
                    { Player.SendMessage(p, "&c" + g); }
                }

                if (gamesStarted.Count != 0)
                {
                    Player.SendMessage(p, "Jeux joignable :");
                    foreach (string g in gamesPlayed)
                    { Player.SendMessage(p, "&2" + g); }
                }
                return;
            }

            if (message.Split(' ')[0] == "commandes")
            {
                string name = "";
                if (message.IndexOf(' ') != -1) { name = message.Split(' ')[1]; }

                if (name == "")
                {
                    if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

                    BaseGame game = Server.allGames.Find(g => g.lvl == p.level);
                    if (game == null) { Player.SendMessage(p, "Il n'y a pas de jeu sur votre map"); return; }

                    if (game.cmdAllow.Count == 0)
                    { Player.SendMessage(p, "Aucune commande n'est autorise dans ce jeu"); return; }

                    string cmds = "";
                    foreach (string s in game.cmdAllow) { cmds += s + ", "; }

                    Player.SendMessage(p, "Il y a " + game.cmdAllow.Count + " commandes autorise sur cette map");
                    Player.SendMessage(p, cmds.Remove(cmds.Length - 2));
                }
                else
                {
                    if (BaseGame.NameGames.Find(ng => ng.ToLower() == name.ToLower()) == null)
                    { Player.SendMessage(p, "Il n'existe pas de jeu nomme '" + name + "'"); return;}

                    if (!File.Exists("extra/games/" + name.ToLower() + "/default.cmds"))
                    { Player.SendMessage(p, "Par defaut, aucunes commande n'est autorise sur le jeu '" + name + "'"); return; }

                    string[] lignes = File.ReadAllLines("extra/games/" + name.ToLower() + "/default.cmds");

                    string cmds = "";
                    foreach (string l in lignes)
                    {
                        if (l == "") { continue; }
                        if (l[0] == '#') { continue; }

                        cmds += l + ", ";
                    }

                    if (cmds == "") { Player.SendMessage(p, "Par defaut, aucunes commande n'est autorise sur le jeu '" + name + "'"); return; }
                    else
                    {
                        Player.SendMessage(p, "Commandes autorise par defaut sur le jeu '" + name + "' :");
                        Player.SendMessage(p, cmds.Remove(cmds.Length - 2));
                    }
                }
                return;
            }

            if (message.IndexOf(' ') == -1) { Help(p); return; }

            string key = message.Split(' ')[0];

            if (key == "cheats")
            {
                BaseGame game = Server.allGames.Find(g => g.lvl == p.level);
                if (game == null) { Player.SendMessage(p, "Il n'y a pas de jeu en cours sur cette map"); return; }

                if (p != game.owner) { Player.SendMessage(p, "Vous n'etes pas le maitre du jeu"); return; }

                if (message.Split(' ')[1] == "on")
                {
                    game.cheat = true;
                    Player.GlobalMessageLevel(p.level, "Les cheats sont maintenant autorise");
                }
                else if (message.Split(' ')[1] == "off")
                {
                    game.cheat = false;
                    Player.GlobalMessageLevel(p.level, "Les cheats sont maintenant interdit");
                }
                else { Help(p); }

                return;
            }

            if (key == "help")
            {
                string name = message.Split(' ')[1];

                if (BaseGame.NameGames.Find(ng => ng.ToLower() == name.ToLower()) != null)
                {
                    Command cmd = Command.all.Find(name);
                    if (cmd != null) { cmd.Help(p); }
                    else { Player.SendMessage(p, "Il n'existe pas de commande de jeu nomme '" + name + "'"); }
                }
                else { Player.SendMessage(p, "Il n'existe pas de commande de jeu nomme '" + name + "'"); }

                return;
            }

            if (key == "joindre")
            {
                if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

                string name = message.Split(' ')[1];

                BaseGame game = Server.allGames.Find(g => g.name.ToLower() == name.ToLower());
                if (game == null)
                { Player.SendMessage(p, "Aucun jeu en cours se nome '" + name + "'"); return; }

                if (game.gameOn) { Player.SendMessage(p, "La partie est en cours, vous rejoignez en tant que spectateur"); }
                else
                {
                    Player.SendMessage(p, "Vous rejoignez la map du jeu '" + name + "'.");
                    Player.SendMessage(p, "Lisez /help " + game.typeGame.ToLower() + " pour avoir plus d'infos sur ce jeu");
                }
                Command.all.Find("goto").Use(p, game.lvl.name + " " + game.lvl.world);

                return;
            }

            if (key == "add" || key == "del")
            {
                if (message.Split(' ').Length == 3)
                {
                    if (p != null)
                    { if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Vous n'avez pas le rang pour faire ca !"); return; } }

                    string name = message.Split(' ')[1];
                    string cmdName = message.Split(' ')[2];

                    if (BaseGame.NameGames.Find(ng => ng.ToLower() == name.ToLower()) == null)
                    { Player.SendMessage(p, "Il n'existe pas de jeu nomme '" + name + "'"); return; }

                    if (Command.all.Find(cmdName) == null) { Player.SendMessage(p, "La commande '" + cmdName + "' n'existe pas"); return; }
                    
                    if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
                    if (!Directory.Exists("extra/games")) { Directory.CreateDirectory("extra/games"); }
                    if (!Directory.Exists("extra/games/" + name.ToLower())) { Directory.CreateDirectory("extra/games/" + name.ToLower()); }
                    
                    List<string> cmdAllow = new List<string>();
                    if (File.Exists("extra/games/" + name.ToLower() + "/default.cmds"))
                    {
                        string[] lignes = File.ReadAllLines("extra/games/" + name.ToLower() + "/default.cmds");

                        foreach (string l in lignes)
                        {
                            if (l == "") { continue; }
                            if (l[0] == '#') { continue; }
                            if (Command.all.Find(l) == null) { continue; }

                            if (cmdAllow.Find(cmd => cmd == l) == null)
                            { cmdAllow.Add(l); }
                        }
                    }

                    if (cmdAllow.Find(cmd => cmd == cmdName) == null)
                    {
                        if (key == "del") { Player.SendMessage(p, "La commande '" + cmdName + "' n'est pas autorise pour le jeu '" + name + "'"); return; }
                        cmdAllow.Add(cmdName);
                        Player.SendMessage(p, "Ajout de la commande '" + cmdName + "' au jeu '" + name + "'");
                    }
                    else
                    {
                        if (key == "add"){ Player.SendMessage(p, "La commande '" + cmdName + "' est deja autorise pour le jeu '" + name + "'"); return;}
                        cmdAllow.Remove(cmdName);
                        Player.SendMessage(p, "Supression de la commande '" + cmdName + "' au jeu '" + name + "'");
                    }

                    StreamWriter SW = new StreamWriter(File.Create("extra/games/" + name.ToLower() + "/default.cmds"));
                    foreach (string cmd in cmdAllow)
                    { SW.WriteLine(cmd); }

                    SW.Flush();
                    SW.Close();
                    SW.Dispose();
                }
                else if (message.Split(' ').Length == 2)
                {
                    if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

                    string cmdName = message.Split(' ')[1];
                    if (Command.all.Find(cmdName) == null) { Player.SendMessage(p, "La commande '" + cmdName + "' n'existe pas"); return; }

                    BaseGame game = Server.allGames.Find(g => g.lvl == p.level);
                    if (game == null) { Player.SendMessage(p, "Il n'y a pas de jeu sur votre map"); }

                    if (p.group.Permission < LevelPermission.Operator && game.owner != p)
                    { Player.SendMessage(p, "Vous n'avez pas les autorisation pour faire ca !"); return; }

                    if (game.cmdAllow.Find(cmd => cmd == cmdName) == null)
                    {
                        if (key == "del") { Player.SendMessage(p, "La commande '" + cmdName + "' n'est pas autorise"); return; }
                        game.cmdAllow.Add(cmdName);
                        Player.GlobalMessageLevel(p.level, "Ajout de la commande '" + cmdName + "' au jeu");
                    }
                    else
                    {
                        if (key == "add") { Player.SendMessage(p, "La commande '" + cmdName + "' est deja autorise"); return; }
                        game.cmdAllow.Remove(cmdName);
                        Player.GlobalMessageLevel(p.level, "Supression de la commande '" + cmdName + "' au jeu");
                    }
                }
                else { Help(p); }
                return;
            }
            Help(p);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/game - Liste tous les jeux disponible sur le serveur.");
            Player.SendMessage(p, "/game liste - Liste les parties en cours");
            Player.SendMessage(p, "/game joindre [nom] - Rejoindre la map d'une partie");
            Player.SendMessage(p, "/game help [jeu] - Affiche l'help du jeu");
            Player.SendMessage(p, "/game commandes [jeu] - Liste les commandes autorise pour le jeu");
            Player.SendMessage(p, "/game commandes - Liste les commandes autorise pour le jeu en cour");
            Player.SendMessage(p, "/game <add/del> [jeu] [cmd] - Ajoute/supprime une commande dans la liste des cmds autorise (MODO+)");
            Player.SendMessage(p, "/game <add/del> [cmd] - Ajoute/supprime une commande dans la partie en cours (OWNER game)");
            Player.SendMessage(p, "/game cheats [on/off] - active/desactive les cheats sur le jeu (OWNER game)");
        }
    }
}
