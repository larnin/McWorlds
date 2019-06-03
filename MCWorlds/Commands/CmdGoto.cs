using System;
using System.IO;


namespace MCWorlds
{
    public class CmdGoto : Command
    {
        public override string name { get { return "goto"; } }
        public override string shortcut { get { return "g"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdGoto() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            string lvl = "";
            string world = "";
            if (message.IndexOf(' ') == -1)
            { lvl = message; world = p.level.world; }
            else
            { lvl = message.Split(' ')[0]; world = message.Split(' ')[1]; }

            try
            {
                Level foundLevel = Level.Find(lvl, world);
                if (foundLevel != null)
                {
                    Level startLevel = p.level;
                    string startworld = p.level.world;

                    GC.Collect();

                    if (p.level == foundLevel) { Player.SendMessage(p, "Vous etes deja dans \"" + foundLevel.name + "\"."); return; }
                    if (!p.ignorePermission)
                        if (!foundLevel.pervisit && p.group.Permission < LevelPermission.Admin && p.name.ToLower() != foundLevel.world.ToLower()) { Player.SendMessage(p, "Vous n'etes pas autorise a aller dans " + foundLevel.name + "."); return; }

                    p.Loading = true;
                    foreach (Player pl in Player.players) if (p.level == pl.level && p != pl ) p.SendDie(pl.id);
                    foreach (PlayerBot b in PlayerBot.playerbots) if (p.level == b.level) p.SendDie(b.id);

                    Player.GlobalDie(p, true);
                    p.level = foundLevel; p.SendUserMOTD(); p.SendMap();

                    p.perbuild = false;

                    if (p.name.ToLower() == p.level.world.ToLower())
                    { p.perbuild = true; }

                    if (p.level.perbuild)
                    { p.perbuild = true; }
                    else
                    {
                        foreach (string n in p.level.Perbuildliste)
                        {
                            if (p.name.ToLower() == n.ToLower())
                            { p.perbuild = true; }
                        }
                    }

                    GC.Collect();

                    ushort x = (ushort)((0.5 + foundLevel.spawnx) * 32);
                    ushort y = (ushort)((1 + foundLevel.spawny) * 32);
                    ushort z = (ushort)((0.5 + foundLevel.spawnz) * 32);

                    if (!p.hidden) Player.GlobalSpawn(p, x, y, z, foundLevel.rotx, foundLevel.roty, true);
                    else unchecked { p.SendPos((byte)-1, x, y, z, foundLevel.rotx, foundLevel.roty); }

                    foreach (Player pl in Player.players)
                        if (pl.level == p.level && p != pl && !pl.hidden)
                        {
                            if (pl.skin != "") { p.SendSpawn(pl.id, pl.color + pl.skin, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]); }
                            else { p.SendSpawn(pl.id, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]); }
                        }

                    foreach (PlayerBot b in PlayerBot.playerbots)
                        if (b.level == p.level)
                            p.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);

                    if (!p.hidden)
                    {
                        foreach (Player pl in Player.players)
                        {
                            if (pl.level.world == foundLevel.world) { Player.SendMessage(pl, p.color + "*" + p.Name() + Server.DefaultColor + " va sur la map &b" + foundLevel.name); }
                            if (pl.level.world == startLevel.world && foundLevel.world != startLevel.world) { Player.SendMessage(pl, p.color + "*" + p.Name() + Server.DefaultColor + " vas sur la map &b" + foundLevel.name + Server.DefaultColor + " du monde &b" + world); }
                        }
                    }

                    p.Loading = false;

                    bool skipUnload = false;
                    if (startLevel.unload && !startLevel.name.Contains("&cMuseum "))
                    {
                        foreach (Player pl in Player.players) if (pl.level == startLevel) skipUnload = true;
                        if (!skipUnload && Server.AutoLoad) startLevel.Unload();
                    }

                    if (p.level.name == "spawn")
                    {
                        if (!File.Exists("levels/" + p.level.world.ToLower() + "/bienvenu.txt"))
                        {
                            StreamWriter SW = new StreamWriter(File.Create("levels/" + p.name.ToLower() + "/bienvenu.txt"));
                            SW.WriteLine("Bienvenue dans votre monde");
                            SW.WriteLine("Ici vous pouvez faire tout ce que vous voulez");
                            SW.WriteLine("Pour modifier ce message, utilisez la fonction /bienvenue");
                            SW.Flush();
                            SW.Close();
                        }

                        foreach (string line in File.ReadAllLines("levels/" + p.level.world.ToLower() + "/bienvenu.txt"))
                        {
                            if (line != "") { Player.SendMessage(p, line); }
                        }
                    }
                }
                else if (Server.AutoLoad)
                {
                    Command.all.Find("load").Use(p, lvl + " " + world);
                    foundLevel = Level.Find(lvl, world);
                    if (foundLevel != null) Use(p, message);
                }
                else
                {
                    if (p.level.world == world )
                    Player.SendMessage(p, "Il n'y a pas de map \"" + lvl + "\" dans ce monde.");
                    else Player.SendMessage(p, "Il n'y a pas de map \"" + lvl + "\" dans le monde \"" + world + "\".");
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/goto [map] - Permet d'aller sur une autre map du monde actuel");
            Player.SendMessage(p, "/goto [map] [monde] - Permet d'aller sur une map d'un autre monde");
        }
    }
}