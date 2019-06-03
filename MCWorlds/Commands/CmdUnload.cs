using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdUnload : Command
    {
        public override string name { get { return "unload"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdUnload() { }

        public override void Use(Player p, string message)
        {
            if (message.ToLower() == "empty")
            {
                Boolean Empty = true;

                for (int i = 0 ; i < Server.levels.Count ; i++)
                {
                    Empty = true;
                    Player.players.ForEach(delegate(Player pl)
                    {
                        if (pl.level == Server.levels[i]) Empty = false;
                    });

                    if (Empty == true && Server.levels[i].unload)
                    {
                        Server.levels[i].Unload();
                        return;
                    }
                }
                Player.SendMessage(p, "Aucune map est vide.");
                return;
            }
            
            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            string lvl = "";
            string world = "";
            if (message.IndexOf(' ') == -1)
            { 
                lvl = message;
                if (p == null) { world = Server.mainWorld; }
                else { world = p.level.world; }
            }
            else
            { lvl = message.Split(' ')[0]; world = message.Split(' ')[1]; }
            Level level = Level.Find(lvl,world);

            if (level != null)
            {
                if (!level.Unload()) Player.SendMessage(p, "Vous ne pouvez pas decharger la map principale.");
                return;
            }

            Player.SendMessage(p, "Il n'y a pas de map \"" + lvl + "\" charge.");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/unload [map] <monde> - Decharge une map.");
            Player.SendMessage(p, "/unload empty - Decharge une map sans joueur.");
        }
    }
}