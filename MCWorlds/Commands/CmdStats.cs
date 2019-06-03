
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdStats : Command
    {
        public static List<string> listGame = new List<string>() { "bomberman", "ctf", "infection" };

        public override string name { get { return "stats"; } }
        public override string shortcut { get { return "stat"; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdStats() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }

            if (message == "list" || message == "liste")
            {
                if (listGame.Count == 0) { Player.SendMessage(p, "Aucun jeu ne propose de statistiques"); }

                string li = "";
                foreach (string g in listGame)
                { li += g + ", "; }
                Player.SendMessage(p, "Jeux avec statistiques : " + li.Remove(li.Length - 2));
                return;
            }

            if (message.Split(' ').Length < 2) { Help(p); return; }

            switch (message.Split(' ')[0])
            {
                case "top":
                    switch (message.Split(' ')[1])
                    {
                        case "bomberman":
                            BombermanGame.top(p);
                            break;
                        case "ctf":
                            CTFGame2.top(p);
                            break;
                        case "infection":
                            InfectionGame.top(p);
                            break;
                        default:
                            Player.SendMessage(p, "Le jeu '" + message.Split(' ')[1] + "' n'a pas de statistiques");
                            break;
                    }
                    break;
                case "reset":
                    if (p.group.Permission < LevelPermission.Admin) { Player.SendMessage(p, "Tu n'a pas les permissions de faire ca"); }
                    if (!listGame.Exists(m => m == message.Split(' ')[1])) { Player.SendMessage(p, "Le jeu '" + message.Split(' ')[1] + "' n'a pas de statistiques"); }
                    if (message.Split(' ').Length == 2) //resetjeu
                    {
                        BaseGame.resetGame(message.Split(' ')[1]);
                        Player.SendMessage(p, "Statistiques du jeu '" + message.Split(' ')[1] + "' reinitialise");
                    }
                    else
                    {
                        BaseGame.resetPlayer(message.Split(' ')[2], message.Split(' ')[1]);
                        Player.SendMessage(p, "Statistique de '" + message.Split(' ')[2] + "' reset sur '" + message.Split(' ')[1] + "'");
                    }
                    break;
                case "bomberman":
                    BombermanGame.stats(p, message.Split(' ')[1]);
                    break;
                case "ctf":
                    CTFGame2.stats(p, message.Split(' ')[1]);
                    break;
                case "infection":
                    InfectionGame.stats(p, message.Split(' ')[1]);
                    break;
                default :
                    Player.SendMessage(p, "Le jeu '" + message.Split(' ')[0] + "' n'a pas de statistiques");
                    break; 
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/stats list - Donne les jeux dans lesquels les stats sont disponible.");
            Player.SendMessage(p, "/stats top [jeu] - Liste les meilleurs joueurs du jeu.");
            Player.SendMessage(p, "/stats [jeu] [joueur] - Permet de voir les resultats du joueur");
            Player.SendMessage(p, "/stats reset [jeu] <joueur> - Permet de remetre a 0 les stats (ADMIN +)");
        }
    }
}
