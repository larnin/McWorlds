using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdText : Command
    {
        public override string name { get { return "text"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdText() { }

        public override void Use(Player p, string message)
        {
            if (!Directory.Exists("extra/text/")) Directory.CreateDirectory("extra/text");
            if (message.IndexOf(' ') == -1) { Help(p); return; }

            try
            {
                if (message.Split(' ')[0].ToLower() == "delete")
                {
                    if (File.Exists("extra/text/" + message.Split(' ')[1] + ".txt"))
                    {
                        File.Delete("extra/text/" + message.Split(' ')[1] + ".txt");
                        Player.SendMessage(p, "Fichier supprime");
                    }
                    else
                    {
                        Player.SendMessage(p, "Impossible de trouver le fichier");
                    }
                }
                else
                {
                    bool again = false;
                    string fileName = "extra/text/" + message.Split(' ')[0] + ".txt";
                    string group = Group.findPerm(LevelPermission.Builder).name;
                    if (Group.Find(message.Split(' ')[1]) != null)
                    {
                        group = Group.Find(message.Split(' ')[1]).name;
                        again = true;
                    }
                    message = message.Substring(message.IndexOf(' ') + 1);
                    if (again)
                        message = message.Substring(message.IndexOf(' ') + 1);
                    string contents = message;
                    if (contents == "") { Help(p); return; }
                    if (!File.Exists(fileName))
                        contents = "#" + group + System.Environment.NewLine + contents;
                    else
                        contents = " " + contents;
                    File.AppendAllText(fileName, contents);
                    Player.SendMessage(p, "Text ajoute");
                }
            } catch { Help(p); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/text [fichier] [rang] [message] - cree un fichier visible avec /view.");
            Player.SendMessage(p, "Le [rang] est le rang minimum pour pouvoir lire le fichier.");
            Player.SendMessage(p, "Le [message] est le texte rajoute au fichier.");
            Player.SendMessage(p, "Si le fichier existe le texte sera rajoute a la fin du fichier.");
        }
    }
}