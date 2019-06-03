using System;
using System.IO;
using System.Data;

namespace MCWorlds
{
    public class CmdSave : Command
    {
        public override string name { get { return "save"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdSave() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { message = "all"; }

            if (message.ToLower() == "all")
            {
                for ( int i = 0 ; i < Server.levels.Count ; i++)
                {
                    try
                    {
                        Server.levels[i].Save();
                    }
                    catch { }
                }
                Player.GlobalMessage("Toutes les maps ont ete sauvegarde.");
            }
            else
            {
                if (message.Split(' ').Length == 1)         //Just save level given
                {
                    Level foundLevel = Level.Find(message, p.level.world);
                    if (foundLevel != null)
                    {
                        foundLevel.Save(true);
                        Player.SendMessage(p, "Map \"" + foundLevel.name + "\" sauvegardee.");
                        int backupNumber = p.level.Backup(true);
                        if (backupNumber != -1)
                            p.level.ChatLevel("Sauvegarde " + backupNumber + " terminee.");
                    }
                    else
                    {
                        Player.SendMessage(p, "Impossible de trouver la map demande");
                    }
                }
                else if (message.Split(' ').Length == 2)
                {
                    Level foundLevel = Level.Find(message.Split(' ')[0], p.level.world);
                    string restoreName = message.Split(' ')[1].ToLower();
                    if (foundLevel != null)
                    {
                        foundLevel.Save(true);
                        int backupNumber = p.level.Backup(true, restoreName);
                        Player.GlobalMessage(foundLevel.name + " a une nouvelle sauvegarde apelee &b" + restoreName);
                    }
                    else
                    {
                        Player.SendMessage(p, "Impossible de trouver la map demande.");
                    }
                }
                else
                {
                    if (p == null)
                    {
                        Use(p, "all");
                    }
                    else
                    {
                        p.level.Save(true);
                        Player.SendMessage(p, "Map \"" + p.level.name + "\" sauvegarde.");

                        int backupNumber = p.level.Backup(true);
                        if (backupNumber != -1)
                            p.level.ChatLevel("Sauvegarde " + backupNumber + " terminee.");
                    }
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/save - Sauvegarde la map ou vous etes");
            Player.SendMessage(p, "/save all - Sauvegarde toutes les maps charge.");
            Player.SendMessage(p, "/save [map] - Sauvegarde une map situe dans le monde ou vous etes.");
            Player.SendMessage(p, "/save [map] [nom] - Cree un backup de la map avec un nom  particulier");
        }
    }
}