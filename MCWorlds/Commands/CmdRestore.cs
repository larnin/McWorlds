using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MCWorlds
{
    class CmdRestore : Command
    {
        public override string name { get { return "restore"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdRestore() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message != "")
            {
                Server.s.Log(Server.backupLocation + "/" + p.level.world + "/" + p.level.name + "/" + message + "/" + p.level.name + ".lvl");
                if (File.Exists(Server.backupLocation + "/" + p.level.world + "/" + p.level.name + "/" + message + "/" + p.level.name + ".lvl"))
                {
                    try
                    {
                        File.Copy(Server.backupLocation + "/" + p.level.world + "/" + p.level.name + "/" + message + "/" + p.level.name + ".lvl", "levels/" + p.level.world + "/" + p.level.name + ".lvl", true);
                        Level temp = Level.Load(p.level.name, p.level.world);
                        temp.physThread.Start();
                        if (temp != null)
                        {
                            p.level.spawnx = temp.spawnx;
                            p.level.spawny = temp.spawny;
                            p.level.spawnz = temp.spawnz;

                            p.level.height = temp.height;
                            p.level.width = temp.width;
                            p.level.depth = temp.depth;

                            p.level.blocks = temp.blocks;
                            p.level.setPhysics(0);
                            p.level.ClearPhysics();

                            Command.all.Find("reveal").Use(p, "all");
                        }
                        else
                        {
                            Server.s.Log("Restore nulled");
                            File.Copy("levels/" + p.level.world + "/" + p.level.name + ".lvl.backup", "levels/" + p.level.world + "/" + p.level.name + ".lvl", true);
                        }

                    }
                    catch 
                    { 
                        Server.s.Log("Restore fail"); 
                        Player.SendMessage(p, "Erreur");
                    }
                    
                }
                else { Player.SendMessage(p, "La sauvegarde " + message + " n'existe pas."); }
            }
            else
            {
                if (Directory.Exists(Server.backupLocation + "/" + p.level.world + "/" + p.level.name))
                {
                    string[] directories = Directory.GetDirectories(Server.backupLocation + "/" + p.level.world + "/" + p.level.name);
                    int backupNumber = directories.Length;
                    Player.SendMessage(p, p.level.name + " a " + backupNumber + " sauvegardes .");

                    bool foundOne = false; string foundRestores = "";
                    foreach (string s in directories)
                    {
                        string directoryName = s.Substring(s.LastIndexOf('\\') + 1);
                        try
                        {
                            int.Parse(directoryName);
                        }
                        catch
                        {
                            foundOne = true;
                            foundRestores += ", " + directoryName;
                        }
                    }

                    if (foundOne)
                    {
                        Player.SendMessage(p, "Restauration d'une sauvegarde-custom:");
                        Player.SendMessage(p, "> " + foundRestores.Remove(0, 2));
                    }
                }
                else
                {
                    Player.SendMessage(p, p.level.name + " n'a pas de sauvegardes.");
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/restore <nombre> - restaure une sauvegarde precedente de la map actuelle");
        }
    }
}
