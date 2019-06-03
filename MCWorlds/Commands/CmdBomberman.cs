
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    public class CmdBomberman : Command
    {
        public override string name { get { return "bomberman"; } }
        public override string shortcut { get { return "bb"; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBomberman() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible de faire ca depuis la console ou l'IRC"); return; }

            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            BombermanGame bBInfo = null;
            if (gameInfo != null) { if (gameInfo.typeGame == "bomberman") { bBInfo = (BombermanGame)gameInfo; } }
            

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
                    if (bBInfo.gameOn) { Player.SendMessage(p, "Le jeu est deja lance"); return; }
                    bBInfo.startGame(p);
                    break;
                case "stop":
                    if (!verif(gameInfo, p)) { return; }
                    if (!bBInfo.gameOn) { Player.SendMessage(p, "Le jeu n'est pas lance"); return; }
                    bBInfo.stopGame(p);
                    break;
                case "info":
                    if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return; }
                    if (gameInfo.typeGame.ToLower() != "bomberman") { Player.SendMessage(p, "Le bomberman n'est pas actif sur cette map"); return; }
                    bBInfo.GameInfo(p);
                    break;
                case "list":
                    BaseGame.listSave(p, "bomberman");
                    break;
                case "save":
                    if (!verif(gameInfo, p)) { return; }
                    if (gameInfo.gameOn){ Player.SendMessage(p, "Impossible de sauvegarder les configurations quand le bomberman est lance"); return; }
                    gameInfo.saveGame(p, message);
                    break;
                case "load":
                    if (!verif(gameInfo, p)) { return; }
                    if (gameInfo.gameOn){ Player.SendMessage(p, "Impossible de charger des configurations quand le bomberman est lance"); return; }
                    gameInfo.loadGame(p, message);
                    break;
                case "zone":
                    if (!verif(gameInfo, p)) { return; }
                    CatchPos cpos;
                    cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;
                    Player.SendMessage(p, "Place 2 blocs pour determiner la taille.");
                    p.ClearBlockchange();
                    p.Blockchange += new Player.BlockchangeEventHandler(ZoneBlocChange1);
                    break;
                case "portail":
                    if (!verif(gameInfo, p)) { return; }
                    if (gameInfo.gameOn) { Player.SendMessage(p, "Impossible de creer un portail quand le bomberman est lance"); return; }
                    bBInfo.addPortail(p, (ushort)(p.pos[0] / 32), (ushort)(p.pos[1] / 32), (ushort)(p.pos[2] / 32));
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
                        BombermanGame bB = new BombermanGame(p.level);
                        bB.owner = p;

                        if (key == "") { Player.SendMessage(p, "Vous devez donner un nom a la partie"); return; }
                        if (message != "") { Player.SendMessage(p, "Le nom de la partie doit etre fait d'un seul mot"); return; }
                        bB.name = key;

                        if (!BaseGame.addGame(bB))
                        { Player.SendMessage(p, "Impossible de creer le jeu, reessayez !"); return; }

                        Player.SendMessage(p, "Creation du bomberman termine");
                        Player.GlobalMessage("Une partie de bomberman vas bientot demarrer sur &b" + bB.lvl.name + Server.DefaultColor + " du monde &b" + bB.lvl.world); 
                        return;
                    }

                    if (gameInfo.typeGame.ToLower() != "bomberman")
                    { Player.SendMessage(p, "Un jeu est deja actif sur cette map"); return; }

                    if (gameInfo != null)
                    {
                        if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return; }
                        if (gameInfo.gameOn)
                        { Player.SendMessage(p, "Arettez la partie avant de desactiver le bomberman"); return; }

                        gameInfo.deleteGame(p);

                        Player.GlobalMessageLevel(p.level, "Mode bomberman desactive");
                        return;
                    }
                    break;

            }
        }

        public override void Help(Player p, string message = "")
        {
            if (message == "regles")
            {
                Player.SendMessage(p, "Le but du jeu est de tuer ses adversaires a la bombe");
                Player.SendMessage(p, "Pour placer une bombe, utilisez le bloc de tnt");
                Player.SendMessage(p, "Attention, les bombes peuvent exploser en serie !");
                Player.SendMessage(p, "Vous pouvez aussi placer un mur avec un bloc de plaches");
                return;
            }
            if (message == "mod")
            {
                Player.SendMessage(p, "/bomberman [nom] - Active/desactive le bomberman sur la map");
                Player.SendMessage(p, "/bomberman start - Lance la partie");
                Player.SendMessage(p, "/bomberman stop - Arette le jeu");
                Player.SendMessage(p, "/bomberman list - Liste les configurations disponible");
                Player.SendMessage(p, "/bomberman save [file] - Sauvegarde les configurations de partie");
                Player.SendMessage(p, "/bomberman load [file] - Charge une configuration");
                Player.SendMessage(p, "/bomberman zone - Cree la zone de jeu");
                Player.SendMessage(p, "&cAttention : Les blocs seront modifie au debut du jeu !");
                Player.SendMessage(p, "/bomberman portail - Cree un portail d'entree dans la zone de jeu");
            }

            Player.SendMessage(p, "/help bomberman regles - Affiche les regles du jeu");
            Player.SendMessage(p, "/help bomberman mod - Affiche les commandes de moderation du jeu");
            Player.SendMessage(p, "/bomberman info - Donne des informations sur la partie");
        }

        private bool verif(BaseGame gameInfo, Player p)
        {
            if (gameInfo == null) { Player.SendMessage(p, "Il y a aucune partie en cours sur la map"); return false; }
            if (gameInfo.typeGame.ToLower() != "bomberman") { Player.SendMessage(p, "Le bomberman n'est pas actif sur cette map"); return false; }

            if (p != gameInfo.owner || p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Vous n'etes pas le maitre de la partie"); return false; }
            return true;
        }

        void ZoneBlocChange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(ZoneBlocChange2);
        }

        void ZoneBlocChange2(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;

            int sizeX = Math.Abs(x - bp.x), sizeZ = Math.Abs(z - bp.z);
            int minY = Math.Abs(y - bp.y);

            BaseGame gameInfo = Server.allGames.Find(g => g.lvl == p.level);
            BombermanGame bBInfo = null;
            if (gameInfo != null) { bBInfo = (BombermanGame)gameInfo; }
            if (bBInfo == null) { Player.SendMessage(p, "Il n'y a pas de bomberman d'active sur cette map"); return; }
            
            if (bBInfo.gameOn) { Player.SendMessage(p, "Vous ne pouvez pas changer la zone quand le jeu est en cours"); return; }

            if (sizeX < 14 || sizeZ < 14) { Player.SendMessage(p, "La zone de jeu est trop petite !!!"); }

            bBInfo.setZone(p, x, bp.x, Math.Min(y, bp.y), z, bp.z);
        }

        public struct CatchPos { public ushort x, y, z; }
    }
}
