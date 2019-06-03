using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdInfection : Command
    {
        public override string name { get { return "infection"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public CmdInfection() { }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }


            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            InfectionGame InfInfo = null;
            if (gameInfo != null) { if (gameInfo.typeGame == "infection") { InfInfo = (InfectionGame)gameInfo; } }

            string key = "";
            if (message == "") ;
            else if (message.IndexOf(' ') == -1)
            { key = message; message = ""; }
            else
            { key = message.Split(' ')[0]; message = message.Substring(message.IndexOf(' ') + 1); }

            switch (key.ToLower())
            {
                case "start":
                    if (!verif(gameInfo, p)) { return; }
                    if (InfInfo.gameOn)
                    { Player.SendMessage(p, "Le jeu est deja lance"); return; }
                    InfInfo.startGame(p);
                    break;
                case "stop":
                    if (!verif(gameInfo, p)) { return; }
                    if (!InfInfo.gameOn)
                    { Player.SendMessage(p, "Le jeu n'est pas lance"); return; }
                    InfInfo.gameOn = false;
                    InfInfo.stopGame(p);
                    break;
                case "info":
                    if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
                    if (gameInfo.typeGame.ToLower() != "infection") { Player.SendMessage(p, "L'infection n'est pas actif sur cette map"); return; }
                    InfInfo.GameInfo(p);
                    break;
                case "list":
                    BaseGame.listSave(p, "ctf");
                    break;
                case "save":
                    if (!verif(gameInfo, p)) { return; }
                    if (InfInfo.gameOn) { Player.SendMessage(p, "Impossible de sauvegarder les configurations quand l'infection est lance"); return; }
                    InfInfo.saveGame(p, message);
                    break;
                case "load":
                    if (!verif(gameInfo, p)) { return; }
                    if (InfInfo.gameOn) { Player.SendMessage(p, "Impossible de charger une configuration quand l'infection est lance"); return; }
                    InfInfo.loadGame(p, message);
                    break;
                case "blocs":
                case "blocks":
                    if (!verif(gameInfo, p)) { return; }
                    int nbBlocs = 0;
                    Int32.TryParse(message, out nbBlocs);
                    if (nbBlocs <= 0) { Player.SendMessage(p, "Vous devez choisir un nombre de blocs superieur a 0 !"); return; }
                    InfInfo.nbBlocsAllow = nbBlocs;
                    Player.SendMessage(p, "Le nombre de blocs posable a ete mis a " + nbBlocs);
                    break;
                case "time":
                case "temps":
                    if (!verif(gameInfo, p)) { return; }
                    int nbMins = 0;
                    Int32.TryParse(message, out nbMins);
                    if (nbMins <= 0) { Player.SendMessage(p, "Vous devez choisir un temps superieur a 0 !"); return; }
                    if (nbMins >= 30) { Player.SendMessage(p, "Vous devez choisir un temps de moins de 30 min !"); return; }
                    InfInfo.time = nbMins;
                    Player.SendMessage(p, "Le temps d'une partie est regle a " + nbMins + " minutes");
                    break;
                default:
                    if (gameInfo == null)
                    {
                        if (p.level.world != p.name.ToLower() && p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous ne pouvez pas lancer de jeu dans ce monde"); return; }

                        if (Server.allGames.Count > Server.maxGames)
                        {
                            Player.SendMessage(p, "Le maximum de jeux en simultanees est attein");
                            Player.SendMessage(p, "Attendez qu'une partie se finisse avant d'en lancer une autre");
                            return;
                        }
                        InfectionGame infection = new InfectionGame(p.level);
                        infection.owner = p;

                        if (key == "") { Player.SendMessage(p, "Vous devez donner un nom a la partie"); return; }
                        if (message != "") { Player.SendMessage(p, "Le nom de la partie doit etre fait d'un seul mot"); return; }
                        infection.name = key;

                        if (!BaseGame.addGame(infection))
                        { Player.SendMessage(p, "Impossible de creer le jeu, reessayez !"); return; }

                        Player.SendMessage(p, "Creation du ctf termine");
                        Player.GlobalMessage("Une infection vas bientot demarrer sur &b" + infection.lvl.name + Server.DefaultColor + " du monde &b" + infection.lvl.world);
                        return;
                    }

                    if (gameInfo.typeGame.ToLower() != "infection")
                    { Player.SendMessage(p, "Un jeu est deja actif sur cette map"); return; }

                    if (gameInfo != null)
                    {
                        if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return; }
                        if (gameInfo.gameOn)
                        { Player.SendMessage(p, "Arettez la partie avant de desactiver l'infection"); return; }

                        gameInfo.deleteGame(p);

                        Player.GlobalMessageLevel(p.level, "Infection desactive");
                        return;
                    }
                    break;
            }
        }

        public override void Help(Player p, string message = "")
        {
            if (message == "regles")
            {
                Player.SendMessage(p, "Au debut de la partie, un joueur devient zombie (_Infected_)");
                Player.SendMessage(p, "Son but est d'infecter tous les autres joueurs avant la fin du temps");
                Player.SendMessage(p, "Le but des survivants est de ne pas se faire attraper par les joueurs infecte");
                Player.SendMessage(p, "Sur certaines parties, il est possible de placer ou casser les blocs"); 
                return;
            }
            if (message == "mod")
            {
                Player.SendMessage(p, "/infection [nom] - Active/desactive l'infection sur la map.");
                Player.SendMessage(p, "/infection start - Demarre le jeu.");
                Player.SendMessage(p, "/infection stop - Arrete le jeu.");
                Player.SendMessage(p, "/infection list - Liste les configurations disponible.");
                Player.SendMessage(p, "/infection save [file] - Sauvegarde les configurations de partie.");
                Player.SendMessage(p, "/infection load [file] - Charge une configuration.");
                Player.SendMessage(p, "/infection blocs [nombre] - Change le nombre de blocs autorise");
                Player.SendMessage(p, "/infection temps [minutes] - Change le temps de la partie");
            }

            Player.SendMessage(p, "/help infection regles - Affiche les regles du jeu");
            Player.SendMessage(p, "/help infection mod - Affiche les commandes de moderation du jeu");
            Player.SendMessage(p, "/infection info - Donne des infos sur la partie.");
        }

        private bool verif(BaseGame gameInfo, Player p)
        {
            if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return false; }
            if (gameInfo.typeGame.ToLower() != "infection") { Player.SendMessage(p, "L'infection n'est pas actif sur cette map"); return false; }

            if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return false; }
            return true;
        }

    }
}
