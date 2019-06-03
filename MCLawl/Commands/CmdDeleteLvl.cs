using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdDeleteLvl : Command
    {
        public override string name { get { return "deletelvl"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdDeleteLvl() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!Directory.Exists("levelsdeleted")) Directory.CreateDirectory("levelsdeleted");

            if (p.group.Permission < LevelPermission.Admin && p.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Vous ne pouvez pas supprimer de map dans ce monde"); return; }

            if (message.Split(' ').Length != 1) { Help(p); return; }
            string lvl = "";
            string world = "";
            lvl = message; world = p.level.world; 
            
            if (lvl == "spawn")
            { Player.SendMessage(p, "Impossible de suppimer votre spawn."); return; }

            Level foundLevel = Level.Find(lvl,world);
            if (foundLevel == Server.mainLevel) { Player.SendMessage(p, "Impossible de suppimer la map principale."); return; }
            if (foundLevel != null) foundLevel.Unload();

            try
            {
                if (!Directory.Exists("levelsdeleted/" + world)) Directory.CreateDirectory("levelsdeleted/" + world);

                if (File.Exists("levels/" + world + "/" + lvl + ".lvl"))
                {
                    if (File.Exists("levelsdeleted/" + world + "/" + lvl + ".lvl"))
                    {
                        int currentNum = 0;
                        while (File.Exists("levelsdeleted/" + world + "/" + lvl + currentNum + ".lvl")) currentNum++;

                        File.Move("levels/" + world + "/" + lvl + ".lvl", "levelsdeleted/" + world + "/" + lvl + currentNum + ".lvl");
                    }
                    else
                    {
                        File.Move("levels/" + world + "/" + lvl + ".lvl", "levelsdeleted/" + world + "/" + lvl + ".lvl");
                    }
                    Player.SendMessage(p, "Sauvegarde cree.");

                    try { File.Delete("levels/" + world + "/level properties/" + lvl + ".properties"); }
                    catch { }
                    try { File.Delete("levels/" + world + "/level properties/" + lvl); }
                    catch { }

                    MySQL.executeQuery("DROP TABLE `Block" + lvl + "." + world + "`");
                    MySQL.executeQuery("DROP TABLE `Portals" + lvl + "." + world + "`");
                    MySQL.executeQuery("DROP TABLE `Messages" + lvl + "." + world + "`");
                    MySQL.executeQuery("DROP TABLE `Like" + lvl + "." + world + "`");

                    foreach (Player pl in Player.players)
                    {
                        if (pl.level.world == world) { Player.SendMessage(pl, "Map \"" + lvl + "\" supprimee"); }
                    }

                    if (p.level.world.ToLower() == p.name.ToLower()) { p.nbMaps--; }
                    else
                    {
                        Player who = Player.Find(p.level.world.ToLower());
                        if (who != null) { who.nbMaps--; }
                    }
                }
                else
                {
                    Player.SendMessage(p, "Impossible de trouver la map.");
                }
            }
            catch (Exception e) { Player.SendMessage(p, "Erreur."); Server.ErrorLog(e); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/deletelvl [map] - Supprime une map");
            Player.SendMessage(p, "Une sauvegarde est place dans le dossier levels/deleted");
        }
    }
}