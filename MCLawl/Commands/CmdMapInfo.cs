using System;
using System.IO;

namespace MCWorlds
{
    public class CmdMapInfo : Command
    {
        public override string name { get { return "mapinfo"; } }
        public override string shortcut { get { return "status"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdMapInfo() { }

        public override void Use(Player p, string message)
        {
            Level foundLevel;
            
            if (message.Split(' ').Length > 2) { Help(p); return; }
            string lvl = "";
            string world = "";
            if (message.IndexOf(' ') == -1)
            {
                lvl = message;
                if (p == null) { world = "main"; }
                else { world = p.level.world; }
            }
            if ( message.Split(' ').Length == 2 )
            { lvl = message.Split(' ')[0]; world = message.Split(' ')[1]; }

            if (message == "") 
            {
                if (p == null) { foundLevel = Server.mainLevel; }
                else { foundLevel = p.level; }
            }
            else foundLevel = Level.Find(lvl, world);

            if (foundLevel == null) { Player.SendMessage(p, "Impossible de trouver la map."); return; }

            Player.SendMessage(p, "Map : &b" + foundLevel.name + Server.DefaultColor + ", est situe dans le monde &b" + foundLevel.world ) ;
            Player.SendMessage(p, "Largeur=" + foundLevel.width.ToString() + " Hauteur=" + foundLevel.depth.ToString() + " longueur=" + foundLevel.height.ToString());

            switch (foundLevel.physics)
            {
                case 0: Player.SendMessage(p, "Les physics sont &cOFF"); break;
                case 1: Player.SendMessage(p, "Les physics sont &aNormal"); break;
                case 2: Player.SendMessage(p, "Les physics sont &aAdvance"); break;
                case 3: Player.SendMessage(p, "Les physics sont &aHardcore"); break;
                case 4: Player.SendMessage(p, "Les physics sont &aInstant"); break;
            }

            if (foundLevel.perbuild)
            {
                Player.SendMessage(p, "Tout le monde peut build dans cette map");
            }
            else
            {
                string perbuildliste = "" ;
                if (foundLevel.world != "main")
                {  perbuildliste = foundLevel.world; }

                foreach(string n in foundLevel.Perbuildliste)
                { perbuildliste += ", " + n; }

                if (perbuildliste != "")
                {
                    if (foundLevel.world == "main"){ perbuildliste = perbuildliste.Remove(0, 2); }
                    Player.SendMessage(p, "Joueurs pouvant construire dans cette map : &b" + perbuildliste);
                }
                else Player.SendMessage(p, "Personne ne peut construire dans cette map");
            }
            if (foundLevel.pervisit)
            { Player.SendMessage(p, "La map est visitable par tout le monde"); }
            else
            { Player.SendMessage(p, "La map n'est pas visitable"); }

            if (!foundLevel.pergun)
                Player.SendMessage(p, "Le gun n'est pas utilisable dans cette map");
            
            if (Directory.Exists(@Server.backupLocation + "/" + foundLevel.world + "/" + foundLevel.name))
            {
                int latestBackup = Directory.GetDirectories(@Server.backupLocation + "/" + foundLevel.world + "/" + foundLevel.name).Length;
                Player.SendMessage(p, "Dernier backup: &a" + latestBackup + Server.DefaultColor + " at &a" + Directory.GetCreationTime(@Server.backupLocation + "/" + foundLevel.world + "/" + foundLevel.name + "/" + latestBackup).ToString("yyyy-MM-dd HH:mm:ss")); // + Directory.GetCreationTime(@Server.backupLocation + "/" + latestBackup + "/").ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                Player.SendMessage(p, "Il n'existe pas de backup pour cette map.");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/mapinfo <map> <monde> - Donne des detailles sur une map");
            Player.SendMessage(p, "Si Le monde n'est pas marque, le monde du joueur sera utilise");
        }
    }
}