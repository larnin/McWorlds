using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Data;

namespace MCWorlds
{
    public class InfectionGame : BaseGame
    {
        public int time = 1;
        public int nbBlocsAllow = 0;

        public int nbSurvivants = 0;

        public bool gameStarted = false;

        public DateTime startDate = DateTime.Now;

        public List<pinfo> players = new List<pinfo>();

        public System.Timers.Timer playerCheck = new System.Timers.Timer(500);
        public System.Timers.Timer timeTimer = new System.Timers.Timer(1000);
        public System.Timers.Timer sendMessagesTimer = new System.Timers.Timer(1000);

        public InfectionGame(Level l)
        {
            typeGame = "infection";

            lvl = l;

            loadCmds();

            MySQL.executeQuery("CREATE TABLE if not exists infection (Name VARCHAR(20), nbkills MEDIUMINT, nbkilled MEDIUMINT, nbsurvie MEDIUMINT, nbgames MEDIUMINT);");
        }

        public override void loadGame(Player p, string file)
        {
            if (file == "") { Player.SendMessage(p, "Le nom du fichier est vide"); return; }
            file = file.ToLower();
            if (!Player.ValidName(file)) { Player.SendMessage(p, "Le nom de ficher n'est pas valide"); return; }

            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/games")) { Directory.CreateDirectory("extra/games"); }
            if (!Directory.Exists("extra/games/" + typeGame.ToLower())) { Directory.CreateDirectory("extra/games/" + typeGame.ToLower()); }

            if (!File.Exists("extra/games/" + typeGame.ToLower() + "/" + file + ".txt")) { Player.SendMessage(p, "Le fichier " + file + " n'existe pas"); return; }

            string[] allLines = File.ReadAllLines("extra/games/" + typeGame.ToLower() + "/" + file + ".txt");

            cmdAllow.Clear();

            foreach (string line in allLines)
            {
                if (line == "") { continue; }
                if (line[0] == '#' || line.IndexOf("=") == -1) { continue; }

                string key = line.Split('=')[0].Trim();
                string value = line.Split('=')[1].Trim();

                switch (key)
                {
                    case "time":
                        try { time = int.Parse(value); }
                        catch { time = 1; }
                        break;
                    case "blocks":
                        try { nbBlocsAllow = int.Parse(value); }
                        catch { nbBlocsAllow = 0; }
                        break;
                    case "commandes":
                        if (value.Split(' ').Length == 0) { continue; }
                        foreach (string s in value.Split(' '))
                        { cmdAllow.Add(s); }
                        break;
                    default:
                        continue;
                }
            }
            Player.SendMessage(p, "Chargement des configurations termine");
        }

        public override void saveGame(Player p, string file)
        {
            if (file == "") { Player.SendMessage(p, "Le nom du fichier est vide"); return; }
            file = file.ToLower();
            if (!Player.ValidName(file)) { Player.SendMessage(p, "Le nom de ficher n'est pas valide"); return; }

            if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
            if (!Directory.Exists("extra/games")) { Directory.CreateDirectory("extra/games"); }
            if (!Directory.Exists("extra/games/" + typeGame.ToLower())) { Directory.CreateDirectory("extra/games/" + typeGame.ToLower()); }

            StreamWriter SW = new StreamWriter(File.Create("extra/games/" + typeGame.ToLower() + "/" + file + ".txt"));

            SW.WriteLine("time = " + time);
            SW.WriteLine("blocks = " + nbBlocsAllow);

            string cmds = "";

            foreach (string cmd in cmdAllow)
            { cmds += cmd + " "; }
            if (cmds != "") { SW.WriteLine("commandes = " + cmds.Trim()); }

            SW.Flush();
            SW.Close();
            SW.Dispose();

            Player.SendMessage(p, "Les configurations de l'infection sont sauvegarde dans " + file);
        }

