using System;
using System.IO;

namespace MCWorlds
{
    public class CmdBotAI : Command
    {
        public override string name { get { return "botai"; } }
        public override string shortcut { get { return "bai"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBotAI() { }

        public override void Use(Player p, string message)
        {
            if (message == "list" || message == "liste")
            {
                if (!Directory.Exists("bots/")) { Directory.CreateDirectory("bots/"); }
                DirectoryInfo di = new DirectoryInfo("bots/");
                FileInfo[] fi = di.GetFiles();

                string bots = "";

                foreach (FileInfo file in fi)
                {
                    bots += file.Name + ", ";
                }

                if (bots == "") { Player.SendMessage(p, "Aucunes ia existante"); }
                else { Player.SendMessage(p, "IAs :" + bots.Remove(bots.Length - 2)); }
                return;
            }


            if (message.Split(' ').Length < 2) { Help(p); return; }

            string foundPath = message.Split(' ')[1].ToLower();

            if (!Player.ValidName(foundPath)) { Player.SendMessage(p, "Nom d'IA invalide!"); return; }
            if (foundPath == "hunt" || foundPath == "kill") { Player.SendMessage(p, "Reserve aux AI speciales."); return; }

            try
            {
                switch (message.Split(' ')[0])
                {
                    case "add":
                        if (message.Split(' ').Length == 2) addPoint(p, foundPath);
                        else if (message.Split(' ').Length == 3) addPoint(p, foundPath, message.Split(' ')[2]);
                        else if (message.Split(' ').Length == 4) addPoint(p, foundPath, message.Split(' ')[2], message.Split(' ')[3]);
                        else addPoint(p, foundPath, message.Split(' ')[2], message.Split(' ')[3], message.Split(' ')[4]);
                        break;
                    case "del":
                        if (!Directory.Exists("bots/deleted")) Directory.CreateDirectory("bots/deleted");

                        int currentTry = 0;
                        if (File.Exists("bots/" + foundPath))
                        {
                        retry: try
                            {
                                if (message.Split(' ').Length == 2)
                                {
                                    if (currentTry == 0)
                                        File.Move("bots/" + foundPath, "bots/deleted/" + foundPath);
                                    else
                                        File.Move("bots/" + foundPath, "bots/deleted/" + foundPath + currentTry);
                                }
                                else
                                {
                                    if (message.Split(' ')[2].ToLower() == "last")
                                    {
                                        string[] Lines = File.ReadAllLines("bots/" + foundPath);
                                        string[] outLines = new string[Lines.Length - 1];
                                        for (int i = 0; i < Lines.Length - 1; i++)
                                        {
                                            outLines[i] = Lines[i];
                                        }

                                        File.WriteAllLines("bots/" + foundPath, outLines);
                                        Player.SendMessage(p, "Supprime le dernie rpoint de" + foundPath);
                                        return;
                                    }
                                    else
                                    {
                                        Help(p); return;
                                    }
                                }
                            }
                            catch (IOException) { currentTry++; goto retry; }
                            Player.SendMessage(p, "Supprime &b" + foundPath);
                        }
                        else
                        {
                            Player.SendMessage(p, "Ne trouve pas l'IA");
                        }
                        break;
                    default: Help(p); return;
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/botai <add/del> [Nom AI] <extra> - Ajoute ou supprime  [nom AI]");
            Player.SendMessage(p, "Extras: walk, teleport, wait, nod, speed, spin, reset, remove, reverse, linkscript, jump");
            Player.SendMessage(p, "wait, nod et spin peuvent avoir un parametre de '0,1 secondes' supplementaires");
            Player.SendMessage(p, "nod et spin peuvent egalement prendre un 'troisième' parametre de vitesse");
            Player.SendMessage(p, "speed fixe la vitesse a un pourcentage de la vitesse normale");
            Player.SendMessage(p, "linkscript prend un nom de script comme parametre");
            Player.SendMessage(p, "/botai list - liste toutes les ia cree");
        }

        public void addPoint(Player p, string foundPath, string additional = "", string extra = "10", string more = "2")
        {
            string[] allLines;
            try { allLines = File.ReadAllLines("bots/" + foundPath); }
            catch { allLines = new string[1]; }

            StreamWriter SW;
            try
            {
                if (!File.Exists("bots/" + foundPath))
                {
                    Player.SendMessage(p, "Cree une nouvelle IA de bot: &b" + foundPath);
                    SW = new StreamWriter(File.Create("bots/" + foundPath));
                    SW.WriteLine("#Version 2");
                }
                else if (allLines[0] != "#Version 2")
                {
                    Player.SendMessage(p, "Le fichier trouver est depacer. Reecriture");
                    SW = new StreamWriter(File.Create("bots/" + foundPath));
                    SW.WriteLine("#Version 2");
                }
                else
                {
                    Player.SendMessage(p, "Apprentissage a l'IA: &b" + foundPath);
                    SW = new StreamWriter("bots/" + foundPath, true);
                }
            }
            catch { Player.SendMessage(p, "Une erreur s'est produite lors de l'acces aux fichiers. Vous devez le supprimer."); return; }

            try
            {
                switch (additional.ToLower())
                {
                    case "":
                    case "walk":
                        SW.WriteLine("walk " + p.pos[0] + " " + p.pos[1] + " " + p.pos[2] + " " + p.rot[0] + " " + p.rot[1]);
                        break;
                    case "teleport":
                    case "tp":
                        SW.WriteLine("teleport " + p.pos[0] + " " + p.pos[1] + " " + p.pos[2] + " " + p.rot[0] + " " + p.rot[1]);
                        break;
                    case "wait":
                        SW.WriteLine("wait " + int.Parse(extra)); break;
                    case "nod":
                        SW.WriteLine("nod " + int.Parse(extra) + " " + int.Parse(more)); break;
                    case "speed":
                        SW.WriteLine("speed " + int.Parse(extra)); break;
                    case "remove":
                        SW.WriteLine("remove"); break;
                    case "reset":
                        SW.WriteLine("reset"); break;
                    case "spin":
                        SW.WriteLine("spin " + int.Parse(extra) + " " + int.Parse(more)); break;
                    case "reverse":
                        for (int i = allLines.Length - 1; i > 0; i--) if (allLines[i][0] != '#' && allLines[i] != "") SW.WriteLine(allLines[i]);
                        break;
                    case "linkscript":
                        if (extra != "10") SW.WriteLine("linkscript " + extra); else Player.SendMessage(p, "Linkscript necessite un script comme paramètre");
                        break;
                    case "jump":
                        SW.WriteLine("jump"); break;
                    default:
                        Player.SendMessage(p, "Could not find \"" + additional + "\""); break;
                }

                SW.Flush();
                SW.Close();
                SW.Dispose();
            }
            catch { Player.SendMessage(p, "parametre invalide"); SW.Close(); }
        }
    }
}