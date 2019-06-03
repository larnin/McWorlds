using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdLevels : Command
    {
        public override string name { get { return "levels"; } }
        public override string shortcut { get { return "maps"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdLevels() { }

        public override void Use(Player p, string message)
        { // TODO
            try
            {
                if (p == null && message == "")
                { message = "all"; }

                if (message.IndexOf(' ') != -1 && message != "") { Help(p); return; }

                string maps = "" ;

                if (message == "all")
                {
                    for (int i = 0 ; i < Server.levels.Count; i++)
                    {
                        if (p != null)
                        {
                            if (Server.levels[i].pervisit || !Server.levels[i].pervisit && (p.name.ToLower() == p.level.world.ToLower() || p.group.Permission > LevelPermission.Admin))
                            { maps += ", &2" + Server.levels[i].name + " (" + Server.levels[i].world + ",&b[" + Server.levels[i].physics + "]&2)"; }
                            else
                            { maps += ", &c" + Server.levels[i].name + " (" + Server.levels[i].world + ",&b[" + Server.levels[i].physics + "]&c)"; }
                        }
                        else { maps += ", " + Server.levels[i].name + " (" + Server.levels[i].world + ",[" + Server.levels[i].physics + "])"; }

                    }
                    maps = maps.Remove(0, 2);
                    Player.SendMessage(p, "Maps charges: " + maps);
                }
                else
                {
                    if (message == "")
                    { message = p.level.world; }

                    if (!Directory.Exists("levels/" + message.ToLower() + "/"))
                    { Player.SendMessage(p, "Le monde \"" + message.ToLower() + "\" n'existe pas"); return; }
                    
                    DirectoryInfo di = new DirectoryInfo("levels/" + message.ToLower() + "/");
                    FileInfo[] fi = di.GetFiles("*.lvl");

                    string mapsUnloads = "&c";
                    
                    bool load = false ; 
                    
                    foreach (FileInfo file in fi)
                    {
                        load = false;
                        for ( int i = 0 ; i < Server.levels.Count ; i++)
                        {
                            if (file.Name.Replace(".lvl", "").ToLower() == Server.levels[i].name.ToLower() && message.ToLower() == Server.levels[i].world.ToLower() && !load)
                            {
                                maps += "&2, " + Server.levels[i].name + "&b[" + Server.levels[i].physics + "]";
                                load = true;
                            }
                        }
                        if (!load)
                        {mapsUnloads += file.Name.Replace(".lvl", "") + ", ";}
                    }

                    
                   

                    Player.SendMessage(p, "Monde : " + message);
                    if (maps != "")
                    {
                        maps = maps.Remove(2, 2);
                        Player.SendMessage(p, "Maps chargees : " + maps);
                    }
                    if (mapsUnloads != "&c")
                    {
                        mapsUnloads = mapsUnloads.Remove(mapsUnloads.Length-2);
                        Player.SendMessage(p, "Maps non chargees : " + mapsUnloads);
                    }
                }
                    
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/levels - Liste les maps du monde actuel");
            Player.SendMessage(p, "/levels [monde] - Liste toutes les maps du monde.");
            Player.SendMessage(p, "/levels all - Liste toutes les maps charges");
        }
    }
}