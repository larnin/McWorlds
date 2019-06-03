using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdFoot : Command
    {
        public override string name { get { return "foot"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public CmdFoot() { }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }


            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            FootGame footInfo = null;
            if (footInfo != null) { if (gameInfo.typeGame == "foot") { footInfo = (FootGame)gameInfo; } }

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
                    if (footInfo.gameOn) { Player.SendMessage(p, "Le jeu est deja lance"); return; }
                    footInfo.startGame(p);
                    break;
                case "stop":
                    if (!verif(gameInfo, p)) { return; }
                    if (!footInfo.gameOn) { Player.SendMessage(p, "Le jeu n'est pas lance"); return; }
                    footInfo.stopGame(p);
                    break;
                case "info":
                    if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
                    if (gameInfo.typeGame.ToLower() != "foot") { Player.SendMessage(p, "Le foot n'est pas actif sur cette map"); return; }
                    footInfo.GameInfo(p);
                    break;
                case "list":
                    BaseGame.listSave(p, "foot");
                    break;
                case "save":
                    if (!verif(gameInfo, p)) { return; }
                    if (gameInfo.gameOn) { Player.SendMessage(p, "Impossible de sauvegarder les configurations quand le foot est lance"); return; }
                    gameInfo.saveGame(p, message);
                    break;
                case "load":
                    if (!verif(gameInfo, p)) { return; }
                    if (gameInfo.gameOn) { Player.SendMessage(p, "Impossible de charger des configurations quand le foot est lance"); return; }
                    gameInfo.loadGame(p, message);
                    break;
                case "but":
                    if (!verif(gameInfo, p)) { return; }
                    CatchPos cpos;
                    cpos.x = 0; cpos.y = 0; cpos.z = 0;
                    switch (message)
                    {
                        case "red":
                        case "rouge":
                            cpos.colorTeam = "red";
                            break;
                        case "bleu":
                        case "blue":
                            cpos.colorTeam = "blue";
                            break;
                        default:
                            Player.SendMessage(p, "Couleur inconnue");
                            return;
                    }
                    p.blockchangeObject = cpos;
                    Player.SendMessage(p, "Place 2 blocs pour determiner la zone de but.");
                    p.ClearBlockchange();
                    p.Blockchange += new Player.BlockchangeEventHandler(butBlocChange1);
                    break;
                case "spawn":
                    if (!verif(gameInfo, p)) { return; }
                    p.blockchangeObject = null;
                    Player.SendMessage(p, "Place un bloc pour determiner la position du spawn du ballon.");
                    p.ClearBlockchange();
                    p.Blockchange += new Player.BlockchangeEventHandler(BallonBlocChange);
                    break;
                case "points":
                    if (!verif(gameInfo, p)) { return; }
                    int points = 0 ;
                    try { points = int.Parse(message); }
                    catch { Player.SendMessage(p, "Valeur invalide"); return; }
                    if (points < 1 || points > 25) { Player.SendMessage(p, "La valeur doit etre entre 1 et 25"); return; }
                    footInfo.points = points;
                    break;
                case "joint":
                    if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
                    if (gameInfo.typeGame.ToLower() != "foot") { Player.SendMessage(p, "Le foot n'est pas actif sur cette map"); return; }
                    
                    retry:
                    if (message == "")
                    {
                        Random rand = new Random();
                        if (rand.Next(2) == 0) { message = "red"; }
                        else { message = "blue"; }
                    }
                    switch (message)
                    {
                        case "red":
                        case "rouge":
                        case "bleu":
                        case "blue":
                            jointTeam(p, footInfo, message);
                            break;
                        default :
                            message = "";
                            goto retry;
                    }
                    break;
                default:
                    if (gameInfo == null)
                    {
                        if (p.level.world != p.name.ToLower() && p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous ne pouvez pas lancer de jeu dans ce monde"); return;}

                        if (Server.allGames.Count > Server.maxGames)
                        {
                            Player.SendMessage(p, "Le maximum de jeux en simultanees est attein");
                            Player.SendMessage(p, "Attendez qu'une partie se finisse avant d'en lancer une autre");
                            return;
                        }
                        FootGame bB = new FootGame(p.level);
                        bB.owner = p;

                        if (key == "") { Player.SendMessage(p, "Vous devez donner un nom a la partie"); return; }
                        if (message != "") { Player.SendMessage(p, "Le nom de la partie doit etre fait d'un seul mot"); return; }
                        bB.name = key;

                        if (!BaseGame.addGame(bB))
                        { Player.SendMessage(p, "Impossible de creer le jeu, reessayez !"); return; }

                        Player.SendMessage(p, "Creation du foot termine");
                        Player.GlobalMessage("Une partie de foot vas bientot demarrer sur &b" + bB.lvl.name + Server.DefaultColor + " du monde &b" + bB.lvl.world); 
                        return;
                    }

                    if (gameInfo.typeGame.ToLower() != "foot")
                    { Player.SendMessage(p, "Un jeu est deja actif sur cette map"); return; }

                    if (gameInfo != null)
                    {
                        if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return; }
                        if (gameInfo.gameOn)
                        { Player.SendMessage(p, "Arettez la partie avant de desactiver le foot"); return; }

                        gameInfo.deleteGame(p);

                        Player.GlobalMessageLevel(p.level, "Mode foot desactive");
                        return;
                    }
                    break;
            }
        }

        public override void Help(Player p, string message = "")
        {
            if (message == "regles")
            {
                Player.SendMessage(p, "Le but du jeu est d'ammener le ballon dans la cage adverse");
                Player.SendMessage(p, "Le ballon est represente par un cube blanc");
                Player.SendMessage(p, "Pour prendre le ballon, il vous suffit de passer a cote, il vous suivra");
                return;
            }
            if (message == "mod")
            {
                Player.SendMessage(p, "/foot [nom] - Active/desactive le foot sur la map.");
                Player.SendMessage(p, "A besoin d'etre active pour jouer.");
                Player.SendMessage(p, "/foot start - Demarre le jeu !");
                Player.SendMessage(p, "/foot stop - Arrete le jeu.");
                Player.SendMessage(p, "/foot list - Liste les configurations disponible");
                Player.SendMessage(p, "/foot save [file] - Sauvegarde les configurations de partie");
                Player.SendMessage(p, "/foot load [file] - Charge une configuration");
                Player.SendMessage(p, "/foot but <red/blue> - Selectionne la zone de but d'une equipe");
                Player.SendMessage(p, "/foot spawn - Place le point de spawn du ballon");
                Player.SendMessage(p, "/foot points - Change le nombre de but a marquer");
            }

            Player.SendMessage(p, "/help foot regles - Affiche les regles du jeu");
            Player.SendMessage(p, "/help foot mod - Affiche les commandes de moderation du jeu");
            Player.SendMessage(p, "/foot info - Donne des informations sur la partie");
            Player.SendMessage(p, "/foot joint <red/blue> - Permet de rejoindre une equipe");
        }

        private bool verif(BaseGame gameInfo, Player p)
        {
            if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return false; }
            if (gameInfo.typeGame.ToLower() != "foot") { Player.SendMessage(p, "Le foot n'est pas actif sur cette map"); return false; }

            if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return false; }
            return true;
        }

        private void jointTeam(Player p, FootGame game, string colorTeam)
        {
            if (game == null) { Player.SendMessage(p, "Error"); return; }
            if (colorTeam == "red" || colorTeam == "rouge")
            {
                if (p.team != null) { p.team.RemoveMember(p); }
                game.redTeam.AddMember(p);
            }
            else
            {
                if (p.team != null) { p.team.RemoveMember(p); }
                game.blueTeam.AddMember(p);
            }
        }

        void BallonBlocChange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);

            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            FootGame footInfo = null;
            if (gameInfo == null) { Player.SendMessage(p, "Il n'y a pas de foot d'active sur cette map"); return; }
            if (gameInfo.typeGame == "foot") { footInfo = (FootGame)gameInfo; }
            if (footInfo == null) { Player.SendMessage(p, "Il n'y a pas de foot d'active sur cette map"); return; }

            if (footInfo.gameOn) { Player.SendMessage(p, "Vous ne pouvez pas changer le spawn quand le jeu est en cours"); return; }

            footInfo.spawnBallon.x = x;
            footInfo.spawnBallon.y = y;
            footInfo.spawnBallon.z = z;
            Player.SendMessage(p, "Point de spawn du ballon definit");
        }

        void butBlocChange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(butBlocChange2);
        }

        void butBlocChange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;

            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            FootGame footInfo = null;
            if (gameInfo == null) { Player.SendMessage(p, "Il n'y a pas de foot d'active sur cette map"); return; }
            if (gameInfo.typeGame == "foot") { footInfo = (FootGame)gameInfo; }
            if (footInfo == null) { Player.SendMessage(p, "Il n'y a pas de foot d'active sur cette map"); return; }

            if (footInfo.gameOn) { Player.SendMessage(p, "Vous ne pouvez pas changer un zone de but lorsque la partie est en cour"); return; }

            if (bp.colorTeam == "red")
            {
                footInfo.redButMin.x = Math.Min(x, bp.x);
                footInfo.redButMax.x = Math.Max(x, bp.x);
                footInfo.redButMin.y = Math.Min(y, bp.y);
                footInfo.redButMax.y = Math.Max(y, bp.y);
                footInfo.redButMin.z = Math.Min(z, bp.z);
                footInfo.redButMax.z = Math.Max(z, bp.z);
                Player.SendMessage(p, "But de l'equipe rouge positionne");
            }
            else if (bp.colorTeam == "blue")
            {
                footInfo.blueButMin.x = Math.Min(x, bp.x);
                footInfo.blueButMax.x = Math.Max(x, bp.x);
                footInfo.blueButMin.y = Math.Min(y, bp.y);
                footInfo.blueButMax.y = Math.Max(y, bp.y);
                footInfo.blueButMin.z = Math.Min(z, bp.z);
                footInfo.blueButMax.z = Math.Max(z, bp.z);
                Player.SendMessage(p, "But de l'equipe bleu positionne");
            }
            else { Player.SendMessage(p, "Erreur de couleur"); return; }
        }

        public struct CatchPos 
        { 
            public ushort x, y, z;
            public string colorTeam;
        }

    }
}
