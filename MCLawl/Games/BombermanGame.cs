using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Data;


namespace MCWorlds
{
    public class BombermanGame : BaseGame
    {
        public zone gameZone = new zone(); 

        public List<pinfo> players = new List<pinfo>();

        public List<pos> bonus = new List<pos>();

        public List<tntExp> explose = new List<tntExp>();

        public List<posXYZ> portails = new List<posXYZ>();

        public System.Timers.Timer playerCheck = new System.Timers.Timer(500);
        public System.Timers.Timer tntCheck = new System.Timers.Timer(100);
        public System.Timers.Timer addBonusTimer = new System.Timers.Timer(8000);
        public System.Timers.Timer saveTimer = new System.Timers.Timer(60000);

        public BombermanGame(Level l)
        {
            typeGame = "bomberman";
            lvl = l;

            gameZone.xMin = 0;
            gameZone.xMax = 0;
            gameZone.y = 0;
            gameZone.zMin = 0;
            gameZone.zMax = 0;

            loadCmds();

            MySQL.executeQuery("CREATE TABLE if not exists bomberman (Name VARCHAR(20), nbKills MEDIUMINT, nbDeaths MEDIUMINT, nbPowers MEDIUMINT, nbPoints MEDIUMINT, nbWalls MEDIUMINT);");

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
                    case "xmin":
                        try { gameZone.xMin = ushort.Parse(value); }
                        catch { gameZone.xMin = 0; }
                        break;
                    case "xmax":
                        try { gameZone.xMax = ushort.Parse(value); }
                        catch { gameZone.xMax = 0; }
                        break;
                    case "y":
                        try { gameZone.y = ushort.Parse(value); }
                        catch { gameZone.y = 0; }
                        break;
                    case "zmin":
                        try { gameZone.zMin = ushort.Parse(value); }
                        catch { gameZone.zMin = 0; }
                        break;
                    case "zmax":
                        try { gameZone.zMax = ushort.Parse(value); }
                        catch { gameZone.zMax = 0; }
                        break;
                    case "portail":
                        if (value.Split(' ').Length != 3) { continue; }
                        posXYZ po = new posXYZ();
                        try { po.x = ushort.Parse(value.Split(' ')[0]); }
                        catch { po.x = 0; }
                        try { po.y = ushort.Parse(value.Split(' ')[1]); }
                        catch { po.y = 0; }
                        try { po.z = ushort.Parse(value.Split(' ')[2]); }
                        catch { po.z = 0; }
                        portails.Add(po);
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

            SW.WriteLine("xmin = " + gameZone.xMin);
            SW.WriteLine("xmax = " + gameZone.xMax);
            SW.WriteLine("y = " + gameZone.y);
            SW.WriteLine("zmin = " + gameZone.zMin);
            SW.WriteLine("zmax = " + gameZone.zMax);

            foreach (posXYZ po in portails)
            { SW.WriteLine("portail = " + po.x + " " + po.y + " " + po.z); }

            string cmds = "";

            foreach (string cmd in cmdAllow)
            { cmds += cmd + " "; }
            if (cmds != "") { SW.WriteLine("commandes = " + cmds.Trim()); }
            
            SW.Flush();
            SW.Close();
            SW.Dispose();

            Player.SendMessage(p, "Les configurations du bomberman sont sauvegarde dans " + file);
        }

