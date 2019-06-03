using System;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdResetLvl : Command
    {
        public override string name { get { return "resetlvl"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdResetLvl() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }

            if (p.group.Permission < LevelPermission.Admin && p.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Vous ne pouvez pas reinitialiser une map dans ce monde"); return; }

            string lvl = "", world = "", newType = "flat";
            ushort x = 0, y = 0, z = 0;
            bool newTailles = false;

            if (message.IndexOf(' ') == -1) { lvl = message; }
            else 
            { 
                lvl = message.Split(' ')[0];
                if (message.Split(' ').Length == 2) { newType = message.Split(' ')[1]; }
                else
                {
                    if (message.Split(' ').Length != 4 && message.Split(' ').Length != 5) { Help(p); return; }
                    if (message.Split(' ').Length == 6) { newType = message.Split(' ')[4]; }
                    try
                    {
                        x = Convert.ToUInt16(message.Split(' ')[1]);
                        y = Convert.ToUInt16(message.Split(' ')[2]);
                        z = Convert.ToUInt16(message.Split(' ')[3]);
                    }
                    catch { Player.SendMessage(p, "Dimentions invalides."); return; }
                    newTailles = true;
                }
            }

            switch (newType)
            {
                case "flat":
                case "pixel":
                case "island":
                case "mountains":
                case "ocean":
                case "forest":
                case "desert":
                    break;

                default:
                    Player.SendMessage(p, "Types valides: island, mountains, forest, ocean, flat, pixel, desert"); return;
            }

            if (!newTailles)
            {
                Level level = Level.Find(lvl, p.level.world);
                if (level == null)
                {
                    if (!File.Exists("levels/" + p.level.world + "/" + lvl + ".lvl")) { Player.SendMessage(p, "La map '" + lvl + "' n'existe pas"); return; }
                    level = Level.Load(lvl, p.level.world, 0);

                    world = p.level.world;
                    x = level.width;
                    y = level.depth;
                    z = level.height;

                    level.blocks = new byte[0];

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                else
                {
                    world = level.world;
                    lvl = level.name;
                    x = level.width;
                    y = level.depth;
                    z = level.height;
                    Command.all.Find("unload").Use(p, level.name);
                }
            }
            else
            {
                world = p.level.world;
                Level level = Level.Find(lvl, p.level.world);
                if (level != null) { Command.all.Find("unload").Use(p, level.name); }
            }

            try
            {
                Level l = new Level(lvl, x, y, z, newType);
                l.world = world;
                l.Save(true);
                l.blocks = new byte[0];
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            MySQL.executeQuery("DELETE FROM `Block" + lvl + "." + world + "`");
            MySQL.executeQuery("DELETE FROM `Portals" + lvl + "." + world + "`");
            MySQL.executeQuery("DELETE FROM `Messages" + lvl + "." + world + "`");
            MySQL.executeQuery("DELETE FROM `Like" + lvl + "." + world + "`");
            
            Player.SendMessage(p, "Map '" + lvl + "' reinitialise !");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/resetlvl [map] <newtype> - Reinitialise la map.");
            Player.SendMessage(p, "/resetlvl [map] [x] [y] [z] <newtype> - Remet la map a 0 avec de nouvelles tailles");
            Player.SendMessage(p, "/help newlvl types - Pour plus d'infos sur les types de maps");
        }
    }
}