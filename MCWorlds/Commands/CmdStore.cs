using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdStore : Command
    {
        public override string name { get { return "store"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public List<CopyOwner> list = new List<CopyOwner>();
        public CmdStore() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            try
            {
                if (message == "") { Help(p); return; }

                if (message.IndexOf(' ') == -1)
                {
                    if (File.Exists("extra/copy/" + message + ".copy"))
                    {
                        Player.SendMessage(p, "Le fichier &f" + message + Server.DefaultColor + " existe deja. Le supprimer avant.");
                        return;
                    }
                    else
                    {
                        Player.SendMessage(p, "Storing: " + message);
                        StreamWriter sW = new StreamWriter(File.Create("extra/copy/" + message + ".copy"));
                        sW.WriteLine("Saved by: " + p.name + " at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss "));
                        for (int k = 0; k < p.CopyBuffer.Count; k++)
                        {
                            sW.WriteLine(p.CopyBuffer[k].x + " " + p.CopyBuffer[k].y + " " + p.CopyBuffer[k].z + " " + p.CopyBuffer[k].type);
                        }
                        sW.Flush();
                        sW.Close();

                        sW = File.AppendText("extra/copy/index.copydb");
                        sW.WriteLine(message + " " + p.name);
                        sW.Flush();
                        sW.Close();
                    }
                }
                else
                {
                    if (message.Split(' ')[0] == "delete")
                    {
                        message = message.Split(' ')[1];
                        list.Clear();
                        foreach (string s in File.ReadAllLines("extra/copy/index.copydb"))
                        {
                            CopyOwner cO = new CopyOwner();
                            cO.file = s.Split(' ')[0];
                            cO.name = s.Split(' ')[1];
                            list.Add(cO);
                        }
                        CopyOwner result = list.Find(
                            delegate(CopyOwner cO) {
                                return cO.file == message;
                            }
                        );

                        if (p.group.Permission >= LevelPermission.Operator || result.name == p.name)
                        {
                            if (File.Exists("extra/copy/" + message + ".copy"))
                            {
                                try
                                {
                                    if (File.Exists("extra/copyBackup/" + message + ".copy")) { File.Delete("extra/copyBackup/" + message + ".copy"); }
                                    File.Move("extra/copy/" + message + ".copy", "extra/copyBackup/" + message + ".copy");
                                }
                                catch { }
                                Player.SendMessage(p, "File &f" + message + Server.DefaultColor + " has been deleted.");
                                list.Remove(result);
                                File.Delete("extra/copy/index.copydb");
                                StreamWriter sW = new StreamWriter(File.Create("extra/copy/index.copydb"));
                                foreach (CopyOwner cO in list)
                                {
                                    sW.WriteLine(cO.file + " " + cO.name);
                                }
                                sW.Flush();
                                sW.Close();
                            }
                            else
                            {
                                Player.SendMessage(p, "Le fichier n'existe pas.");
                            }
                        }
                        else
                        {
                            Player.SendMessage(p, "Vous devez etre op ou le proprietaire pour suprimer ce fichier.");
                            return;
                        }
                    }
                    else { Help(p); return; }
                }

            }
            catch (Exception e) { Server.ErrorLog(e); }
        }
        public class CopyOwner
        {
            public string name;
            public string file;
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/store [fichier] - Enregistre ce que vous avez copier dans un  fichier .");
            Player.SendMessage(p, "/store delete [fichier] - Supprime un fichier de sauvegarde.");
            return;
        }
    }
}
