using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdRenameLvl : Command
    {
        public override string name { get { return "renamelvl"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRenameLvl() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.group.Permission < LevelPermission.Admin && p.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Vous ne pouvez pas renommer de map dans ce monde"); return; }

            if (message.Split(' ').Length != 2 ) { Help(p); return; }
            string lvl = "";
            string newName = "";
            string world = "";
            lvl = message.Split(' ')[0]; newName = message.Split(' ')[1]; world = p.level.world;

            if (lvl == "spawn")
            { Player.SendMessage(p, "Impossible de renommer votre spawn."); return; }

            if (!File.Exists("levels/" + world + "/" + lvl + ".lvl")) { Player.SendMessage(p, "La map '" + lvl + "' n'existe pas"); return; }

            if (!Player.ValidName(newName)) { Player.SendMessage(p, "Nom invalide!"); return; }

            Level foundLevel = Level.Find(lvl ,world);

            if (File.Exists("levels/" + world + "/" + newName + ".lvl")) { Player.SendMessage(p, "Cette map existe deja."); return; }
            if (foundLevel == Server.mainLevel) { Player.SendMessage(p, "Impossible de renommer la map principale."); return; }
            if (foundLevel != null) foundLevel.Unload();

            try
            {
                File.Move("levels/" + world + "/" + foundLevel.name + ".lvl", "levels/" + world + "/" + newName + ".lvl");

                try
                {
                    File.Move("levels/" + world + "/level properties/" + foundLevel.name + ".properties", "levels/" + world + "/level properties/" + newName + ".properties");
                }
                catch { }
                try
                {
                    File.Move("levels/" + world + "/level properties/" + foundLevel.name, "levels/" + world + "/level properties/" + newName + ".properties");
                }
                catch { }

                MySQL.executeQuery("RENAME TABLE `Block" + foundLevel.name.ToLower() + "." + foundLevel.world + "` TO `Block" + newName.ToLower() + "." + foundLevel.world + "`");
                MySQL.executeQuery("RENAME TABLE `Portals" + foundLevel.name.ToLower() + "." + foundLevel.world + "` TO `Portals" + newName.ToLower() + "." + foundLevel.world + "`");
                MySQL.executeQuery("RENAME TABLE `Messages" + foundLevel.name.ToLower() + "." + foundLevel.world + "` TO `Messages" + newName.ToLower() + "." + foundLevel.world + "`");
                MySQL.executeQuery("RENAME TABLE `Like" + foundLevel.name.ToLower() + "." + foundLevel.world + "` TO `Like" + newName.ToLower() + "." + foundLevel.world + "`");

                Player.GlobalMessage("La map " + foundLevel.name + " du monde "+ world + " a ete renomme en " + newName);
            }
            catch (Exception e) { Player.SendMessage(p, "Erreur."); Server.ErrorLog(e); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/renamelvl [map] [nouveau nom] - renomme une map.");
            Player.SendMessage(p, "Les portails allant sur <map> seront perdu.");
        }
    }
}