        public override void startGame(Player p)
        {
            Player.GlobalMessageLevel(lvl, "Preparation de la zone ...");

            int sizeX = gameZone.xMax - gameZone.xMin;
            int sizeZ = gameZone.zMax - gameZone.zMin;

            Random rand = new Random();

            for (int i = 0; i <= sizeX / 2; i++)
            {
                for (int j = 0; j <= sizeZ / 2; j++)
                {
                    if (gameZone.xMin + i > gameZone.xMax || gameZone.zMin + j > gameZone.zMax) { continue; }
                    if (i % 2 == 1 && j % 2 == 1) // murs
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.blackrock, true);
                            lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.blackrock, true);
                            lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.blackrock, true);
                            lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.blackrock, true);
                        }
                    }
                    else
                    {
                        lvl.Blockchange((ushort)(gameZone.xMin + i * 2), gameZone.y, (ushort)(gameZone.zMin + j * 2), Block.darkgrey);
                        lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), gameZone.y, (ushort)(gameZone.zMin + j * 2), Block.darkgrey);
                        lvl.Blockchange((ushort)(gameZone.xMin + i * 2), gameZone.y, (ushort)(gameZone.zMin + j * 2 + 1), Block.darkgrey);
                        lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), gameZone.y, (ushort)(gameZone.zMin + j * 2 + 1), Block.darkgrey);

                        if (rand.Next(20) < 4) //mur en bois
                        {
                            for (int k = 1; k < 4; k++)
                            {
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.wood);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.wood);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.wood);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.wood);
                            }
                        }
                        else // air
                        {
                            for (int k = 1; k < 3; k++)
                            {
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.air);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2), Block.air);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.air);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + k), (ushort)(gameZone.zMin + j * 2 + 1), Block.air);
                               
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + 3), (ushort)(gameZone.zMin + j * 2), Block.glass);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + 3), (ushort)(gameZone.zMin + j * 2), Block.glass);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + 3), (ushort)(gameZone.zMin + j * 2 + 1), Block.glass);
                                lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + 3), (ushort)(gameZone.zMin + j * 2 + 1), Block.glass);
                            }
                        }
                    }

                    lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + 4), (ushort)(gameZone.zMin + j * 2), Block.glass);
                    lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + 4), (ushort)(gameZone.zMin + j * 2), Block.glass);
                    lvl.Blockchange((ushort)(gameZone.xMin + i * 2), (ushort)(gameZone.y + 4), (ushort)(gameZone.zMin + j * 2 + 1), Block.glass);
                    lvl.Blockchange((ushort)(gameZone.xMin + i * 2 + 1), (ushort)(gameZone.y + 4), (ushort)(gameZone.zMin + j * 2 + 1), Block.glass);
                }
            }

            foreach (posXYZ po in portails)
            {
                lvl.Blockchange(po.x, po.y, po.z, Block.water);
                lvl.Blockchange(po.x, (ushort)(po.y - 1), po.z, Block.water);
            }

            Player.GlobalMessageLevel(lvl, "Go !!!!");

            foreach (Player pl in Player.players)
            {
                if (pl.level != lvl) { continue; }
                Command.all.Find("spawn").Use(pl,"");
                addPlayer(pl);
            }
            runGame(p);
        }

        public override void stopGame(Player p)
        {
            if (!gameOn) { return; }

            gameOn = false;
            if (p != null) { Player.GlobalMessageLevel(lvl, "Le jeu est arette par " + p.Name()); }

            List<pinfo> stored = new List<pinfo>();
            for (int i = 0; i < players.Count; i++)
            {
                stored.Add(players[i]);
            }
            foreach (pinfo pi in stored)
            {
                removePlayer(pi);
            }

            playerCheck.Stop();
            tntCheck.Stop();
            addBonusTimer.Stop();
            
        }

        public override void deleteGame(Player p)
        {
            removeGame(this);
            Player.GlobalMessageLevel(lvl, "Bomberman desactive");

            playerCheck.Close();
            tntCheck.Close();
            addBonusTimer.Close();
        }

        public override bool changebloc(Player p, byte type, ushort x, ushort y, ushort z, byte action)
        {
            if (!gameOn) { return true; }

            if (x > gameZone.xMax || x < gameZone.xMin || y > gameZone.y + 4 || y < gameZone.y || z > gameZone.zMax || z < gameZone.zMin)
            { return true; }

            byte b = lvl.GetTile(x, y, z);

            if (y != gameZone.y + 1 || action == 0 )
            {p.SendBlockchange(x, y, z, b); return false;}

            if (type != Block.tnt && type != Block.wood)
            { p.SendBlockchange(x, y, z, b); return false; }
            
            if (b != Block.air)
            { p.SendBlockchange(x, y, z, b); return false; }

            if (lvl.GetTile(x, (ushort)(y - 1), z) == Block.lava)
            { p.SendBlockchange(x, y, z, b); return false; }

            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null)
            { p.SendBlockchange(x, y, z, b); return false; }

            if (type == Block.tnt)
            {
                if (pi.tnts.Count >= pi.nbTnt)
                { p.SendBlockchange(x, y, z, b); return false; }

                tnt tInfo = new tnt(x, z);
                pi.tnts.Add(tInfo);
                lvl.Blockchange(x, y, z, Block.tnt);
            }
            else if (type == Block.wood)
            {
                if (pi.nbWalls <= 0)
                { p.SendBlockchange(x, y, z, b); return false; }
                pi.nbWalls--;
                pi.overAllWalls++;

                p.addMessage("&eMurs : " + pi.nbWalls + " - Tnt : " + pi.nbTnt + " - Puissance : " + pi.puissanceTnt, true, 1);

                lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.wood);
                lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.wood);
                lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.wood);
            }
            else { p.SendBlockchange(x, y, z, b); }

            return false;
        }

        public override bool checkPos(Player p, ushort x, ushort y, ushort z)
        {
            if (!gameOn) { return true; }

            if (portails.Exists(po => po.x == x && po.y == y && po.z == z))
            { jumpPortail(p); }
            if (portails.Exists(po => po.x == x && po.y == y - 1 && po.z == z))
            { jumpPortail(p); }

            if (x > gameZone.xMax || x < gameZone.xMin || y > gameZone.y + 2 || y < gameZone.y || z > gameZone.zMax || z < gameZone.zMin) 
            { return true; }

            byte b = lvl.GetTile(x, (ushort)(y - 1), z);
            byte b1 = lvl.GetTile(x, (ushort)(y - 2), z);

            if (b == Block.lava || b1 == Block.lava)
            {
                tntExp tExplose = explose.Find(tE => tE.x == x && tE.z == z);
                if (tExplose == null) { lvl.Blockchange(x, gameZone.y, z, Block.darkgrey); return true; }
                killPlayer(p, tExplose);
                return false;
            }
            else if (b == Block.water || b1 == Block.water)
            {
                int boIndex = bonus.FindIndex(bo => bo.x == x && bo.z == z);
                if (boIndex == -1)
                {
                    lvl.Blockchange(x, gameZone.y, z, Block.darkgrey);
                    lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.air);
                    lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.air);
                    lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.glass);
                    return true;
                }
                takeBonus(p, x, z, boIndex);
            }

            return true;
        }

        public override void death(Player p)
        {
            Command.all.Find("spawn").Use(p, "");
        }

        public static void stats(Player p, string pname)
        {
            DataTable playerDb = MySQL.fillData("SELECT * FROM bomberman WHERE Name='" + pname + "'");

            if (playerDb.Rows.Count == 0)
            {
                Player.SendMessage(p, "Le joueur '" + pname + " n'a pas de statistiques sur le bomberman"); 
            }
            else
            {
                pinfo pi = new pinfo();
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbKills"].ToString());
                pi.nbDeaths = int.Parse(playerDb.Rows[0]["nbDeaths"].ToString());
                pi.nbPowerUp = int.Parse(playerDb.Rows[0]["nbPowers"].ToString());
                pi.nbPoints = int.Parse(playerDb.Rows[0]["nbPoints"].ToString());
                pi.overAllWalls = int.Parse(playerDb.Rows[0]["nbWalls"].ToString());

                Player.SendMessage(p, "Statistiques de '" + pname + " :");
                Player.SendMessage(p, "> > Nombre de points : &2" + pi.nbPoints);
                Player.SendMessage(p, "> > Nombre de kills : &2" + pi.nbKills);
                Player.SendMessage(p, "> > Nombre de morts : &2" + pi.nbDeaths);
                Player.SendMessage(p, "> > Nombre de powers up attrape : &2" + pi.nbPowerUp);
                Player.SendMessage(p, "> > Nombre de murs pose : &2" + pi.overAllWalls);
            }
            playerDb.Dispose();
        }

        public static void top(Player p)
        {
            DataTable maxDb = MySQL.fillData("SELECT MAX(nbPoints), MAX(nbKills), MAX(nbDeaths), MAX(nbPowers), MAX(nbWalls) FROM bomberman");

            int maxKill = int.Parse(maxDb.Rows[0]["MAX(nbKills)"].ToString());
            int maxDeath = int.Parse(maxDb.Rows[0]["MAX(nbDeaths)"].ToString());
            int maxPoint = int.Parse(maxDb.Rows[0]["MAX(nbPoints)"].ToString());
            int maxPowers = int.Parse(maxDb.Rows[0]["MAX(nbPowers)"].ToString());
            int maxWalls = int.Parse(maxDb.Rows[0]["MAX(nbWalls)"].ToString());
            maxDb.Dispose();

            Player.SendMessage(p, "Bomberman - le top :"); 
            DataTable namePoints = MySQL.fillData("SELECT Name FROM bomberman WHERE nbPoints=" + maxPoint);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de points : &2" + namePoints.Rows[0]["name"] + " (" + maxPoint + ")"); }
            namePoints.Dispose();
            DataTable nameKills = MySQL.fillData("SELECT Name FROM bomberman WHERE nbKills=" + maxKill);
            if (nameKills.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de kills : &2" + nameKills.Rows[0]["name"] + " (" + maxKill + ")"); }
            nameKills.Dispose();
            DataTable nameDeaths = MySQL.fillData("SELECT Name FROM bomberman WHERE nbDeaths=" + maxDeath);
            if (nameDeaths.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de morts : &2" + nameDeaths.Rows[0]["name"] + " (" + maxDeath + ")"); }
            nameDeaths.Dispose();
            DataTable namePowers = MySQL.fillData("SELECT Name FROM bomberman WHERE nbPowers=" + maxPowers);
            if (namePowers.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant attrape le plus de powerup : &2" + namePowers.Rows[0]["name"] + " (" + maxPowers + ")"); }
            namePowers.Dispose();
            DataTable nameWalls = MySQL.fillData("SELECT Name FROM bomberman WHERE nbWalls=" + maxWalls);
            if (nameWalls.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant pose le plus de murs : &2" + nameWalls.Rows[0]["name"] + " (" + maxWalls + ")"); }
            nameWalls.Dispose();
        }

        public void runGame(Player p)
        {
            gameOn = true;

            Thread sendMessagesThread = new Thread(new ThreadStart(delegate
            {
                while (gameOn)
                {
                    for (int i = 0 ; i < players.Count ; i++)
                    { players[i].p.sendAll(); }
                    Thread.Sleep(1000);
                }
            })); sendMessagesThread.Start();

            bool inGame = false;
            playerCheck.Start();
            playerCheck.Elapsed += delegate
            {
                for( int i = 0 ; i < Player.players.Count ; i++)
                {
                    if (Player.players[i].level != lvl) { continue; }

                    inGame = false;

                    foreach (pinfo pi in players)
                    { if (pi.p == Player.players[i]) { inGame = true; } }
                    if (inGame) { continue; }

                    addPlayer(Player.players[i]);
                }

                List<pinfo> stored = new List<pinfo>();
                for (int i = 0; i < players.Count; i++)
                {
                    stored.Add(players[i]);
                }
                foreach (pinfo pi in stored)
                {
                    if (pi.p.disconnected) { players.Remove(pi); continue;}
                    if (pi.p.level != lvl) { removePlayer(pi); continue; }
                }
            };

            saveTimer.Start();
            saveTimer.Elapsed += delegate
            {
                for (int i = 0 ; i < players.Count ; i++)
                { savePlayer(players[i]); }
            };

            tntCheck.Start();
            tntCheck.Elapsed += delegate
            {
                for (int j = 0 ; j < players.Count; j++)
                {
                    if (players[j].tnts.Count == 0) { continue; }

                    for (int i = 0; i < players[j].tnts.Count; i++) 
                    {
                        players[j].tnts[i].time++;
                        if (players[j].tnts[i].time >= 20) 
                        {
                            tntExplose(players[j].tnts[i], players[j]);
                            players[j].tnts.RemoveAt(i);
                            i--;
                        }
                    }
                }

                for (int i = 0; i < explose.Count; i++)
                {
                    explose[i].time++;
                    if (explose[i].time >= 20)
                    {
                        lvl.Blockchange(explose[i].x, gameZone.y, explose[i].z, Block.darkgrey);
                        explose.RemoveAt(i);
                    }
                }
            };

            addBonusTimer.Start();
            addBonusTimer.Elapsed += delegate
            {
                if (bonus.Count < (gameZone.xMax - gameZone.xMin) * (gameZone.zMax - gameZone.zMin) / 64)
                {addBonus();}
            };
        }

        public void GameInfo(Player p)
        {
            if (gameOn) { Player.SendMessage(p, "Bomberman - Le jeu est en cour"); }
            else { Player.SendMessage(p, "Bomberman - Explosez vos adverssaires !"); }

            if (gameZone.xMax - gameZone.xMin <= 0 || gameZone.zMax - gameZone.zMin <= 0) { Player.SendMessage(p, "Zone de jeu non definie"); }
            else { Player.SendMessage(p, "Zone de jeu - Largeur : " + (gameZone.xMax - gameZone.xMin) + " - Longueur : " + (gameZone.zMax - gameZone.zMin)); }

            string plist = "";
            foreach (pinfo pi in players)
            { plist += pi.p.name + " "; }
            if (plist != "") { Player.SendMessage(p, "Joueurs : " + plist); }
        }

        public void setZone(Player p, ushort x1, ushort x2, ushort y, ushort z1, ushort z2)
        {
            gameZone.xMin = Math.Min(x1, x2);
            gameZone.xMax = Math.Max(x1, x2);
            gameZone.zMin = Math.Min(z1, z2);
            gameZone.zMax = Math.Max(z1, z2);
            gameZone.y = y;

            Player.SendMessage(p, "Zone de jeu cree");
        }

        public void addPlayer(Player p)
        {
            pinfo pi = new pinfo();

            pi.p = p;
            pi.lastKill = 0;
            pi.nbDeaths = 0;
            pi.nbKills = 0;
            pi.nbTnt = 2;
            pi.nbWalls = 4;
            pi.puissanceTnt = 4;
            pi.tnts = new List<tnt>();

            pi.nbPoints = 0;
            pi.nbPowerUp = 0;
            pi.overAllWalls = 0;

            DataTable playerDb = MySQL.fillData("SELECT * FROM bomberman WHERE Name='" + p.name + "'");

            if (playerDb.Rows.Count == 0)
            {
                MySQL.executeQuery("INSERT INTO bomberman (Name, nbKills, nbDeaths, nbPowers, nbPoints, nbWalls)" +
                    "VALUES ('" + p.name + "', " + pi.nbKills + ", " + pi.nbDeaths + ", " + pi.nbPowerUp + ", " + pi.nbPoints + ", " + pi.overAllWalls + ")");
            }
            else
            {
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbKills"].ToString());
                pi.nbDeaths = int.Parse(playerDb.Rows[0]["nbDeaths"].ToString());
                pi.nbPowerUp = int.Parse(playerDb.Rows[0]["nbPowers"].ToString());
                pi.nbPoints = int.Parse(playerDb.Rows[0]["nbPoints"].ToString());
                pi.overAllWalls = int.Parse(playerDb.Rows[0]["nbWalls"].ToString());
            }
            playerDb.Dispose();

            p.ingame = true;
            p.tailleBufferGame = 4;
            p.gameMessages.Clear();
            p.addMessage("&e---------- bomberman -------------", true);
            p.addMessage("&eMurs : " + pi.nbWalls + " - Tnt : " + pi.nbTnt + " - Puissance : " + pi.puissanceTnt, true);
            p.addMessage("&eKills : " + pi.nbKills + " - Morts : " + pi.nbDeaths + " - lastKills : " + pi.lastKill, true);
            p.addMessage("&e----------------------------------", true);
            abort(p);

            players.Add(pi);
        }

        public void savePlayer(pinfo pi)
        {
            MySQL.executeQuery("UPDATE bomberman SET nbKills=" + pi.nbKills + ", nbDeaths=" + pi.nbDeaths + ", nbPowers=" + pi.nbPowerUp + ", nbPoints=" + pi.nbPoints + ", nbWalls=" + pi.overAllWalls + " WHERE Name='" + pi.p.name + "'");
        }

        public void removePlayer(pinfo pi)
        {
            savePlayer(pi);
            pi.p.sendAll();
            players.Remove(pi);
            pi.p.ingame = false;
            pi.p.tailleBufferGame = 0;
            pi.p.gameMessages.Clear();
        }

        public void addBonus()
        {
            int nbTry = 0;

        retry:
            nbTry++;
            if (nbTry > 5) { return; }
            Random rand = new Random();

            ushort x = (ushort)rand.Next(gameZone.xMin, gameZone.xMax);
            ushort z = (ushort)rand.Next(gameZone.zMin, gameZone.zMax);

            byte b = lvl.GetTile(x,(ushort)(gameZone.y + 1), z);
            if (b == Block.wood || b == Block.water || b == Block.blackrock) { goto retry; }
            if (lvl.GetTile(x, gameZone.y, z) == Block.lava) { goto retry; }

            pos pBonus = new pos();
            pBonus.x = x;
            pBonus.z = z;
            bonus.Add(pBonus);

            lvl.Blockchange(x, gameZone.y, z, Block.iron);
            lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.water);
            lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.water);
            lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.iron);
        }

        public void addPortail(Player p, ushort x, ushort y, ushort z)
        {
            posXYZ pos = new posXYZ();
            pos.x = x;
            pos.y = y;
            pos.z = z;
            portails.Add(pos);
            Player.SendMessage(p, "Portail cree en " + x + " " + y + " " + z); 
        }

        public void removeBonus(ushort x, ushort z)
        {
            int index = bonus.FindIndex(bo => bo.x == x && bo.z == z);
            if (index == -1) { return; }

            bonus.RemoveAt(index);

            lvl.Blockchange(x, gameZone.y, z, Block.darkgrey);
            lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.air);
            lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.air);
            lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.glass);
        }

        public void tntExplose(tnt tInfo,pinfo pi)
        {
            lvl.Blockchange(tInfo.x, (ushort)(gameZone.y + 1), tInfo.z, Block.air);

            bool canExplode = true;
            for (int i = 0; i < pi.puissanceTnt && canExplode; i++)
            { canExplode = createExplosion(pi, (ushort)(tInfo.x + i), tInfo.z); }

            canExplode = true;
            for (int i = 1; i < pi.puissanceTnt && canExplode; i++)
            { canExplode = createExplosion(pi, (ushort)(tInfo.x - i), tInfo.z); }

            canExplode = true;
            for (int i = 1; i < pi.puissanceTnt && canExplode; i++)
            { canExplode = createExplosion(pi, tInfo.x, (ushort)(tInfo.z + i)); }

            canExplode = true;
            for (int i = 1; i < pi.puissanceTnt && canExplode; i++)
            { canExplode = createExplosion(pi, tInfo.x, (ushort)(tInfo.z - i)); }
        }

        public bool createExplosion(pinfo pi, ushort x, ushort z)
        {
            tntExp tE = new tntExp();
            tE.pi = pi; tE.x = x; tE.z = z;

            bool explodeNext = true;

            if (gameZone.xMin > tE.x || gameZone.xMax < tE.x || gameZone.zMin > tE.z || gameZone.zMax < tE.z) { return false; }
            byte b = lvl.GetTile(tE.x, (ushort)(gameZone.y + 1), tE.z);

            if (b == Block.blackrock) { return false; }

            if (b == Block.water) 
            { 
                removeBonus(tE.x, tE.z); 
                explodeNext = false; 
            }
            if (b == Block.wood)
            {
                lvl.Blockchange(tE.x, (ushort)(gameZone.y + 1), tE.z, Block.air);
                lvl.Blockchange(tE.x, (ushort)(gameZone.y + 2), tE.z, Block.air);
                lvl.Blockchange(tE.x, (ushort)(gameZone.y + 3), tE.z, Block.glass);
                explodeNext = false;
            }

            if (lvl.GetTile(tE.x, gameZone.y, tE.z) != Block.lava)
            {
                explose.Add(tE);
                lvl.Blockchange(tE.x, gameZone.y, tE.z, Block.lava);
            }

            if (b == Block.tnt)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].tnts.Count == 0) { continue; }

                    tnt ptnt = players[i].tnts.Find(t => t.x == tE.x && t.z == tE.z);
                    if (ptnt == null) { continue; }
                    tntExplose(ptnt, players[i]);

                }
            }
            return explodeNext;
        }

        public void killPlayer(Player p, tntExp tE)
        {
            if (p.lastDeath.AddSeconds(2) > DateTime.Now) { return; }
            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null) { return; }

            pi.lastKill = 0;
            pi.nbWalls = 4;
            pi.nbTnt = 2;
            pi.puissanceTnt = 4;
            pi.nbDeaths++;
            pi.nbPoints--;

            p.addMessage("&eMurs : " + pi.nbWalls + " - Tnt : " + pi.nbTnt + " - Puissance : " + pi.puissanceTnt, true, 1);
            p.addMessage("&eKills : " + pi.nbKills + " - Morts : " + pi.nbDeaths + " - lastKills : " + pi.lastKill, true, 2);

            if (tE.pi == null)
            { p.HandleDeath(Block.stone, "&c explose."); }
            else
            {
                if (tE.pi.p == p) { p.HandleDeath(Block.stone, "&c explose dans sa propre tnt !"); }
                else { p.HandleDeath(Block.stone, "&c explose dans la tnt de " + tE.pi.p.color + tE.pi.p.Name()); }

                if (tE.pi.p != p)
                {
                    tE.pi.nbKills++;
                    tE.pi.lastKill++;
                    tE.pi.nbPoints += tE.pi.lastKill;
                    if (tE.pi.lastKill == 5) { Player.GlobalMessageLevel(lvl, tE.pi.p.color + tE.pi.p.Name() + Server.DefaultColor + " - quintuple kills !"); }
                    if (tE.pi.lastKill == 10) { Player.GlobalMessageLevel(lvl, tE.pi.p.color + tE.pi.p.Name() + Server.DefaultColor + " - expert ! 10 kills !"); }
                    if (tE.pi.lastKill == 20) { Player.GlobalMessageLevel(lvl, tE.pi.p.color + tE.pi.p.Name() + Server.DefaultColor + " - 20 kills ! Bomberman !"); }
                    tE.pi.p.addMessage("&eKills : " + tE.pi.nbKills + " - Morts : " + tE.pi.nbDeaths + " - lastKills : " + tE.pi.lastKill, true, 2);
                }
            }
        }

        public void takeBonus(Player p, ushort x, ushort z, int index)
        {
            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null) 
            {
                lvl.Blockchange(x, gameZone.y, z, Block.darkgrey);
                lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.air);
                lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.air);
                lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.glass);
                bonus.RemoveAt(index);
                return; 
            }

            Random rand = new Random();
            switch (rand.Next(3))
            {
                case 0: //walls
                    if (pi.nbWalls + 6 < 30)
                    { pi.nbWalls += 6; }
                    else { pi.nbWalls = 30; }
                    break;
                case 1: //puissance
                    if (pi.puissanceTnt + 2 < 12)
                    { pi.puissanceTnt += 2; }
                    else { pi.puissanceTnt = 12; }
                    break;
                case 2: //tnt
                    if (pi.nbTnt + 2 < 12)
                    { pi.nbTnt += 2; }
                    else { pi.nbTnt = 12; }
                    break;
                default:
                    break;
            }

            pi.nbPowerUp++;

            p.addMessage("&eMurs : " + pi.nbWalls + " - Tnt : " + pi.nbTnt + " - Puissance : " + pi.puissanceTnt, true, 1);

            lvl.Blockchange(x, gameZone.y, z, Block.darkgrey);
            lvl.Blockchange(x, (ushort)(gameZone.y + 1), z, Block.air);
            lvl.Blockchange(x, (ushort)(gameZone.y + 2), z, Block.air);
            lvl.Blockchange(x, (ushort)(gameZone.y + 3), z, Block.glass);
            bonus.RemoveAt(index);
            return;
        }

        public void jumpPortail(Player p)
        {
            Random rand = new Random();
            ushort x, z;
            int nbTry = 0;
        retry:
            nbTry++;

            if (nbTry > 10) { return; }
            x = (ushort)rand.Next(gameZone.xMin, gameZone.xMax);
            z = (ushort)rand.Next(gameZone.zMin, gameZone.zMax);

            byte b = lvl.GetTile(x, (ushort)(gameZone.y + 1), z);
            if (b == Block.wood || b == Block.water || b == Block.blackrock) { goto retry; }
            if (lvl.GetTile(x, gameZone.y, z) == Block.lava) { goto retry; }

            ushort xx = (ushort)((0.5 + x) * 32);
            ushort yy = (ushort)((2 + gameZone.y) * 32);
            ushort zz = (ushort)((0.5 + z) * 32);
            unchecked
            { p.SendPos((byte)-1, xx, yy, zz, p.level.rotx, p.level.roty); }
        }

        public struct zone
        {
            public ushort xMin, xMax;
            public ushort zMin, zMax;
            public ushort y;
        }

        public class pinfo
        {
            public Player p;
            public int nbTnt, puissanceTnt;
            public int nbWalls;
            public int nbKills, nbDeaths;
            public int lastKill;
            public List<tnt> tnts;
            public int nbPowerUp;
            public int nbPoints;
            public int overAllWalls;
        }

        public class tnt
        {
            public ushort x, z;
            public int time;

            public tnt(ushort px, ushort pz)
            { x = px; z = pz; time = 0; }
        }

        public class tntExp
        {
            public ushort x, z;
            public int time;
            public pinfo pi;

            public tntExp() { time = 0; }
        }

        public struct pos{public ushort x, z;}

        public struct posXYZ{public ushort x, y, z;}
    }
}