        public override void startGame(Player p)
        {
            gameOn = true;
            gameStarted = false;

            foreach (Player pl in Player.players)
            {
                if (pl.level != lvl) { continue; }
                addPlayer(pl, false);
                Command.all.Find("spawn").Use(pl, "");
            }

            bool inGame = false;
            playerCheck.Start();
            playerCheck.Elapsed += delegate
            {
                for (int i = 0; i < Player.players.Count; i++)
                {
                    if (Player.players[i].level != lvl) { continue; }

                    inGame = false;

                    foreach (pinfo pi in players)
                    { if (pi.p == Player.players[i]) { inGame = true; } }
                    if (inGame) { continue; }

                    addPlayer(Player.players[i],gameStarted);
                }

                List<pinfo> stored = new List<pinfo>();
                for (int i = 0; i < players.Count; i++)
                {
                    stored.Add(players[i]);
                }
                foreach (pinfo pi in stored)
                { 
                    if (pi.p.disconnected) { players.Remove(pi); }
                    if (pi.p.level != lvl) { delPlayer(pi); }
                }
            };

            sendMessagesTimer.Start();
            sendMessagesTimer.Elapsed += delegate
            {
                for (int i = 0; i < players.Count; i++)
                { players[i].p.sendAll(); }
            };

            Thread startThread = new Thread(new ThreadStart(delegate
            {
                startDate = DateTime.Now.AddSeconds(30);
                while (!gameStarted && gameOn)
                {
                    if (DateTime.Now >= startDate)
                    { run(p); }

                    for (int i = 0; i < players.Count; i++)
                    {
                        if ((DateTime.Now - startDate).TotalSeconds % 5 == 0 || (DateTime.Now - startDate).TotalSeconds < 5)
                        {
                            players[i].p.addMessage("&eDebut de la partie dans &2" + (int)((startDate - DateTime.Now).TotalSeconds) + Server.DefaultColor + " secondes", true, 1);
                            players[i].p.addMessage("&eChoix de l'infecte en cours ...", true, 2);
                        }
                    }
                    Thread.Sleep(500);
                }
            })); startThread.Start();
        }

        public override void stopGame(Player p)
        {
            gameOn = false;
            gameStarted = false;

            List<pinfo> stored = new List<pinfo>();
            for (int i = 0; i < players.Count; i++)
            {
                stored.Add(players[i]);
            }
            foreach (pinfo pi in stored)
            {
                if (pi.zombie)
                {
                    Player.GlobalDie(pi.p, false);
                    Player.GlobalSpawn(pi.p, pi.p.pos[0], pi.p.pos[1], pi.p.pos[2], pi.p.rot[0], pi.p.rot[1], false);
                }
                delPlayer(pi);
            }

            playerCheck.Stop();
            timeTimer.Stop(); 
            sendMessagesTimer.Stop();
        }

        public override void deleteGame(Player p)
        {
            removeGame(this);
            Player.GlobalMessageLevel(lvl, "Infection desactive");

            playerCheck.Close();
            timeTimer.Close();
            sendMessagesTimer.Close();
        }

