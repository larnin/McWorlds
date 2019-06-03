
using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdScript : Command
    {
        struct CatchPos
        {
            public bool ok;
            public byte type;
            public ushort x, y, z;
        }
        CatchPos cpos;
        string[] listvariables = { "a", "b" }; //liste de toutes les variables

        public override string name { get { return "script"; } }
        public override string shortcut { get { return "script"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdScript() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/script")) { Directory.CreateDirectory("extra/script"); }

            if (p == null) { Player.SendMessage(p, "Impossible d'utiliser les scripts depuis la console ou l'irc"); return; }
            
            if (message == "") { Help(p); return;}

            switch (message.Split(' ')[0])
            {
                case "create":
                    if (message.Split(' ').Length != 2) { Help(p); return; }
                    if (File.Exists("extra/script/" + message.Split(' ')[1] + ".mcwscr"))
                    { Player.SendMessage(p, "Le script '" + message.Split(' ')[1] + "' existe deja"); return; }
                    File.Create("extra/script/" + message.Split(' ')[1] + ".mcwscr");
                    p.scriptMod = message.Split(' ')[1];
                    Player.SendMessage(p, "Le script " + p.scriptMod + "a ete cree");
                    Player.SendMessage(p, "Vous modifiez a present le script " + p.scriptMod);
                    break;
                case "mod":
                    if (message.Split(' ').Length != 2){ Help(p); return;}
                    if (File.Exists("extra/script/" + message.Split(' ')[1] + ".mcwscr"))
                    {
                        p.scriptMod = message.Split(' ')[1];
                        Player.SendMessage(p, "Vous modifiez a present le script " + p.scriptMod);
                    }
                    else { Player.SendMessage(p, "Aucun script ayant le nom '" + message.Split(' ')[1] + "'"); }
                    break;
                case "stopmod":
                    p.scriptMod = "";
                    Player.SendMessage(p, "Vous ne modifiez plus de script");
                    break;
                case "add":
                    add(p, message);
                    break;
                case "rewrite":
                    rewrite(p, message);
                    break;
                case "suppr":
                    suppr(p, message);
                    break;
                case "view":
                    view(p, message);
                    break;
                case "list":
                    DirectoryInfo di = new DirectoryInfo("levels/" + message.ToLower() + "/");
                    FileInfo[] fi = di.GetFiles("*.mcwscr");
                    string allScripts = "";
                    foreach (FileInfo f in fi)
                    { allScripts += ", " + f.Name; }
                    if (allScripts == "") { Player.SendMessage(p, "Il n'existe aucun script"); }
                    else
                    {
                        allScripts.Remove(0, 2);
                        Player.SendMessage(p, "Liste des scripts :");
                        Player.SendMessage(p, allScripts);
                    }
                    break;
                case "exec":
                    exec(p, message);
                    break;
                case "stop":
                    stop(p, message);
                    break;
                case "del":
                    del(p, message);
                    break;
                default:
                    Help(p);
                    break;
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/script create nom - cree un nouveau script");
            Player.SendMessage(p, "/script mod <script> - Permet de modifier un script");
            Player.SendMessage(p, "/script stopmod - Arette la modification du script");
            Player.SendMessage(p, "/script add <param1> <param2> ... - Ajoute des elements au script");
            Player.SendMessage(p, "&5Toutes les infos sur les parametres sont donne dans un tuto sur le forum");
            Player.SendMessage(p, "/script rewrite <igne> <param1> <param2> ... - Modifie le contenu d'une ligne");
            Player.SendMessage(p, "/script suppr <ligne> - supprime une ligne au script");
            Player.SendMessage(p, "/script view <script> <page> - affiche le code du script");
            Player.SendMessage(p, "/script list - liste tous les scripts existant");
            Player.SendMessage(p, "/script exec <script> - execute le script sur la map actuelle");
            Player.SendMessage(p, "&cIl n'est possible d'executer qu'un seul script par map");
            Player.SendMessage(p, "/script stop - Arette le script en execution");
            Player.SendMessage(p, "/script del <script> - supprime un script (MODO+)");
        }

        // a faire
        public void add(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (p.scriptMod == "") { Player.SendMessage(p, "Vous ne modifiez pas de script, utilisez /script mod <script>"); return; }

            if (!File.Exists("extra/script/" + p.scriptMod + ".mcwscr"))
            {
                Player.SendMessage(p, "Le script que vous modifiez n'existe pas");
                p.scriptMod = "";
                return;
            }

            message = message.Remove(0, message.IndexOf(' ') + 1);

            string resultat = addElement(p, message);
        }

        //a faire
        public void rewrite(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (p.scriptMod == "") { Player.SendMessage(p, "Vous ne modifiez pas de script, utilisez /script mod <script>"); return; }
            
            if (!File.Exists("extra/script/" + p.scriptMod + ".mcwscr"))
            {
                Player.SendMessage(p, "Le script que vous modifiez n'existe pas");
                p.scriptMod = "";
                return;
            }

            message = message.Remove(0, message.IndexOf(' ') + 1);

            string resultat = addElement(p, message.Remove(0, message.IndexOf(' ') + 1));
        }

        //en cours - manque : if / boucle / var
        public string addElement(Player p, string message)
        {
            string[] parametres = message.Split(' ');
            string messageSend = "";

            switch (parametres[0])
            {
                case "changebloc":
                    if (parametres.Length < 3)
                    {
                        cpos.ok = false;
                        Player.SendMessage(p, "Placer un bloc");
                        p.ClearBlockchange();
                        p.blockchangeObject = null;
                        p.Blockchange += new Player.BlockchangeEventHandler(blocChange);
                        while (!cpos.ok) ;
                        if (parametres.Length == 2)
                        {
                            cpos.type = Block.Byte(parametres[1]);
                            if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + parametres[1] + "\"."); break; }
                            if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); break; }
                        }
                        messageSend = "changebloc " + cpos.x + " " + cpos.y + " " + cpos.z + " " + cpos.type; 
                    }
                    else if (parametres.Length == 5)
                    {
                        byte typeb = Block.Byte(parametres[1]);
                        if (typeb == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + parametres[1] + "\"."); break; }
                        if (!Block.canPlace(p, typeb)) { Player.SendMessage(p, "Impossible de placer ca."); break; }

                        int xb = 0, yb = 0, zb = 0;
                        try
                        {
                            xb = int.Parse(parametres[2]);
                            yb = int.Parse(parametres[3]);
                            zb = int.Parse(parametres[4]);
                            
                        }
                        catch { Player.SendMessage(p, "Valeur incorrecte"); break; }
                        if (xb < 0 || yb < 0 || zb < 0) { Player.SendMessage(p, "Valeur incorrecte"); break; }
                        messageSend = "changebloc " + xb + " " + yb + " " + zb + " " + typeb;
                    }
                    else { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    break;
                case "move":
                    if (parametres.Length != 4 && parametres.Length != 1) { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    int x = 0, y = 0, z = 0, rotx = p.rot[0], roty = p.rot[1];
                    if (parametres.Length < 4)
                    { x = p.pos[0] / 32; y = (p.pos[1] / 32) - 1; z = p.pos[2] / 32; }
                    else
                    {
                        try
                        {
                            x = int.Parse(parametres[1]);
                            y = int.Parse(parametres[2]);
                            z = int.Parse(parametres[3]);
                        }
                        catch { Player.SendMessage(p, "Valeur incorrecte"); break; }
                        if (x < 0 || y < 0 || z < 0) { Player.SendMessage(p, "Valeur incorrecte"); break; }
                        rotx = 128; roty = 128;
                    }
                    messageSend = "move " + x + " " + y + " " + z + " " + rotx + " " + roty;
                    break;
                case "if":

                    break;
                case "else":
                case "endif":
                case "endboucle":
                case "stop":
                    if (parametres.Length != 1) { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    messageSend = parametres[0];
                    break;
                case "boucle":

                    break;
                case "var":

                    break;
                case "commande":
                    if (parametres.Length < 2) { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    Command cmd = Command.all.Find(parametres[1]);
                    if (cmd != null)
                    {
                        LevelPermission perm = GrpCommands.allowedCommands.Find(grpComm => grpComm.commandName == cmd.name).lowestRank;
                        if (perm > LevelPermission.Operator)
                        { Player.SendMessage(p, "Impossible d'utiliser cette commande"); }
                    }
                    else { Player.SendMessage(p, "Commande '" + parametres[1] + "' inconnue"); break; }
                    messageSend = message;
                    break;
                case "message":
                    if (parametres.Length < 2) { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    messageSend = message;
                    break;
                case "wait":
                    if (parametres.Length != 2) { Player.SendMessage(p, "Nombre de parametres invalide"); break; }
                    int temps = 0;
                    try { temps = int.Parse(parametres[1]); }
                    catch { Player.SendMessage(p, "Valeur incorrecte"); break; }
                    if (temps <= 0) { Player.SendMessage(p, "Valeur incorrecte"); break; }
                    messageSend = message;
                    break;
            }

            return messageSend;
        }

        //fait - a valider
        private void blocChange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            CatchPos bp;
            bp.x = x; bp.y = y; bp.z = z; bp.type = type; bp.ok = true;
            cpos = bp;
        }

        //fait - a valider
        public void suppr(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (p.scriptMod == "") { Player.SendMessage(p, "Vous ne modifiez pas de script, utilisez /script mod <script>"); return; }

            if (!File.Exists("extra/script/" + p.scriptMod + ".mcwscr"))
            {
                Player.SendMessage(p, "Le script que vous modifiez n'existe pas");
                p.scriptMod = "";
                return;
            }
            message = message.Remove(0, message.IndexOf(' ') + 1);

            int line = 0;
            try { line = int.Parse(message); }
            catch { Player.SendMessage(p, "Valeur incorrecte"); return; }
            if (line <= 0) { Player.SendMessage(p, "Valeur incorrecte"); return; }

            string[] allLines = File.ReadAllLines("extra/script/" + p.scriptMod + ".mcwscr");
            
            if (allLines.Length < line) 
            { 
                Player.SendMessage(p, "Ligne " + line + " innexistante dans le fichier");
                Player.SendMessage(p, "Il y a " + allLines.Length + " lignes au total");
                return; 
            }
            
            StreamWriter SW = new StreamWriter(File.Create("extra/savepoints2/" + p.name.ToLower() + ".txt"));
            for (int i = 0; i < allLines.Length; i++)
            {
                if (i + 1 == line) { continue; }
                SW.WriteLine(allLines[i]);
            }

            SW.Flush();
            SW.Close();
            SW.Dispose();
        }

        //fait - a valider
        public void view(Player p, string message)
        {
            if (message.IndexOf(' ') == -1 || message.Split(' ').Length > 3) { Help(p); return; }

            if (!File.Exists("extra/script/" + message.Split(' ')[1] + ".mcwscr"))
            { Player.SendMessage(p, "Le script '" + message.Split(' ')[1] + "' n'existe pas"); return; }

            string[] allLines = File.ReadAllLines("extra/script/" + message.Split(' ')[1] + ".mcwscr");

            int numpage = -1;
            if (message.Split(' ').Length == 3)
            {
                try{ numpage = int.Parse(message.Split(' ')[2]);}
                catch{ Player.SendMessage(p, "Valeur incorrecte"); return;}
                if (numpage < 1 ){ Player.SendMessage(p, "Valeur incorrecte"); return;}
            }
            
            int lineStart = 0, lineStop = 0;

            if (numpage == -1){lineStop = allLines.Length;}
            else
            {
                if ((numpage - 1) * 10 > allLines.Length)
                { Player.SendMessage(p, "Il y a " + allLines.Length + " lignes dans le fichier"); return; }

                lineStart = (numpage - 1) * 10;
                lineStop = numpage * 10;
                if (lineStop > allLines.Length) { lineStop = allLines.Length; }
            }

            if (numpage == -1)
            { Player.SendMessage(p, "Fichier  : &b" + message.Split(' ')[1]); }
            else
            { Player.SendMessage(p, "Fichier  : &b" + message.Split(' ')[1] + Server.DefaultColor + ", ligne " + lineStart + " a " + lineStop); }

            for (int i = lineStart; i < lineStop; i++)
            { Player.SendMessage(p, allLines[i]); }
        }

        // a faire
        public void exec(Player p, string message)
        {
            if (p.level.scriptEnCours != null) { Player.SendMessage(p, "Un script est deja en cours sur cette map"); return; }

            if (message.IndexOf(' ') == -1 || message.Split(' ').Length > 2) { Help(p); return; }
            message = message.Remove(0, message.IndexOf(' ') + 1);

            if (!File.Exists("extra/script/" + message.Split(' ')[1] + ".mcwscr"))
            { Player.SendMessage(p, "Le script '" + message.Split(' ')[1] + "' n'existe pas"); return; }

            Player.SendMessage(p, "Chargement du script");
            script scriptEx = new script();
            string error = scriptEx.load(message, p.level);

            if (error != "") { Player.SendMessage(p, "&cErreur : " + Server.DefaultColor + error); return;}
            Player.SendMessage(p, "Chargement reussi !"); 
        }

        //a faire
        public void stop(Player p, string message)
        {
            if (p.level.scriptEnCours == null) { Player.SendMessage(p, "Aucun script est en cours"); return; }

            p.level.scriptEnCours.stop();
            p.level.scriptEnCours = null;
        }

        //fait - a valider
        public void del(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }

            if (p.group.Permission < LevelPermission.Operator)
            { Player.SendMessage(p, "Fonction reserve aux MODO+ pour le moment"); return; }

            if (!File.Exists("extra/script/" + message.Split(' ')[1] + ".mcwscr"))
            { Player.SendMessage(p, "Le script '" + message.Split(' ')[1] + "' n'existe pas"); }

            File.Delete("extra/script/" + message.Split(' ')[1] + ".mcwscr");
            Player.SendMessage(p, "Script '" + message.Split(' ')[1] + "' supprime");
        }
    }
}