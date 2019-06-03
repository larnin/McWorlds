using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdImport : Command
    {
        public override string name { get { return "import"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdImport() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            string fileName;
            fileName = "extra/import/" + message + ".dat";

            if (!Directory.Exists("extra/import")) Directory.CreateDirectory("extra/import");
            if (!File.Exists(fileName))
            {
                Player.SendMessage(p, "Impossible de trouver le fichier .dat.");
                return;
            }
            
            FileStream fs = File.OpenRead(fileName);
            if (ConvertDat.Load(fs, message) != null)
            {
                Player.SendMessage(p, "Map convertie.");
            }
            else
            {
                Player.SendMessage(p, "La convertion de la map a echouee.");
                return;
            }
            fs.Close();

            
            Command.all.Find("load").Use(p, message + " main");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/import [fichier .dat] - Importe une map .dat");
            Player.SendMessage(p, "Le fichier .dat doit etre place dans le dossier /extra/import/");
        }
    }
}