        public override bool changebloc(Player p, byte type, ushort x, ushort y, ushort z, byte action)
        {
            if (!gameOn) { return true; }

            byte b = lvl.GetTile(x, y, z);
            
            if (nbBlocsAllow == 0)
            {
                p.SendBlockchange(x, y, z, b);
                Player.SendMessage(p, "Impossible de placer ca ici");
                return false;
            }

            if (Block.OPBlocks(b))
            {
                p.SendBlockchange(x, y, z, b);
                Player.SendMessage(p, "Impossible de placer ca ici");
                return false;
            }

            if (Block.RightClick(Block.Convert(b)))
            {
                p.SendBlockchange(x, y, z, b);
                return false;
            }

            if (action == 0)
            {
                lvl.Blockchange(x, y, z, Block.air);
                return false;
            }
            else
            {
                pinfo pi = players.Find(pin => pin.p == p);
                if (pi == null) { return true; }

                if (!cheat)
                {
                    if (pi.lastBloc[0] == x && pi.lastBloc[2] == z)
                    {
                        if (pi.lastBloc[1] == y || pi.lastBloc[1] == y - 1)
                        {
                            pi.nbWarning++;
                            if (pi.nbWarning < 3)
                            { Player.SendMessage(p, "Les piliers interdits !"); }
                            else if (pi.nbWarning == 3)
                            {
                                Player.SendMessage(p, "&4Attention, kick a la prochaine alerte !");
                            }
                            else { p.Kick("Kick auto : les piliers sont interdit !"); }
                        }
                    }
                }

                if (pi.nbBlocs == 0)
                {
                    p.SendBlockchange(x, y, z, b);
                    Player.SendMessage(p, "Vous n'avez plus de blocs");
                    return false;
                }

                pi.nbBlocs--;
                if (pi.nbBlocs % 10 == 0)
                { Player.SendMessage(p, "Blocs restant : &2" + pi.nbBlocs); }
                else if (pi.nbBlocs <= 5)
                { Player.SendMessage(p, "Blocs restant : &2" + pi.nbBlocs); }

                lvl.Blockchange(x, y, z, type);
                return false;
            }
        }

        public override bool checkPos(Player p, ushort x, ushort y, ushort z)
        {
            if (!gameOn || !gameStarted) { return true; }

            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null) { return true; }

            if (pi.zombie)
            {
                double dist = 0;

                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].p == p) { continue; }
                    if (players[i].zombie) { continue; }

                    dist = Math.Sqrt((p.pos[0] - players[i].p.pos[0]) * (p.pos[0] - players[i].p.pos[0]) + (p.pos[2] - players[i].p.pos[2]) * (p.pos[2] - players[i].p.pos[2]));

