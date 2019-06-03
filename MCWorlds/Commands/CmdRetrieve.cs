using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdRetrieve : Command
    {
        public override string name { get { return "retrieve"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public List<CopyOwner> list = new List<CopyOwner>();
        public CmdRetrieve() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            try
            {
                if (!File.Exists("extra/copy/index.copydb")) { Player.SendMessage(p, "Aucunes copie trouvee! Sauvegardez quelque chose avant d'essayer de le recuperer!"); return; }
                if (message == "") { Help(p); return; }
                if (message.Split(' ')[0] == "info")
                {
                    if (message.IndexOf(' ') != -1)
                    {
                        message = message.Split(' ')[1];
                        if (File.Exists("extra/copy/" + message + ".copy"))
                        {
                            StreamReader sR = new StreamReader(File.OpenRead("extra/copy/" + message + ".copy"));
                            string infoline = sR.ReadLine();
                            sR.Close();
                            sR.Dispose();
                            Player.SendMessage(p, infoline);
                            return;
                        }
                    }
                    else
                    {
                        Help(p);
                        return;
                    }
                }
                if (message.Split(' ')[0] == "find")
                {
                    message = message.Replace("find", "");
                    string storedcopies = ""; int maxCopies = 0; int findnum = 0; int currentnum = 0;
                    bool isint = int.TryParse(message, out findnum);
                    if (message == "") { goto retrieve; }
                    if (!isint)
                    {
                        message = message.Trim();
                        list.Clear();
                        foreach (string s in File.ReadAllLines("extra/copy/index.copydb"))
                        {
                            CopyOwner cO = new CopyOwner();
                            cO.file = s.Split(' ')[0];
                            cO.name = s.Split(' ')[1];
                            list.Add(cO);
                        }
                        List<CopyOwner> results = new List<CopyOwner>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].name.ToLower() == message.ToLower())
                            {
                                storedcopies += ", " + list[i].file;
                            }
                        }
                        if (storedcopies == "") { Player.SendMessage(p, "Pas de copies sauvegardee pour : " + message); }
                        else
                        {
                            Player.SendMessage(p, "Copies sauvegarde : ");
                            Player.SendMessage(p, "&f " + storedcopies.Remove(0, 2));
                        }
                        return;
                    }

                    // SEARCH BASED ON NAME STUFF ABOVE HERE
                    if (isint)
                    {
                        maxCopies = findnum * 50; currentnum = maxCopies - 50;
                    }
        retrieve:   DirectoryInfo di = new DirectoryInfo("extra/copy/");
                    FileInfo[] fi = di.GetFiles("*.copy");

                    if (maxCopies == 0)
                    {
                        foreach (FileInfo file in fi)
                        {
                            storedcopies += ", " + file.Name.Replace(".copy", "");
                        }
                        if (storedcopies != "")
                        {
                            Player.SendMessage(p, "Copies sauvegarde: ");
                            Player.SendMessage(p, "&f " + storedcopies.Remove(0, 2));
                            if (fi.Length > 50) { Player.SendMessage(p, "Pour une liste plus structure, utilisez /retrieve find <1/2/3/...>"); }
                        }
                        else { Player.SendMessage(p, "Pas de copies sauvegardes."); }
                    }
                    else
                    {
                        if (maxCopies > fi.Length) maxCopies = fi.Length;
                        if (currentnum > fi.Length) { Player.SendMessage(p, "Pas de copie sauvegarde pour le nombre " + fi.Length); return; }

                        Player.SendMessage(p, "copies sauvegarde (" + currentnum + " sur " + maxCopies + "):");
                        for (int i = currentnum; i < maxCopies; i++)
                        {
                            storedcopies += ", " + fi[i].Name.Replace(".copy", "");
                        }
                        if (storedcopies != "")
                        {
                            Player.SendMessage(p, "&f" + storedcopies.Remove(0, 2));
                        }
                        else Player.SendMessage(p, "Il n'y a pas de copie sauvegarde.");
                    }
                }
                else
                {
                    if (message.IndexOf(' ') == -1)
                    {
                        message = message.Split(' ')[0];
                        if (File.Exists("extra/copy/" + message + ".copy"))
                        {
                            p.CopyBuffer.Clear();
                            bool readFirst = false;
                            foreach (string s in File.ReadAllLines("extra/copy/" + message + ".copy"))
                            {
                                if (readFirst)
                                {
                                    Player.CopyPos cP;
                                    cP.x = Convert.ToUInt16(s.Split(' ')[0]);
                                    cP.y = Convert.ToUInt16(s.Split(' ')[1]);
                                    cP.z = Convert.ToUInt16(s.Split(' ')[2]);
                                    cP.type = Convert.ToByte(s.Split(' ')[3]);
                                    p.CopyBuffer.Add(cP);
                                }
                                else readFirst = true;
                            }
                            Player.SendMessage(p, "&f" + message + Server.DefaultColor + " est place dans le buffer de copie.  Collez le!");
                        }
                        else
                        {
                            Player.SendMessage(p, "Impossible de trouver la copie");
                            return;
                        }
                    }
                    else 
                    { 
                        Help(p); 
                        return; 
                    }
                }
            }
            catch (Exception e) { Player.SendMessage(p, "Erreur"); Server.ErrorLog(e); }
        }

        public class CopyOwner
        {
            public string name;
            public string file;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/retrieve [fichier] - Recupere une copie sauvegarde. /paste pour la coller!");
            Player.SendMessage(p, "/retrieve info [fichier] - Donne des informations sur le fichier.");
            Player.SendMessage(p, "/retrieve find - Affiche la liste de toutes les copies.");
            Player.SendMessage(p, "/retrieve find [1/2/3/..] - Affiche une liste ordonnee.");
            Player.SendMessage(p, "/retrieve find [pseudo] - Affiche la liste des copies sauvegarde par un joueur.");
            return;
        }
    }
}

    
        