using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdView : Command
    {
        public override string name { get { return "view"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdView() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("extra/text/")) Directory.CreateDirectory("extra/text");
            if (message == "")
            {
                DirectoryInfo di = new DirectoryInfo("extra/text/");
                string allFiles = "";
                foreach (FileInfo fi in di.GetFiles("*.txt"))
                {
                    try
                    {
                        string firstLine = File.ReadAllLines("extra/text/" + fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length) + ".txt")[0];
                        if (firstLine[0] == '#')
                        {
                            if (Group.Find(firstLine.Substring(1)).Permission <= p.group.Permission)
                            {
                                allFiles += ", " + fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                            }
                        }
                        else
                        {
                            allFiles += ", " + fi.Name;
                        }
                    } catch (Exception e) { Server.ErrorLog(e); Player.SendMessage(p, "Erreur"); }
                }

                if (allFiles == "")
                {
                    Player.SendMessage(p, "Pas de fichier que vous pouvez lire.");
                }
                else
                {
                    Player.SendMessage(p, "Fichiers disponibles:");
                    Player.SendMessage(p, allFiles.Remove(0, 2));
                }
            }
            else
            {
                Player who = null;
                if (message.IndexOf(' ') != -1)
                {
                    who = Player.Find(message.Split(' ')[message.Split(' ').Length - 1]);
                    if (who != null)
                        message = message.Substring(0, message.LastIndexOf(' '));
                }
                if (who == null) who = p;

                if (File.Exists("extra/text/" + message + ".txt"))
                {
                    try
                    {
                        string[] allLines = File.ReadAllLines("extra/text/" + message + ".txt");
                        if (allLines[0][0] == '#')
                        {
                            if (Group.Find(allLines[0].Substring(1)).Permission <= p.group.Permission)
                            {
                                for (int i = 1; i < allLines.Length; i++)
                                {
                                    Player.SendMessage(who, allLines[i]);
                                }
                            }
                            else
                            {
                                Player.SendMessage(p, "Vous ne pouvez pas lire ce fichier.");
                            }
                        }
                        else
                        {
                            for (int i = 1; i < allLines.Length; i++)
                            {
                                Player.SendMessage(who, allLines[i]);
                            }
                        }
                    }
                    catch { Player.SendMessage(p, "Une erreur s'est produite lors de la recuperation du fichier"); }
                }
                else
                {
                    Player.SendMessage(p, "Ce fichier n'existe pas");
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/view [fichier] - Permet de lire un fichier");
            Player.SendMessage(p, "/view [fichier] [joueur] - Montre le fichier au joueur");
            Player.SendMessage(p, "/view - Affiche tous les fichier lisibles");
        }
    }
}