                    if (dist < 64)
                    {
                        if (Math.Abs(p.pos[1] - players[i].p.pos[1]) < 64)
                        { infecter(players[i], pi); }
                    }
                }
            }
            else
            {
                double dist = 0;

                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].p == p) { continue; }
                    if (!players[i].zombie) { continue; }

                    dist = Math.Sqrt((p.pos[0] - players[i].p.pos[0]) * (p.pos[0] - players[i].p.pos[0]) + (p.pos[2] - players[i].p.pos[2]) * (p.pos[2] - players[i].p.pos[2]));

                    if (dist < 64)
                    {
                        if (Math.Abs(p.pos[1] - players[i].p.pos[1]) < 64)
                        { infecter(pi, players[i]); }
                    }
                }
            }
            return true; 
        }

        public override void death(Player p)
        { }

        public static void stats(Player p, string pname)
        {
            DataTable playerDb = MySQL.fillData("SELECT * FROM infection WHERE Name='" + pname + "'");

            if (playerDb.Rows.Count == 0)
            {
                Player.SendMessage(p, "Le joueur '" + pname + " n'a pas de statistiques sur l'infection");
            }
            else
            {
                pinfo pi = new pinfo(null);
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbkills"].ToString());
                pi.nbKilled = int.Parse(playerDb.Rows[0]["nbkilled"].ToString());
                pi.nbSurvie = int.Parse(playerDb.Rows[0]["nbsurvie"].ToString());
                pi.nbGames = int.Parse(playerDb.Rows[0]["nbgames"].ToString());

                Player.SendMessage(p, "Statistiques de '" + pname + " :");
                Player.SendMessage(p, "> > Nombre de parties joue : &2" + pi.nbGames);
                Player.SendMessage(p, "> > Nombre de survie : &2" + pi.nbSurvie);
                Player.SendMessage(p, "> > Nombre de fois zombifie : &2" + pi.nbKilled);
                Player.SendMessage(p, "> > Nombre de tue : &2" + pi.nbKills);
            }
            playerDb.Dispose();
        }

        public static void top(Player p)
        {
            DataTable maxDb = MySQL.fillData("SELECT MAX(nbkills), MAX(nbkilled), MAX(nbsurvie), MAX(nbgames) FROM infection");

            int maxKill = int.Parse(maxDb.Rows[0]["MAX(nbkills)"].ToString());
            int maxKilled = int.Parse(maxDb.Rows[0]["MAX(nbkilled)"].ToString());
            int maxSurvie = int.Parse(maxDb.Rows[0]["MAX(nbsurvie)"].ToString());
            int maxGames = int.Parse(maxDb.Rows[0]["MAX(nbgames)"].ToString());
            maxDb.Dispose();

            Player.SendMessage(p, "Infection - le top :");
            DataTable nameGame = MySQL.fillData("SELECT Name FROM infection WHERE nbgames=" + maxGames);
            if (nameGame.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant participe au plus de parties : &2" + nameGame.Rows[0]["name"] + " (" + maxGames + ")"); }
            nameGame.Dispose();
            DataTable nameSurvie = MySQL.fillData("SELECT Name FROM infection WHERE nbsurvie=" + maxSurvie);
            if (nameSurvie.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus souvent survecu: &2" + nameSurvie.Rows[0]["name"] + " (" + maxSurvie + ")"); }
            nameSurvie.Dispose();
            DataTable nameKilled = MySQL.fillData("SELECT Name FROM infection WHERE nbkilled=" + maxKilled);
            if (nameKilled.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant ete zombifie le plus souvent : &2" + nameKilled.Rows[0]["name"] + " (" + maxKilled + ")"); }
            nameKilled.Dispose();
            DataTable nameKill = MySQL.fillData("SELECT Name FROM infection WHERE nbkills=" + maxKill);
            if (nameKill.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus tue : &2" + nameKill.Rows[0]["name"] + " (" + maxKill + ")"); }
            nameKill.Dispose();
        }

        private void run(Player p)
        {
            if (players.Count < 2)
            {
                Player.GlobalMessageLevel(lvl, "Lancement de la partie impossible, trop peux de joueurs");
                stopGame(p);
                return;
            }

            Random rand = new Random();
            infecter(players[rand.Next(players.Count)]);

            nbSurvivants = 0;
            for (int i = 0; i < players.Count; i++) { if (!players[i].zombie) { nbSurvivants++; } }

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].zombie) { players[i].p.addMessage("&eVous etes un &4zombie " + Server.DefaultColor + "! Tuez !", true, 1); }
                else { players[i].p.addMessage("&eVous survivez !", true, 1); }
                players[i].p.addMessage("&eSurvivants : &2 " + nbSurvivants + " " + Server.DefaultColor + "- Temps : &4" + (DateTime.Now - startDate).Minutes + ":" + (DateTime.Now - startDate).Seconds, true, 2);
            }

            gameOn = true;
            gameStarted = true;

            Thread gameThread = new Thread(new ThreadStart(delegate
            {
                while (gameStarted && gameOn)
                {
                    int min = (DateTime.Now - startDate).Minutes;
                    int sec = (DateTime.Now - startDate).Seconds;
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (sec % 5 == 0 || sec < 5)
                        {

                            if (sec < 10)
                            { players[i].p.addMessage("&eSurvivants : &2 " + nbSurvivants + " " + Server.DefaultColor + "- Temps : &4" + min + ":0" + sec, true, 2); }
                            else
                            { players[i].p.addMessage("&eSurvivants : &2 " + nbSurvivants + " " + Server.DefaultColor + "- Temps : &4" + min + ":" + sec, true, 2); }
                        }
                    }
                    Thread.Sleep(500);

                    if (startDate.AddMinutes(time) < DateTime.Now)
                    {
                        if (nbSurvivants > 0) { endGame(true); }
                        else { endGame(false); }
                    }
                }
            })); gameThread.Start();
        }

        public void endGame(bool survie) //survie == true -> des survivant
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].zombie)
                { players[i].nbSurvie++; }
                players[i].nbGames++;

                savePlayer(players[i]);
            }

            if (survie)
            {
                Player.GlobalMessageLevel(lvl, "Fin du jeu !");
                Player.GlobalMessageLevel(lvl, "&2Bravo a tous les survivants !");
                for (int i = 0; i < players.Count; i++)
                {
                    if (!players[i].zombie) { Player.GlobalMessageLevel(lvl, "&2" + players[i].p.name + " - Survivant " + players[i].nbSurvie + " fois"); }
                }
            }
            else
            {
                players.Sort(compKill);

                Player.GlobalMessageLevel(lvl, "Fin du jeu ! Aucun survivants.");
                Player.GlobalMessageLevel(lvl, "Les meilleurs zombies sont :");
                for (int i = 0; i < 3; i++)
                {
                    if (i >= players.Count) { continue; }
                    if (players[i].lastKill == 0) { continue; }
                    if (players[i].lastKill == 1) { Player.GlobalMessageLevel(lvl, "&c" + players[i].p.name + " - un seul kill"); }
                    else { Player.GlobalMessageLevel(lvl, "&c" + players[i].p.name + " &e" + players[i].lastKill + " kills !"); }
                }
            }
            stopGame(null);
        }

        public void GameInfo(Player p)
        {
            if (!gameOn)
            {
                Player.SendMessage(p, "La partie n'est pas en cour");
                Player.SendMessage(p, "Nombre de de blocs placable : " + nbBlocsAllow);
                Player.SendMessage(p, "La partie durera " + time + "minutes");
            }
            else if (!gameStarted)
            {
                Player.SendMessage(p, "Le choix du zombie est en cour ...");
                Player.SendMessage(p, "Nombre de de blocs placable : " + nbBlocsAllow);
                Player.SendMessage(p, "La partie durera " + time + "minutes");

                string pList = "";
                for (int i = 0; i < players.Count; i++)
                { pList += players[i] + ", "; }
                if (pList != "") { Player.SendMessage(p, "Joueurs : " + pList.Remove(pList.Length - 2)); }
            }
            else
            {
                Player.SendMessage(p, "La partie est lancee");
                Player.SendMessage(p, "Nombre de de blocs placable : " + nbBlocsAllow);
                int min = (DateTime.Now - startDate).Minutes;
                int sec = (DateTime.Now - startDate).Seconds;
                if (sec < 10) { Player.SendMessage(p, "Temps - " + min + ":0" + sec + " sur " + time); }
                else { Player.SendMessage(p, "Temps - " + min + ":" + sec + " sur " + time); }

                string zombies = "", survivants = "";
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].zombie) { zombies += players[i].p.name + ", "; }
                    else { survivants += players[i].p.name + ", "; }
                }
                if (zombies != "") { Player.SendMessage(p, "Zombies : " + zombies.Remove(zombies.Length - 2)); }
                if (survivants != "") { Player.SendMessage(p, "Survivants : " + survivants.Remove(survivants.Length - 2)); }
            }
        }

        public void addPlayer(Player p, bool zombie)
        {
            pinfo pi = new pinfo(p);
            pi.zombie = zombie;
            pi.nbBlocs = nbBlocsAllow;
            pi.lastKill = 0;
            pi.nbGames = 0;
            pi.nbKilled = 0;
            pi.nbKills = 0;
            pi.nbSurvie = 0;
            pi.lastBloc = new ushort[] { 0, 0, 0 };
            pi.nbWarning = 0;

            DataTable playerDb = MySQL.fillData("SELECT * FROM infection WHERE Name='" + p.name + "'");

            if (playerDb.Rows.Count == 0)
            {
                MySQL.executeQuery("INSERT INTO infection (Name, nbkills, nbkilled, nbsurvie, nbgames)" +
                    "VALUES ('" + p.name + "', " + pi.nbKills + ", " + pi.nbKilled + ", " + pi.nbSurvie + ", " + pi.nbGames + ")");
            }
            else
            {
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbkills"].ToString());
                pi.nbKilled = int.Parse(playerDb.Rows[0]["nbkilled"].ToString());
                pi.nbSurvie = int.Parse(playerDb.Rows[0]["nbsurvie"].ToString());
                pi.nbGames = int.Parse(playerDb.Rows[0]["nbgames"].ToString());
            }
            playerDb.Dispose();

            p.ingame = true;
            p.tailleBufferGame = 4;
            p.addMessage("&e---------- Infection -------------", true);
            if (zombie) { p.addMessage("&eVous etes un &4zombie " + Server.DefaultColor + "! Tuez !", true); }
            else { p.addMessage("&eVous survivez !", true); }
            p.addMessage("&eSurvivants : &2 0 " + Server.DefaultColor + "- Temps : &4" + (DateTime.Now - startDate).Minutes + ":" + (DateTime.Now - startDate).Seconds, true);
            p.addMessage("&e----------------------------------", true);

            if (zombie) { infecter(pi); }
            players.Add(pi);

            for (int i = 0; i < players.Count; i++)
            {
                if (p == players[i].p) { continue; }
                if (!players[i].zombie) { continue; }
                p.SendDie(players[i].p.id);
                p.SendSpawn(players[i].p.id, "&4_Infected_", players[i].p.pos[0], players[i].p.pos[1], players[i].p.pos[2], players[i].p.rot[0], players[i].p.rot[1]);
            }
        }

        public void delPlayer(pinfo pi)
        {
            pi.p.sendAll();
            pi.p.ingame = false;
            pi.p.tailleBufferGame = 0;
            pi.p.gameMessages.Clear();
            players.Remove(pi);
        }

        public void savePlayer(pinfo pi)
        {
            MySQL.executeQuery("UPDATE infection SET nbkills=" + pi.nbKills + ", nbkilled=" + pi.nbKilled + ", nbsurvie=" + pi.nbSurvie + ", nbgames=" + pi.nbGames + " WHERE Name='" + pi.p.name + "'");
        }

        public void infecter(pinfo cible, pinfo infecteur = null)
        {
            nbSurvivants--;

            cible.nbKilled++;
            cible.zombie = true;

            Player.GlobalDie(cible.p, false);
            for (int i = 0 ; i < players.Count; i++)
            {
                if (players[i].p != cible.p)
                {
                    players[i].p.SendDie(cible.p.id);
                    players[i].p.SendSpawn(cible.p.id, "&4_Infected_", cible.p.pos[0], cible.p.pos[1], cible.p.pos[2], cible.p.rot[0], cible.p.rot[1]); 
                }
            }

            if (infecteur == null)
            { Player.GlobalMessageLevel(lvl, cible.p.color + cible.p.name + Server.DefaultColor +" est un zombie, courez !!!"); }
            else
            {
                Player.GlobalMessageLevel(lvl, infecteur.p.color + infecteur.p.name + Server.DefaultColor + " mange " + cible.p.color + cible.p.name);
                infecteur.nbKills++;
                infecteur.lastKill++;
            }

            if (!gameStarted) { return; }

            if (nbSurvivants == 0) { endGame(false); }
        }

        private static int compKill(pinfo a, pinfo b)
        {
            if (a == null || b == null) { return 0; }
            if (a == null) { return -1; }
            if (b == null) { return 1; }

            if (a.lastKill == b.lastKill) { return 0; }
            if (a.lastKill > b.lastKill) { return -1; }
            if (a.lastKill < b.lastKill) { return 1; }
            return 0;
        }

        public class pinfo
        {
            public Player p;
            public bool zombie;
            public int lastKill;
            public int nbKills;
            public int nbSurvie;
            public int nbGames;
            public int nbKilled;
            public int nbBlocs;

            public ushort[] lastBloc;
            public int nbWarning;

            public pinfo(Player pl)
            { p = pl; }
        }
    }
}
