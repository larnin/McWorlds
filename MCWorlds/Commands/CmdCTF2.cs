using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdCTF2 : Command
    {
        public override string name { get { return "ctf"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public CmdCTF2() { }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }


            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            CTFGame2 ctfInfo = null;
            if (gameInfo != null) { if (gameInfo.typeGame == "ctf") { ctfInfo = (CTFGame2)gameInfo; } }

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
                    if (ctfInfo.gameOn)
                    { Player.SendMessage(p, "Le jeu est deja lance"); return; }
                    ctfInfo.startGame(p);
                    break;
                case "stop":
                    if (!verif(gameInfo, p)) { return; }
                    if (!ctfInfo.gameOn)
                    { Player.SendMessage(p, "Le jeu n'est pas lance"); return; }
                    ctfInfo.gameOn = false;
                    ctfInfo.stopGame(p);
                    break;
                case "info":
                    if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
                    if (gameInfo.typeGame.ToLower() != "ctf") { Player.SendMessage(p, "Le ctf n'est pas actif sur cette map"); return; }
                    ctfInfo.GameInfo(p);
                    break;
                case "list":
                    BaseGame.listSave(p,"ctf");
                    break;
                case "save":
                    if (!verif(gameInfo, p)) { return; }
                    if (ctfInfo.gameOn) { Player.SendMessage(p, "Impossible de sauvegarder les configurations quand le CTF est lance"); return; }
                    ctfInfo.saveGame(p, message);
                    break;
                case "load":
                    if (!verif(gameInfo, p)) { return; }
                    if (ctfInfo.gameOn) { Player.SendMessage(p, "Impossible de charger une configuration quand le CTF est lance"); return; }
                    ctfInfo.loadGame(p, message);
                    break;
                case "clear":
                    if (!verif(gameInfo, p)) { return; }
                    ctfInfo.stopGame(p);
                    List<Team> storedT = new List<Team>();
                    for (int i = 0; i < ctfInfo.teams.Count; i++)
                    {
                        storedT.Add(ctfInfo.teams[i]);
                    }
                    foreach (Team t in storedT)
                    {
                        ctfInfo.RemoveTeam("&" + t.color);
                    }
                    ctfInfo.onTeamCheck.Stop();
                    ctfInfo.onTeamCheck.Dispose();
                    ctfInfo.gameOn = false;
                    ctfInfo.loadCmds();
                    Player.SendMessage(p, "Les parametres du CTF ont ete remis a 0.");
                    break;
                case "ff":
                    if (!verif(gameInfo, p)) { return; }
                    if (ctfInfo.friendlyfire)
                    {
                        p.level.ChatLevel("Les tirs alliers ont ete activer.");
                        ctfInfo.friendlyfire = false;
                    }
                    else
                    {
                        p.level.ChatLevel("Les tirs alliers ont ete activer.");
                        ctfInfo.friendlyfire = true;
                    }
                    break;
                case "flag":
                    if (!verif(gameInfo, p)) { return; }
                    string colorf = c.Parse(message);
                    if (colorf == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    char teamColf = (char)colorf[1];
                    if (ctfInfo.teams.Find(team => team.color == teamColf) == null) { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    CatchPos cpos;
                    cpos.x = 0; cpos.y = 0; cpos.z = 0; cpos.color = colorf; p.blockchangeObject = cpos;
                    Player.SendMessage(p, "Place un bloc pour determiner la position du drapeau.");
                    p.ClearBlockchange();
                    p.Blockchange += new Player.BlockchangeEventHandler(FlagBlockChange);
                    break;
                case "spawn":
                    if (!verif(gameInfo, p)) { return; }
                    string colors = c.Parse(message);
                    if (colors == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    char teamCols = (char)colors[1];
                    if (ctfInfo.teams.Find(team => team.color == teamCols) == null) { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    AddSpawn(p, colors,ctfInfo);
                    break;
                case "points":
                    if (!verif(gameInfo, p)) { return; }
                    int points = 0;
                    Int32.TryParse(message, out points);
                    if (points <= 0) { Player.SendMessage(p, "Vous devez choisir un nombre de points superieur a 0 !"); return; }
                    ctfInfo.maxPoints = points;
                    Player.SendMessage(p, "Le nombre de points a ete mis a " + points);
                    break;
                case "team":
                    if (!verif(gameInfo, p)) { return; }
                    if (message.Split(' ').Length != 2) { Help(p); return; }

                    if (message.Split(' ')[0] == "add")
                    {
                        string colort = c.Parse(message.Split(' ')[1]);
                        if (colort == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                        char teamColt = (char)colort[1];
                        switch (teamColt)
                        {
                            case '2':
                            case '5':
                            case '8':
                            case '9':
                            case 'c':
                            case 'e':
                            case 'f':
                                AddTeam(p, colort, ctfInfo);
                                break;
                            default:
                                Player.SendMessage(p, "Couleur d'equipe invalide.");
                                return;
                        }
                    }
                    else if (message.Split(' ')[0] == "remove")
                    {
                        string colort = c.Parse(message.Split(' ')[1]);
                        if (colort == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                        char teamColt = (char)colort[1];
                        switch (teamColt)
                        {
                            case '2':
                            case '5':
                            case '8':
                            case '9':
                            case 'c':
                            case 'e':
                            case 'f':
                                RemoveTeam(p, colort, ctfInfo);
                                break;
                            default:
                                Player.SendMessage(p, "Couleur d'equipe invalide.");
                                return;
                        }
                    }
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
                        CTFGame2 ctf = new CTFGame2(p.level);
                        ctf.owner = p;

                        if (key == "") { Player.SendMessage(p, "Vous devez donner un nom a la partie"); return; }
                        if (message != "") { Player.SendMessage(p, "Le nom de la partie doit etre fait d'un seul mot"); return; }
                        ctf.name = key;

                        if (!BaseGame.addGame(ctf))
                        { Player.SendMessage(p, "Impossible de creer le jeu, reessayez !"); return; }

                        Player.SendMessage(p, "Creation du ctf termine");
                        Player.GlobalMessage("Une partie de CTF vas bientot demarrer sur &b" + ctf.lvl.name + Server.DefaultColor + " du monde &b" + ctf.lvl.world); 
                        return;
                    }

                    if (gameInfo.typeGame.ToLower() != "ctf")
                    { Player.SendMessage(p, "Un jeu est deja actif sur cette map"); return; }

                    if (gameInfo != null)
                    {
                        if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return; }
                        if (gameInfo.gameOn)
                        { Player.SendMessage(p, "Arettez la partie avant de desactiver le ctf"); return; }

                        gameInfo.deleteGame(p);

                        Player.GlobalMessageLevel(p.level, "Mode CTF desactive");
                        return;
                    }
                    break;
            }
        }

        public override void Help(Player p, string message = "")
        {
            if (message == "regles")
            {
                Player.SendMessage(p, "Le but du jeu est de prendre le drabeau adverse et de le ramener dans son camp");
                Player.SendMessage(p, "Pour prendre ou poser un drapeau, passez simplement dessous");
                Player.SendMessage(p, "Vous pouvez tuer vos ennemis grace au gun ou avec les missiles (/gun et /missile)");
                Player.SendMessage(p, "Vous pouvez aussi poser le drapeau au sol avec la commande /drop");
                return;
            }
            if (message == "mod")
            {
                Player.SendMessage(p, "/ctf [nom] - Active/desactive le mode CTF sur la map.");
                Player.SendMessage(p, "A besoin d'etre active pour jouer.");
                Player.SendMessage(p, "/ctf start - Demarre le jeu !");
                Player.SendMessage(p, "/ctf stop - Arrete le jeu.");
                Player.SendMessage(p, "/ctf list - Liste les configurations disponible");
                Player.SendMessage(p, "/ctf save [file] - Sauvegarde les configurations de partie");
                Player.SendMessage(p, "/ctf load [file] - Charge une configuration");
                Player.SendMessage(p, "/ctf clear - Supprime toutes les donnees du CTF");
                Player.SendMessage(p, "/ctf ff - Active/desactive les tirs allier. Desactive par defaut.");
                Player.SendMessage(p, "/ctf flag [couleur] - Place le drapeau pour l'equipe [couleur].");
                Player.SendMessage(p, "/ctf spawn [couleur] - Place le spawn pour l'equipe [couleur].");
                Player.SendMessage(p, "/ctf points [num] - Modifie le nombre de round. 3 par defaut.");
                Player.SendMessage(p, "/ctf team add [couleur] - Ajoute une equipe.");
                Player.SendMessage(p, "/ctf team remove [couleur] - Supprime une equipe.");
            }

            Player.SendMessage(p, "/help ctf regles - Affiche les regles du jeu");
            Player.SendMessage(p, "/help ctf mod - Affiche les commandes de moderation du jeu");
            Player.SendMessage(p, "/ctf info - Donne des informations sur la partie");
            Player.SendMessage(p, "/team join [couleur] - Permet de joindre une equipe");
        }

        private bool verif(BaseGame gameInfo, Player p)
        {
            if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return false; }
            if (gameInfo.typeGame.ToLower() != "ctf") { Player.SendMessage(p, "Le ctf n'est pas actif sur cette map"); return false; }

            if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return false; }
            return true;
        }

        public void AddTeam(Player p, string color, CTFGame2 game)
        {
            char teamCol = (char)color[1];
            if (game.teams.Find(team => team.color == teamCol) != null) { Player.SendMessage(p, "Cet equipe existe deja."); return; }
            game.AddTeam(color);
        }

        public void RemoveTeam(Player p, string color, CTFGame2 game)
        {
            char teamCol = (char)color[1];
            if (game.teams.Find(team => team.color == teamCol) == null) { Player.SendMessage(p, "Cet equipe n'existe pas."); return; }
            game.RemoveTeam("&" + color);
        }

        public void AddSpawn(Player p, string color, CTFGame2 game)
        {
            char teamCol = (char)color[1];
            ushort x, y, z, rotx;
            x = (ushort)(p.pos[0] / 32);
            y = (ushort)(p.pos[1] / 32);
            z = (ushort)(p.pos[2] / 32);
            rotx = (ushort)(p.rot[0]);
            game.teams.Find(team => team.color == teamCol).AddSpawn(x, y, z, rotx, 0);
            Player.SendMessage(p, "Spawn ajoute pour " + game.teams.Find(team => team.color == teamCol).teamstring);
        }

        void FlagBlockChange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            CatchPos bp = (CatchPos)p.blockchangeObject;
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            p.ClearBlockchange();
            AddFlag(p, bp.color, x, y, z);
        }

        public void AddFlag(Player p, string col, ushort x, ushort y, ushort z)
        {
            char teamCol = (char)col[1];

            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);

            if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
            if (gameInfo.typeGame.ToLower() != "ctf") { Player.SendMessage(p, "Le ctf n'est pas actif sur cette map"); return; }

            if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return; }

            CTFGame2 ctfInfo = (CTFGame2)gameInfo;

            Team workTeam = ctfInfo.teams.Find(team => team.color == teamCol);

            workTeam.flagBase[0] = x;
            workTeam.flagBase[1] = y;
            workTeam.flagBase[2] = z;

            workTeam.flagLocation[0] = x;
            workTeam.flagLocation[1] = y;
            workTeam.flagLocation[2] = z;
            workTeam.Drawflag();
        }

        public struct CatchPos { public ushort x, y, z; public string color;}

    }
}
