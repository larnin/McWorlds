using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    public class CmdLoad : Command
    {
        public override string name { get { return "load"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdLoad() { }

        public override void Use(Player p, string message)
        {
            try
            {
                if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
                int pos = message.IndexOf(' ');
                string world = "";
                string lvl = "";
                if (pos != -1)
                {
                    world = message.Substring(pos + 1).ToLower();
                    lvl = message.Substring(0, pos).ToLower();
                }
                else
                {
                    lvl = message.ToLower();
                    world = p.level.world;
                }

                for ( int i = 0 ; i < Server.levels.Count ; i++)
                {
                    if (Server.levels[i].name == lvl && Server.levels[i].world == world) { Player.SendMessage(p, lvl + " est deja charge!"); return; }
                }

                if (Server.levels.Count == Server.levels.Capacity)
                {
                    if (Server.levels.Capacity == 1)
                    {
                        Player.SendMessage(p, "Vous ne pouvez pas charger de map !");
                    }
                    else
                    {
                        Command.all.Find("unload").Use(p, "empty");
                        if (Server.levels.Capacity == 1)
                        {
                            Player.SendMessage(p, "Il y a des joueurs sur toutes les maps et il n'y a plus de place pour charger une map.");
                            return;
                        }
                    }
                }

                if (!File.Exists("levels/" + world + "/" + lvl + ".lvl"))
                {
                    if ( !Directory.Exists("levels/" + world.ToLower() + "/")){Player.SendMessage(p, "Le monde \'" + world + "\' n'existe pas"); return;}
                    string l = "";
                    bool lvlOk = true;
                    DirectoryInfo di = new DirectoryInfo("levels/" + world.ToLower() + "/");
                    FileInfo[] fi = di.GetFiles("*.lvl");
                    foreach (FileInfo f in fi)
                    {
                        if (f.Name.ToLower() == (lvl + ".lvl").ToLower() )
                        { l = f.Name.Remove(f.Name.Length - 4); lvlOk = true; break; }
                        if (f.Name.ToLower().IndexOf(lvl.ToLower()) != -1)
                        {
                            if (l == "") { l = f.Name.Remove(f.Name.Length - 4); }
                            else { lvlOk = false; }
                        }
                    }
                    if (l == "") { lvlOk = false; }
                    if (!lvlOk)
                    { Player.SendMessage(p, "La map \"" + lvl + "\" n'existe pas!"); return; }
                    else { lvl = l; }
                }

                Level level = Level.Load(lvl,world);

                if (level == null)
                {
                    if (File.Exists("levels/" + world + "/" + lvl + ".lvl.backup"))
                    {
                        Server.s.Log("Attempting to load backup.");
                        File.Copy("levels/" + world + "/" + lvl + ".lvl.backup", "levels/" + lvl + ".lvl", true);
                        level = Level.Load(lvl,world);
                        if (level == null)
                        {
                            Player.SendMessage(p, "Le backup de " + lvl + " a echoue.");
                            return;
                        }
                    }
                    else
                    {
                        Player.SendMessage(p, "Le backup de " + lvl + " n'existe pas.");
                        return;
                    }
                }

                for ( int i = 0 ; i < Server.levels.Count ; i++)
                {
                    if (Server.levels[i].name == lvl && Server.levels[i].world == world)
                    {
                        Player.SendMessage(p, lvl + " est deja charge !");
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        return;
                    }
                }

                lock (Server.levels) 
                {
                    Server.addLevel(level);
                }

                level.physThread.Start();
                foreach (Player pl in Player.players)
                {
                    if (pl.level.world == level.world) { Player.SendMessage(pl,"Map \"" + level.name + "\" chargee"); }
                }
                
            }
            catch (Exception e)
            {
                Player.GlobalMessage("Erreur");
                Server.ErrorLog(e);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/load [map]- Charge une map du monde");
            Player.SendMessage(p, "/load [map] [monde] - Charge une map du monde demande");
        }
    }
}