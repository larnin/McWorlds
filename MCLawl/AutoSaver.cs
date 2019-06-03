using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.ComponentModel;


namespace MCWorlds
{
    class AutoSaver
    {
        static int _interval;
        string backupPath = @Server.backupLocation;

        static int count = 1;
        public AutoSaver(int interval)
        {
            _interval = interval * 1000;

            Thread runner;
            runner = new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    Thread.Sleep(_interval);
                    Server.ml.Queue(delegate
                    {
                        Run();
                    });

                    if (Player.players.Count > 0)
                    {
                        string allCount = "";
                        foreach (Player pl in Player.players) allCount += ", " + pl.name;
                        try { Server.s.Log("!PLAYERS ONLINE: " + allCount.Remove(0, 2), true); }
                        catch { }

                        allCount = "";
                        for (int i = 0; i < Server.levels.Count; i++) { allCount += ", " + Server.levels[i].name; }
                        try { Server.s.Log("!LEVELS ONLINE: " + allCount.Remove(0, 2), true); }
                        catch { }
                    }
                }
            }));
            runner.Start();
        }

        public static void Run()
        {
            try
            {
                count--;

                for (int i = 0; i < Server.levels.Count; i++)
                {
                    try
                    {
                        if (Server.levels[i].changed)
                        {
                            Server.levels[i].Save();
                            if (count == 0)
                            {
                                int backupNumber = Server.levels[i].Backup();

                                if (backupNumber != -1)
                                {
                                    Server.levels[i].ChatLevel("Backup " + backupNumber + ".");
                                    Server.s.Log("Backup " + backupNumber + " saved for " + Server.levels[i].name);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Player.GlobalMessageOps("Erreur de sauvegarde de " + Server.levels[i].name);
                        Server.s.Log("Backup for " + Server.levels[i].name + " has caused an error.");
                    }
                }

                if (count <= 0)
                {
                    count = 15;
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }

            try
            {
                if (Player.players.Count > 0)
                {
                    List<Player> tempList = new List<Player>();
                    tempList.AddRange(Player.players);
                    foreach (Player p in tempList) { p.save(); }
                    tempList.Clear();
                }
            }
            catch (Exception e) { Server.ErrorLog(e); }
        }
    }
}
