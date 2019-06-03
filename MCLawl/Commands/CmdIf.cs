
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class Cmdif : Command
    {
        public override string name { get { return "if"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public Cmdif() { }

        public override void Use(Player p, string message)
        {
            if (message == "symboles") { seeSymboles(p); return;}
            if (message == "parametre" || message == "param") { seeParam(p); return; }

            if (p == null) { Player.SendMessage(p, "Vous ne pouvez pas executer cette commande depuis la console ou l'irc"); return; }

            if (message.Split(' ').Length < 5) { Help(p); return; }
            if (message.IndexOf("then") == -1) { Help(p); return; }
            if (message.LastIndexOf("if") != message.IndexOf("if")) { Player.SendMessage(p, "Vous ne pouvez pas supperposer plusieurs if"); return; }

            string parametre = message.Split(' ')[0];
            string symbole = message.Split(' ')[1];
            string valeur = message.Split(' ')[2];

            bool[] symbolesTest = {false, false, false} ;
            // < - [0], == - [1], > - [2]
            // Permet de decomposer les comparaisons

            if (symbole == "<" || symbole == "<=" || symbole == ">=")
            {symbolesTest[0] = true;}

            if ( symbole == "<=" || symbole == ">=" || symbole == "==")
            {symbolesTest[1] = true;}

            if (symbole == ">=" || symbole == ">" || symbole == "!=")
            {symbolesTest[2] = true;}

            if (symbolesTest[0] == false && symbolesTest[1] == false && symbolesTest[2] == false)
            { Player.SendMessage(p, "Symbole de comparaison incorrecte"); return; }

            string cmds = message.Substring(message.IndexOf("then") + 5);
            string cmd1 = "", cmd2 = "";
            if (message.IndexOf(" else ") == -1)
            { cmd1 = cmds;}
            else
            {
                cmd1 = cmds.Substring(0, cmds.IndexOf(" else "));
                cmd2 = cmds.Substring(cmds.IndexOf(" else ") + 6);
            }

            if (cmdtest(cmd1.Split(' ')[0]))
            { Player.SendMessage(p, "La commande '" + cmd1.Split(' ')[0] + "' ne peut pas etre utilise en if"); }
            if (cmd2 != "")
            {
                if (cmdtest(cmd2.Split(' ')[0]))
                { Player.SendMessage(p, "La commande '" + cmd2.Split(' ')[0] + "' ne peut pas etre utilise en if"); }
            }

            bool[] paramTest = { false, false, false }; 
            // < - [0], == - [1], > - [2]
            // Permet de decomposer les comparaisons

            int valChiffre = 0;
            try { valChiffre = int.Parse(valeur); }
            catch { valChiffre = -1; }

            bool valBool = false;
            if (valChiffre != -1) { if (valChiffre != 0) { valBool = false; } }
            else { if (valeur == "true") { valBool = true; } }

            switch (parametre)
            {
                case "name":
                    paramTest = egalite(p.name, valeur);
                    break;
                case "color":
                    Player.SendMessage(p, "Parametre non disponible pour le moment"); return;
                    break;
                case "title":
                    paramTest = egalite(p.title, valeur);
                    break;
                case "tcolor":
                    Player.SendMessage(p, "Parametre non disponible pour le moment"); return;
                    break;
                case "money":
                    if (valChiffre == -1) { Player.SendMessage(p, "Erreur : Quantitée incorrecte"); return; }
                    paramTest = egalite(p.money, valChiffre);
                    break;
                case "blocks":
                    if (valChiffre == -1) { Player.SendMessage(p, "Erreur : Quantitée incorrecte"); return; }
                    paramTest = egalite((int)p.overallBlocks, valChiffre);
                    break;
                case "deaths":
                    if (valChiffre == -1) { Player.SendMessage(p, "Erreur : Quantitée incorrecte"); return; }
                    paramTest = egalite(p.deathCount, valChiffre);
                    break;
                case "rang":
                    if (valChiffre != -1) { paramTest = egalite((int)p.group.Permission, valChiffre); }
                    else
                    {
                        Group g = Group.Find(valeur);
                        if (g != null)
                        { paramTest = egalite((int)p.group.Permission, (int)g.Permission); }
                        else { Player.SendMessage(p, "ERREUR : rang '" + valeur + "' inconnu"); return; }
                    }
                    break;
                case "first":
                    Player.SendMessage(p, "Parametre non disponible pour le moment"); return;
                    break;
                case "time":
                    Player.SendMessage(p, "Parametre non disponible pour le moment"); return;
                    break;
                case "vip":
                    paramTest = egalite(p.vip, valBool);
                    break;
                case "fly":
                    paramTest = egalite(p.isFlying, valBool);
                    break;
                case "gun":
                    paramTest = egalite(p.aiming, valBool);
                    break;
                case "build":
                    paramTest = egalite(p.canBuild, valBool);
                    break;
                default:
                    Player.SendMessage(p, "parametre inconnu");
                    return;
            }

            if (paramTest[0] == false && paramTest[1] == false && paramTest[2] == false)
            { Player.SendMessage(p, "ERREUR : origine de l'erreur inconnue"); return; }

            if (paramTest[0] == symbolesTest[0] || paramTest[1] == symbolesTest[1] || paramTest[2] == symbolesTest[2])
            {
                string Cmd1Message = "";
                if (cmd1.IndexOf(' ') != -1)
                {
                    Cmd1Message = cmd1.Substring(cmd1.IndexOf(' ') + 1);
                    cmd1.Substring(0, cmd1.IndexOf(' '));
                }

                if (cmd1 == "message" && Cmd1Message != "") { Player.SendMessage(p, Cmd1Message); return; }

                Command.all.Find(cmd1).Use(p, Cmd1Message);
            }
            else
            {
                string Cmd1Message = "";
                if (cmd1.IndexOf(' ') != -1)
                {
                    Cmd1Message = cmd1.Substring(cmd1.IndexOf(' ') + 1);
                    cmd1.Substring(0, cmd1.IndexOf(' '));
                }

                if (cmd1 == "message" && Cmd1Message != "") { Player.SendMessage(p, Cmd1Message); return; }

                Command.all.Find(cmd1).Use(p, Cmd1Message);
            }

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/if [parametre] == [valeur] then <commande> [else <commande>]");
            Player.SendMessage(p, "Permet d'executer des actions conditionnelles");
            Player.SendMessage(p, "Si la condition est vrais, la commande marque apres le then sera execute");
            Player.SendMessage(p, "Sinon c'est la commande apres le else qui est execute (non obligatoire)");
            Player.SendMessage(p, "la commande 'message' permet d'afficher un message au joueur seulement");
            Player.SendMessage(p, "/if symboles - affiche les symboles de test utilisable");
            Player.SendMessage(p, "/if parametre - affiche tous les parametres utilisables");
            Player.SendMessage(p, "&cFonction incomplette, les parametres en rouge ne sont pas encore supporte");
        }

        public void seeParam(Player p)
        {
            Player.SendMessage(p, "Liste des parametres : [les symboles utilisable]");
            Player.SendMessage(p, "name - le nom du joueur [== ou !=]");
            Player.SendMessage(p, "&ccolor - la couleur du joueur [== ou !=]");
            Player.SendMessage(p, "title - le titre du joueur [== ou !=]");
            Player.SendMessage(p, "&ctcolor - la couleur du titre [== ou !=]");
            Player.SendMessage(p, "money - l'argent du joueur [tous]");
            Player.SendMessage(p, "blocks - le nombre de blocs modifie [tous]");
            Player.SendMessage(p, "deaths - le nombre de mort [tous]");
            Player.SendMessage(p, "rang - le rang du joueur [tous]");
            Player.SendMessage(p, "&cfirst - la date d'arrive sur le serveur [tous]");
            Player.SendMessage(p, "&ctime - l'heure (l'heure du serveur, pas la votre) [tous]");
            Player.SendMessage(p, "vip - si le joueur est vip [== ou !=]");
            Player.SendMessage(p, "fly - si le joueur est en mode fly [== ou !=]");
            Player.SendMessage(p, "gun - si le joueur a une arme [== ou !=]");
            Player.SendMessage(p, "build - si le joueur peut construire [== ou !=]");
        }

        public void seeSymboles(Player p)
        {
            Player.SendMessage(p, "Liste des symboles :");
            Player.SendMessage(p, "== - test si le parametre est egale a la valeur");
            Player.SendMessage(p, "!= - test si le parametre est inegal a la valeur");
            Player.SendMessage(p, "> - si le parametre est strictement superieur a la valeur");
            Player.SendMessage(p, ">= - si le parametre est superieur ou egal a la valeur");
            Player.SendMessage(p, "< - si le parametre est strictement inferieur a la valeur");
            Player.SendMessage(p, "<= - si la valeur est inferieur ou egal a la valeur");
        }

        public bool cmdtest(string cmdString)
        {
            Command cmd = Command.all.Find(cmdString);
            if (cmd != null)
            {
                LevelPermission perm = GrpCommands.allowedCommands.Find(grpComm => grpComm.commandName == cmd.name).lowestRank;
                if (perm > LevelPermission.Operator)
                { return false; }
            }
            return true;
        }

        public bool[] egalite(int param, int value)
        {
            bool[] test = {false, false, false};
                if (param == value)
                { test[1] = true; }
                if (param != value)
                { test[0] = true; test[2] = true;}
                if (param < value)
                { test[0] = true; }
                if (param <= value)
                { test[0] = true; test[1] = true; }
                if (param > value)
                { test[2] = true; }
                if (param >= value)
                { test[1] = true; test[2] = true; }
            return test;
        }

        public bool[] egalite(string param, string value)
        {
            bool[] test = { false, false, false };
            if (param != value)
            { test[0] = true; test[1] = true; }
            else { test[1] = true; }
            return test;
        }

        public bool[] egalite(bool param, bool value)
        {
            bool[] test = { false, false, false };
            if (param != value)
            { test[0] = true; test[1] = true; }
            else { test[1] = true; }
            return test;
        }

    }
}
