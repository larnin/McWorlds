using System;
using System.IO;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdWorlds : Command
    {
        public override string name { get { return "worlds"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdWorlds() { }

        public override void Use(Player p, string message)
        {
            try
            {
                string worldListe = "";

                DirectoryInfo di = new DirectoryInfo("levels/");
                DirectoryInfo[] dir = di.GetDirectories();

                if (message == "")
                {
                    foreach (DirectoryInfo dirWorld in dir)
                    { worldListe += ", " + dirWorld.Name; }
                    worldListe = worldListe.Remove(0, 2);

                    Player.SendMessage(p, "Il y a " + dir.Length + " mondes");
                    Player.SendMessage(p, worldListe);
                    Player.SendMessage(p, "Utilisez /worlds <1/2/3/..> pour une liste plus structure");
                    return;
                }
                else if (message.IndexOf(' ') == -1)
                {
                    int page = 0;
                    try { page = int.Parse(message); }
                    catch { Player.SendMessage(p, "Page introuvable"); return; }

                    if (page < 1)
                    { Player.SendMessage(p, "Valeur incorrecte"); return; }

                    int maxWorld = page * 50;
                    int minWorld = maxWorld - 49;
                    if (minWorld > dir.Length)
                    { Player.SendMessage(p, "Pas de monde au dessus de " + dir.Length); return; }

                    if (dir.Length < maxWorld) { maxWorld = dir.Length; }

                    for (int i = minWorld; i <= maxWorld; i++)
                    { worldListe += ", " + dir[i - 1].Name; }
                    worldListe = worldListe.Remove(0, 2);

                    Player.SendMessage(p, "Mondes (de " + minWorld + " a " + maxWorld + ")");
                    Player.SendMessage(p, worldListe);

                    return;
                }
                else if (message.Split(' ').Length == 2 && message.Split(' ')[0] == "short")
                {
                    int nbWorlds = 0;
                    string worldsName = "";

                    foreach (DirectoryInfo dirWorld in dir)
                    {
                        if (dirWorld.Name.ToLower().IndexOf(message.Split(' ')[1].ToLower()) != -1)
                        {
                            nbWorlds++;
                            worldsName += ", " + dirWorld.Name;
                        }
                    }

                    if (nbWorlds == 0)
                    { Player.SendMessage(p, "Il n'existe pas de mondes dont le nom contient \"" + message.Split(' ')[1] + "\""); }
                    else
                    {
                        if ( nbWorlds == 1 )
                        { Player.SendMessage(p, "Il y a un monde dont le nom contient \"" + message.Split(' ')[1] + "\""); }
                        else
                        { Player.SendMessage(p, "Il y a " + nbWorlds + " monde dont le nom contient \"" + message.Split(' ')[1] + "\""); }

                        Player.SendMessage(p, "&c" + worldsName.Remove(0,2));
                    }
                }
                else
                { Help(p); return; }
            }
            catch (Exception e) { Server.ErrorLog(e); Player.SendMessage(p, "Erreur"); }
            //Exception catching since it needs to be tested on Ocean Flatgrass
        }
        public override void Help(Player p, string message = "")
        {

            Player.SendMessage(p, "/worlds - Liste tous les mondes cree.");
            Player.SendMessage(p, "/worlds <1/2/3/..> - Pour avoir une liste plus structure.");
            Player.SendMessage(p, "/worlds short [param] - Liste tous les mondes dont le nom contient param");
            Player.SendMessage(p, "&cAttention, avec le systeme actuel, il y a beaucoup de mondes cree !");
        }
    }
}