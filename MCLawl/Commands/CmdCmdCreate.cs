using System;
using System.IO;
namespace MCWorlds
{
    public class CmdCmdCreate : Command
    {
        public override string name { get { return "cmdcreate"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdCmdCreate() { }
        
        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') != -1)
            {
                Help(p);
                return;
            }
            else
            {
                if (File.Exists("extra/commands/source/Cmd" + message + ".cs")) { p.SendMessage("File Cmd" + message + ".cs already exists.  Choose another name."); return; }
                try
                {
                    Scripting.CreateNew(message);
                }
                catch (Exception e)
                {
                    Server.ErrorLog(e);
                    Player.SendMessage(p, "Une erreur s'est produite la création du fichier classe.");
                    return;
                }
                Player.SendMessage(p, "Creation du fichier class reussi.");
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/cmdcreate [commande] - Cree un nouveau fichier class nome Cmd[commande].cs a partir duquel vous pouvez creer une nouvelle commande .");
        }
    }
}
