using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    class CmdPause : Command
    {
        public override string name { get { return "pause"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdPause() { }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length == 2 || message.Split(' ').Length > 3) { Help(p); return; }

            if (message == "")
            {
                if (p != null)
                {
                    message = p.level.name + " " + p.level.world + " 30";
                }
                else
                {
                    message = Server.mainLevel.name + " " + Server.mainWorld + " 30";
                }
            }
            int foundNum = 0; Level foundLevel;

            if (message.IndexOf(' ') == -1)
            {
                try
                {
                    foundNum = int.Parse(message);
                    if (p != null)
                    {
                        foundLevel = p.level;
                    }
                    else
                    {
                        foundLevel = Server.mainLevel;
                    }
                }
                catch
                {
                    foundNum = 30;
                    foundLevel = Level.Find(message,p.level.world);
                }
            }
            else
            {
                try
                {
                    foundNum = int.Parse(message.Split(' ')[2]);
                    string world = message.Split(' ')[1];
                    foundLevel = Level.Find(message.Split(' ')[0],world);
                }
                catch
                {
                    Player.SendMessage(p, "Entree invalide");
                    return;
                }
            }

            if (foundLevel == null)
            {
                Player.SendMessage(p, "Impossible de trouver la map entree.");
                return;
            }

            try
            {
                if (foundLevel.physPause)
                {
                    foundLevel.physResume = DateTime.Now;
                    foundLevel.physPause = false;
                    foreach (Player pl in Player.players)
                    {
                        if (pl.level.world == foundLevel.world) { Player.SendMessage(pl, "Les physics sur " + foundLevel.name + " sont reactive."); }
                    }
                }
                else
                {
                    foundLevel.physResume = DateTime.Now.AddSeconds(foundNum);
                    foundLevel.physPause = true;
                    foreach (Player pl in Player.players)
                    {
                        if (pl.level.world == foundLevel.world) { Player.SendMessage(pl, "Les physics sur " + foundLevel.name + " sont temporairement desactive."); }
                    }

                    foundLevel.physTimer.Elapsed += delegate
                    {
                        if (DateTime.Now > foundLevel.physResume)
                        {
                            foundLevel.physPause = false;
                            Player.GlobalMessage("Les physics sur " + foundLevel.name + " sont reactive.");
                            foundLevel.physTimer.Stop();
                            foundLevel.physTimer.Dispose();
                        }
                    };
                    foundLevel.physTimer.Start();
                }
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pause - Arette les physics pendant 30 sec dans votre map actuelle");
            Player.SendMessage(p, "/pause [temps] - Arette les physics dans la map actuelle");
            Player.SendMessage(p, "/pause [map] [monde] [temps] - Arette les physics dans une map");
        }
    }
}
