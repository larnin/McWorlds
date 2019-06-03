
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdPlayers : Command
    {

        public override string name { get { return "players"; } }
        public override string shortcut { get { return "who"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPlayers() { }

        struct groups { public Group group; public List<string> players; }
        public override void Use(Player p, string message)
        {
            try
            {
                List<groups> playerList = new List<groups>();

                foreach (Group grp in Group.GroupList)
                {
                    if (grp.name != "nobody")
                    {
                        groups groups;
                        groups.group = grp;
                        groups.players = new List<string>();
                        playerList.Add(groups);
                    }
                }

                int totalPlayers = 0;
                foreach (Player pl in Player.players)
                {
                    if (!pl.hidden || p.group.Permission > LevelPermission.Builder || Server.devs.Contains(p.name.ToLower()))
                    {
                        totalPlayers++;
                        string foundName = pl.Name();

                        if (Server.afkset.Contains(pl.name))
                        {
                            foundName += "-afk";
                        }

                        if (Server.devs.Contains(pl.name.ToLower()))
                        {
                            if (pl.voice)
                                playerList.Find(grp => grp.group == pl.group).players.Add("&f+[Dev]" + pl.group.color + foundName + " (" + pl.level.name + "," + pl.level.world + ")");
                            else
                                playerList.Find(grp => grp.group == pl.group).players.Add("[Dev]" + foundName + " (" + pl.level.name + "," + pl.level.world + ")");
                        }
                        else
                        {
                            if (pl.voice)
                                playerList.Find(grp => grp.group == pl.group).players.Add("&f+" + pl.group.color + foundName + " (" + pl.level.name + "," + pl.level.world + ")");
                            else
                                playerList.Find(grp => grp.group == pl.group).players.Add(foundName + " (" + pl.level.name + "," + pl.level.world + ")");
                        }  
                    }
                }
                Player.SendMessage(p, "Il y a " + totalPlayers + " joueurs en ligne.");

                for (int i = playerList.Count - 1; i >= 0; i--)
                {
                    groups groups = playerList[i];
                    string appendString = "";

                    foreach (string player in groups.players)
                    {
                        appendString += ", " + player;
                    }

                    if (appendString != "")
                    {
                        appendString = appendString.Remove(0, 2);
                        if (appendString.IndexOf(", ") == -1)
                        { appendString = ":" + groups.group.color + groups.group.trueName + " : " + appendString; }
                        else { appendString = ":" + groups.group.color + groups.group.trueName + "s : " + appendString; }
                    
                        Player.SendMessage(p, appendString);
                    }
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }

        public string getPlural(string groupName)
        {
            try
            {
                string last2 = groupName.Substring(groupName.Length - 2).ToLower();
                if ((last2 != "ed" || groupName.Length <= 3) && last2[1] != 's')
                {
                    return groupName + "s";
                }
                return groupName;
            }
            catch
            {
                return groupName;
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/players - Affiche tous les joueurs et leurs rangs");
        }
    }
}
