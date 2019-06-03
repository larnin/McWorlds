using System;
using System.Collections.Generic;
using System.IO;
using System.Data;

namespace MCWorlds
{
    public abstract class BaseGame
    {
        public static List<string> NameGames = new List<string>(new string[] { "CTF", "Bomberman", "infection", "foot" });
        public List<string> cmdAllow = new List<string>();
        public Level lvl;
        public string typeGame = "base";

        public string name = "";

        public bool gameOn = false;

        public Player owner = null;

        public bool cheat = false;

        public abstract void loadGame(Player p, string file);
        public abstract void saveGame(Player p, string file);
        public abstract void startGame(Player p);
        public abstract void stopGame(Player p);
        public abstract void deleteGame(Player p);
        public abstract bool changebloc(Player p, byte b, ushort x, ushort y, ushort z, byte action);
        public abstract bool checkPos(Player p, ushort x, ushort y, ushort z);
        public abstract void death(Player p);

        public static void resetPlayer(string pname, string typegame)
        { MySQL.executeQuery("DELETE FROM " + typegame + " WHERE Name='" + pname + "'"); }

        public static void resetGame(string typegame)
        { MySQL.executeQuery("DELETE FROM " + typegame); }

        public static bool addGame(BaseGame game)
        {
            if (Server.allGames.Find(g => game.lvl == g.lvl) != null)
            { return false; }
            Server.allGames.Add(game);
            return true;
        }

        public static bool removeGame(BaseGame game)
        {
            if (Server.allGames.Find(g => game.lvl == g.lvl) == null)
            { return false; }
            return Server.allGames.Remove(game);
        } 

        public static void listSave(Player p, string game)
        {
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/games")) { Directory.CreateDirectory("extra/games"); }
            if (!Directory.Exists("extra/games/" + game)) { Directory.CreateDirectory("extra/games/" + game); }

            DirectoryInfo di = new DirectoryInfo("extra/games/" + game);
            FileInfo[] fi = di.GetFiles("*.txt");

            string liste = "";
            foreach (FileInfo file in fi)
            { liste += file.Name.Replace(".txt", "") + ", "; }

            if (liste == "")
            { Player.SendMessage(p, "Il y a aucune sauvegarde"); }
            else { Player.SendMessage(p, "Configurations disponibles : " + liste.Remove(liste.Length - 2)); }
        }

        public static void giveAward(Player p, string awardName)
        {


        }

        public void abort(Player p)
        {
            if (cheat){return;}
            Command.all.Find("abort").Use(p, "");
            if (p.hidden) { Command.all.Find("hide").Use(p, ""); }
            p.isFlying = false;
        }
        
        public void loadCmds()
        {
            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/games")) { Directory.CreateDirectory("extra/games"); }
            if (!Directory.Exists("extra/games/" + typeGame.ToLower())) { return; }
            if (!File.Exists("extra/games/" + typeGame.ToLower() + "/default.cmds")) { return; }

            cmdAllow.Clear();

            string[] lignes = File.ReadAllLines("extra/games/" + typeGame.ToLower() + "/default.cmds");

            foreach (string l in lignes)
            {
                if (l == "") { continue; }
                if (l[0] == '#') { continue; }
                if (Command.all.Find(l) == null) { continue; }

                if (cmdAllow.Find(cmd => cmd == l) == null)
                { cmdAllow.Add(l); }
            }
        }
    }
}
