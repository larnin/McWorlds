using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Data;

namespace MCWorlds
{
    public class CTFGame2 : BaseGame
    {
        public List<Team> teams = new List<Team>();

        public List<pinfo> players = new List<pinfo>();

        public int maxPoints = 3;

        public bool friendlyfire = false;

        public System.Timers.Timer onTeamCheck = new System.Timers.Timer(500);
        public System.Timers.Timer flagReturn = new System.Timers.Timer(1000);

        public CTFGame2(Level level)
        {
            typeGame = "ctf";
            lvl = level;

            MySQL.executeQuery("CREATE TABLE if not exists ctf (Name VARCHAR(20), nbKills MEDIUMINT, nbDeaths MEDIUMINT, nbJeux MEDIUMINT, nbWins MEDIUMINT, nbLooses MEDIUMINT, nbFlags MEDIUMINT, nbPoints MEDIUMINT);");

            loadCmds();
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

            List<Team> storedT = new List<Team>();
            for (int i = 0; i < teams.Count; i++)
            {
                storedT.Add(teams[i]);
            }
            foreach (Team t in storedT)
            {
                RemoveTeam("&" + t.color);
            }

            cmdAllow.Clear();

            string teamColor = "";
            Team team = new Team();
            
            foreach (string line in allLines)
            {
                if (line == "") { continue; }
                if (line[0] == '#') { continue; }

                if (line == "teamend") { teamColor = ""; continue; }
                
                if (line.IndexOf("=") == -1) { continue; }
                
                if (teamColor == "") { team = null; }
                else
                {
                    if (team == null) { team = teams.Find(t => t.color == teamColor[0]); }
                    else if (team.color != teamColor[0]) { team = teams.Find(t => t.color == teamColor[0]); }
                }

                string key = line.Split('=')[0].Trim();
                string value = line.Split('=')[1].Trim();

                switch (key)
                {
                    case "ff":
                        try { friendlyfire = bool.Parse(value); }
                        catch { friendlyfire = false; }
                        break;
                    case "points":
                        try { maxPoints = int.Parse(value); }
                        catch { maxPoints = 3; }
                        if (maxPoints < 1) { maxPoints = 3; }
                        break;
                    case "commandes":
                        if (value.Split(' ').Length == 0) { continue; }
                        foreach (string s in value.Split(' '))
                        { cmdAllow.Add(s); }
                        break;
                    case "team":
                        AddTeam("&" + value);
                        teamColor = value;
                        break;
                    case "spawn":
                        if (team == null) { continue; }
                        if (value.Split(' ').Length != 5) { continue; }
                        int[] posSpawn = new int[5];
                        for (int i = 0; i < 5; i++)
                        {
                            try { posSpawn[i] = int.Parse(value.Split(' ')[i]); }
                            catch { posSpawn[i] = 0; }
                            if (posSpawn[i] < 0) { posSpawn[i] = 0; }
                        }
                        team.AddSpawn((ushort)posSpawn[0], (ushort)posSpawn[1], (ushort)posSpawn[2], (ushort)posSpawn[3], (ushort)posSpawn[4]);
                        break;
                    case "drapeau":
                        if (team == null) { continue; }
                        if (value.Split(' ').Length != 3) { continue; }
                        int[] posFlag = new int[3];
                        for (int i = 0; i < 3; i++)
                        {
                            try { posFlag[i] = int.Parse(value.Split(' ')[i]); }
                            catch { posFlag[i] = 0; }
                            if (posFlag[i] < 0) { posFlag[i] = 0; }
                        }
                        team.flagBase[0] = (ushort)posFlag[0];
                        team.flagBase[1] = (ushort)posFlag[1];
                        team.flagBase[2] = (ushort)posFlag[2];

                        team.flagLocation[0] = team.flagBase[0];
                        team.flagLocation[1] = team.flagBase[1];
                        team.flagLocation[2] = team.flagBase[2];

                        team.Drawflag();
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

            SW.WriteLine("ff = " + friendlyfire.ToString());
            SW.WriteLine("points = " + maxPoints);

            string cmds = "";

            foreach (string cmd in cmdAllow)
            { cmds += cmd + " "; }
            if (cmds != "") { SW.WriteLine("commandes = " + cmds.Trim()); }

            foreach (Team team in teams)
            {
                SW.WriteLine("team = " + team.color);
                SW.WriteLine("drapeau = " + team.flagBase[0] + " " + team.flagBase[1] + " " + team.flagBase[2]);
                foreach (Team.Spawn spawn in team.spawns)
                { SW.WriteLine("spawn = " + spawn.x + " " + spawn.y + " " + spawn.z + " " + spawn.rotx + " " + spawn.roty); }
                SW.WriteLine("teamend");
            }
            SW.Flush();
            SW.Close();
            SW.Dispose();

            Player.SendMessage(p, "Les configurations du ctf sont sauvegarde dans " + file);
        }

        public override void startGame(Player p)
        {
            if (teams.Count < 2)
            { Player.SendMessage(p, "Il doit avoir au moins 2 equipes"); return; }

            foreach (Team team in teams)
            {
                if (team.players.Count < 2)
                { Player.SendMessage(p, "Il doit avoir au moins 2 joueurs par equipe"); return; }
            }
            
            foreach (Team team in teams)
            {
                ReturnFlag(null, team, false);
                foreach (Player pl in team.players)
                {
                    if (!pl.loggedIn || pl.level != lvl)
                    {
                        team.RemoveMember(pl);
                    }
                    else
                    {
                        team.SpawnPlayer(pl);
                        addPlayer(pl);
                        abort(p);
                    }
                }
            }
            lvl.ChatLevel("Le CTF commence !");
            gameOn = true;

            onTeamCheck.Start();
            onTeamCheck.Elapsed += delegate
            {
                foreach (Team team in teams)
                {
                    for (int i = 0 ; i < team.players.Count; i++)
                    {
                        if (!team.players[i].loggedIn || team.players[i].level != lvl)
                        {
                            team.RemoveMember(team.players[i]);
                            removePlayer(team.players[i], false);
                        }
                    }
                }
            };

            flagReturn.Start();
            flagReturn.Elapsed += delegate
            {
                foreach (Team team in teams)
                {
                    if (!team.flagishome && team.holdingFlag == null)
                    {
                        team.ftcount++;
                        if (team.ftcount > 30)
                        {
                            lvl.ChatLevel("Le drapeau " + team.teamstring + " est retourne a sa base.");
                            team.ftcount = 0;
                            ReturnFlag(null, team, false);
                        }
                    }
                }
            };

            Thread flagThread = new Thread(new ThreadStart(delegate
            {
                while (gameOn)
                {
                    foreach (Team team in teams)
                    {
                        team.Drawflag();
                    }
                    Thread.Sleep(200);
                }

            })); flagThread.Start();

            
            Thread sendMessagesThread = new Thread(new ThreadStart(delegate
            {
                foreach (Team team in teams)
                {
                    foreach (Player who in team.players)
                    {
                        who.ingame = true;
                        who.tailleBufferGame = 5;
                        who.addMessage(Server.DefaultColor + "--------------- CTF --------------", true);
                        who.addMessage(Server.DefaultColor + "Team " + team.teamstring + " Points : 0", true);
                        who.addMessage(Server.DefaultColor + "Le drapeau est a sa base", true);
                        who.addMessage(Server.DefaultColor + "Vie : &" + team.color + who.health, true);
                        who.addMessage(Server.DefaultColor + "----------------------------------", true);
                    }
                }

                while (gameOn)
                {
                    foreach (Team team in teams)
                    {
                        foreach (Player who in team.players)
                        {
                            if (who.level != lvl) { continue; }
                            who.sendAll();
                        }
                    }
                    Thread.Sleep(1000);
                }

                foreach (Team team in teams)
                {
                    foreach (Player who in team.players)
                    {
                        if (who.level != lvl) { continue; }
                        who.sendAll();
                        who.ingame = false;
                        who.gameMessages.Clear();
                        who.tailleBufferGame = 0;

                    }
                }

            })); sendMessagesThread.Start();
        }

        public override void stopGame(Player p)
        {
            gameOn = false;

            if (p != null) { Player.GlobalMessageLevel(lvl, "Le jeu est arette par " + p.Name()); }
            
            foreach (Team team in teams)
            {
                foreach (Player who in team.players)
                {Command.all.Find("spawn").Use(who, "");}
                ReturnFlag(null, team, false);
            }

            onTeamCheck.Stop();
            flagReturn.Stop();
        }

        public override void deleteGame(Player p)
        {
            List<Team> storedT = new List<Team>();
            for (int i = 0; i < teams.Count; i++)
            {
                storedT.Add(teams[i]);
            }
            foreach (Team t in storedT)
            {
                RemoveTeam("&" + t.color);
            }
            removeGame(this);
            Player.GlobalMessageLevel(lvl, "CTF desactive");

            onTeamCheck.Close();
            flagReturn.Close();
        }

        public override bool changebloc(Player p, byte b, ushort x, ushort y, ushort z, byte action)
        { return true; }

        public override bool checkPos(Player p, ushort x, ushort y, ushort z)
        {
            byte b = lvl.GetTile(x, y, z);
            byte b1 = lvl.GetTile(x, (ushort)((int)y - 1), z);

            if (p.team == null) { return true; }
            
            y = (ushort)(y - 1);

            if (gameOn)
            {
                foreach (Team workTeam in teams)
                {
                    if (workTeam.flagLocation[0] == x && workTeam.flagLocation[1] == y && workTeam.flagLocation[2] == z)
                    {
                        if (workTeam == p.team)
                        {
                            if (workTeam.flagishome)
                            {
                                if (p.carryingFlag)
                                {
                                    CaptureFlag(p, workTeam, p.hasflag);
                                }
                            }
                        }
                        else
                        {
                            GrabFlag(p, workTeam);
                        }
                    }
                }
            }
            return true;
        }

        public override void death(Player p)
        {
            if (p.team != null)
            {
                if (p.carryingFlag)
                {
                    DropFlag(p, p.hasflag);
                }
                p.team.SpawnPlayer(p);
                p.addMessage(Server.DefaultColor + "Vie : &" + p.team.color + 100, true, 3);
                p.health = 100;

                pinfo pi = players.Find(pin => pin.p == p);
                if (pi != null) { pi.nbDeaths++; }
            }
        }

        public static void stats(Player p, string pname)
        {
            DataTable playerDb = MySQL.fillData("SELECT * FROM ctf WHERE Name='" + pname + "'");

            if (playerDb.Rows.Count == 0)
            {
                Player.SendMessage(p, "Le joueur '" + pname + " n'a pas de statistiques sur le ctf");
            }
            else
            {
                pinfo pi = new pinfo(null);
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbKills"].ToString());
                pi.nbDeaths = int.Parse(playerDb.Rows[0]["nbDeaths"].ToString());
                pi.nbFlags = int.Parse(playerDb.Rows[0]["nbFlags"].ToString());
                pi.nbGames = int.Parse(playerDb.Rows[0]["nbJeux"].ToString());
                pi.nbWin = int.Parse(playerDb.Rows[0]["nbWins"].ToString());
                pi.nbLoose = int.Parse(playerDb.Rows[0]["nbLooses"].ToString());
                pi.nbPoints = int.Parse(playerDb.Rows[0]["nbPoints"].ToString());

                Player.SendMessage(p, "Statistiques de '" + pname + " :");
                Player.SendMessage(p, "> > Nombre de points : &2" + pi.nbPoints);
                Player.SendMessage(p, "> > Nombre de parties : &2" + pi.nbGames);
                Player.SendMessage(p, "> > Nombre de victiores : &2" + pi.nbWin);
                Player.SendMessage(p, "> > Nombre de defaites : &2" + pi.nbLoose);
                Player.SendMessage(p, "> > Nombre de drapeaux attrape : &2" + pi.nbFlags);
                Player.SendMessage(p, "> > Nombre de kills : &2" + pi.nbKills);
                Player.SendMessage(p, "> > Nombre de morts : &2" + pi.nbDeaths);
            }
            playerDb.Dispose();
        }

        public static void top(Player p)
        {
            DataTable maxDb = MySQL.fillData("SELECT MAX(nbPoints), MAX(nbKills), MAX(nbDeaths), MAX(nbFlags), MAX(nbJeux), MAX(nbWins), MAX(nbLooses) FROM ctf");

            int maxKill = int.Parse(maxDb.Rows[0]["MAX(nbKills)"].ToString());
            int maxDeath = int.Parse(maxDb.Rows[0]["MAX(nbDeaths)"].ToString());
            int maxPoint = int.Parse(maxDb.Rows[0]["MAX(nbPoints)"].ToString());
            int maxFlag = int.Parse(maxDb.Rows[0]["MAX(nbFlags)"].ToString());
            int maxJeux = int.Parse(maxDb.Rows[0]["MAX(nbJeux)"].ToString());
            int maxWins = int.Parse(maxDb.Rows[0]["MAX(nbWins)"].ToString());
            int maxLooses = int.Parse(maxDb.Rows[0]["MAX(nbLooses)"].ToString());
            maxDb.Dispose();

            Player.SendMessage(p, "CTF - le top :");
            DataTable namePoints = MySQL.fillData("SELECT Name FROM ctf WHERE nbPoints=" + maxPoint);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de points : &2" + namePoints.Rows[0]["name"] + " (" + maxPoint + ")"); }
            namePoints.Dispose();

            DataTable nameGames = MySQL.fillData("SELECT Name FROM ctf WHERE nbJeux=" + maxJeux);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur participe au plus de parties : &2" + nameGames.Rows[0]["name"] + " (" + maxJeux + ")"); }
            namePoints.Dispose();

            DataTable nameWin = MySQL.fillData("SELECT Name FROM ctf WHERE nbWins=" + maxWins);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur le plus victorieux : &2" + nameWin.Rows[0]["name"] + " (" + maxWins + ")"); }
            namePoints.Dispose();

            DataTable nameLooses = MySQL.fillData("SELECT Name FROM ctf WHERE nbLooses=" + maxLooses);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant accumule le plus de defaites : &2" + nameLooses.Rows[0]["name"] + " (" + maxLooses + ")"); }
            namePoints.Dispose();

            DataTable nameFlags = MySQL.fillData("SELECT Name FROM ctf WHERE nbFlags=" + maxFlag);
            if (namePoints.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de points : &2" + nameFlags.Rows[0]["name"] + " (" + maxFlag + ")"); }
            namePoints.Dispose();

            DataTable nameKills = MySQL.fillData("SELECT Name FROM ctf WHERE nbKills=" + maxKill);
            if (nameKills.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de kills : &2" + nameKills.Rows[0]["name"] + " (" + maxKill + ")"); }
            nameKills.Dispose();

            DataTable nameDeaths = MySQL.fillData("SELECT Name FROM ctf WHERE nbDeaths=" + maxDeath);
            if (nameDeaths.Rows.Count != 0)
            { Player.SendMessage(p, "Joueur ayant le plus de morts : &2" + nameDeaths.Rows[0]["name"] + " (" + maxDeath + ")"); }
            nameDeaths.Dispose();
        }

        public void AddTeam(string color)
        {
            char teamCol = (char)color[1];

            Team workteam = new Team();

            workteam.color = teamCol;
            workteam.points = 0;
            workteam.game = this;
            char[] temp = c.Name("&" + teamCol).ToCharArray();
            temp[0] = char.ToUpper(temp[0]);
            string tempstring = new string(temp);
            workteam.teamstring = "&" + teamCol + tempstring + " team" + Server.DefaultColor;

            teams.Add(workteam);

            lvl.ChatLevel(workteam.teamstring + " est initialise!");
        }

        public void RemoveTeam(string color)
        {
            char teamCol = (char)color[1];

            Team workteam = teams.Find(team => team.color == teamCol);
            List<Player> storedP = new List<Player>();

            for (int i = 0; i < workteam.players.Count; i++)
            {
                storedP.Add(workteam.players[i]);
            }
            foreach (Player p in storedP)
            {
                workteam.RemoveMember(p);
            }
            teams.Remove(workteam);
        }

        public void GrabFlag(Player p, Team team)
        {
            if (p.carryingFlag) { return; }
            ushort x = (ushort)(p.pos[0] / 32);
            ushort y = (ushort)((p.pos[1] / 32) + 3);
            ushort z = (ushort)(p.pos[2] / 32);

            team.tempFlagblock.x = x; team.tempFlagblock.y = y; team.tempFlagblock.z = z; team.tempFlagblock.type = lvl.GetTile(x, y, z);

            lvl.Blockchange(x, y, z, Team.GetColorBlock(team.color));

            lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " a pris le drapeau " + team.teamstring + " !");
            p.hasflag = team;
            p.carryingFlag = true;
            team.holdingFlag = p;
            team.flagishome = false;

            if (p.aiming)
            {
                p.ClearBlockchange();
                p.aiming = false;
            }

            foreach (Player who in team.players)
            { who.addMessage(Server.DefaultColor + "Le drapeau est possede par &" + p.team.color + p.name, true, 2); }
        }

        public void ReturnFlag(Player p, Team team, bool verbose)
        {
            if (p != null && p.spawning) { return; }
            if (verbose)
            {
                if (p != null)
                {
                    lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " a r'envoyer le drapeau " + team.teamstring + " a sa base!");
                }
                else
                {
                    lvl.ChatLevel("Le drapeau " + team.teamstring + " est retourne a sa base.");
                }
            }
            team.holdingFlag = null;
            team.flagLocation[0] = team.flagBase[0];
            team.flagLocation[1] = team.flagBase[1];
            team.flagLocation[2] = team.flagBase[2];
            team.flagishome = true;

            foreach (Player who in team.players)
            {who.addMessage(Server.DefaultColor + "Le drapeau est a sa base", true,2);}
        }

        public void CaptureFlag(Player p, Team playerTeam, Team capturedTeam)
        {
            pinfo pi = players.Find(pin => pin.p == p);
            if (pi != null) { pi.nbFlags++; pi.nbPoints++; }

            playerTeam.points++;
            lvl.Blockchange(capturedTeam.tempFlagblock.x, capturedTeam.tempFlagblock.y, capturedTeam.tempFlagblock.z, capturedTeam.tempFlagblock.type);
            lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " a capture le drapeau " + capturedTeam.teamstring + " !");

            if (playerTeam.points >= maxPoints)
            {
                GameEnd(playerTeam);
                return;
            }

            lvl.ChatLevel(playerTeam.teamstring + " a maintenant " + playerTeam.points + " point(s).");
            p.hasflag = null;
            p.carryingFlag = false;
            ReturnFlag(null, capturedTeam, false);

            for (int i = 0 ; i < playerTeam.players.Count; i++)
            { playerTeam.players[i].addMessage(Server.DefaultColor + "Team " + playerTeam.teamstring + " Points : " + playerTeam.points, true, 1); }
        }

        public void DropFlag(Player p, Team team)
        {
            lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " lache le drapeau " + team.teamstring + " !");
            ushort x = (ushort)(p.pos[0] / 32);
            ushort y = (ushort)((p.pos[1] / 32) - 1);
            ushort z = (ushort)(p.pos[2] / 32);

            lvl.Blockchange(team.tempFlagblock.x, team.tempFlagblock.y, team.tempFlagblock.z, team.tempFlagblock.type);

            team.flagLocation[0] = x;
            team.flagLocation[1] = y;
            team.flagLocation[2] = z;

            p.hasflag = null;
            p.carryingFlag = false;

            team.holdingFlag = null;
            team.flagishome = false;

            foreach (Player who in team.players)
            { who.addMessage(Server.DefaultColor + "Le drapeau est au sol", true, 2); }
        }
        
        public void GameEnd(Team winTeam)
        {
            lvl.ChatLevel("Le jeu est termine ! " + winTeam.teamstring + " a gagne avec " + winTeam.points + " point(s)!");
            pinfo pi = null;
            foreach (Team team in teams)
            {
                ReturnFlag(null, team, false);
                foreach (Player p in team.players)
                {
                    p.hasflag = null;
                    p.carryingFlag = false;

                    pi = players.Find(pin => pin.p == p);
                    if (pi != null)
                    {
                        pi.nbGames++;
                        if (team == winTeam) { pi.nbWin++; pi.nbPoints += 10; }
                        else { pi.nbLoose++; }
                        savePlayer(pi);
                        players.Remove(pi);
                    }
                }
                team.points = 0;
            }
            gameOn = false;

            stopGame(null);
        }

        public void GameInfo(Player p)
        {
            if (!gameOn) 
            { 
                Player.SendMessage(p, "&cCTF " + Server.DefaultColor + "- La partie n'est pas lancee");

                if (friendlyfire) { Player.SendMessage(p, "Vous pouvez tirer sur vos alliers"); }
                else { Player.SendMessage(p, "Impossible de tirer sur ses alliers"); }
                Player.SendMessage(p, "La partie se jouera en " + maxPoints + " points");

                string playerlist = "";
                foreach (Team team in teams)
                {
                    playerlist = "";
                    foreach (Player pl in team.players) { playerlist += pl.name + ", "; }
                    if (playerlist == "") { Player.SendMessage(p, "Team : " + team.teamstring + Server.DefaultColor + " - Pas de joueurs"); }
                    else
                    {
                        Player.SendMessage(p, "Team : " + team.teamstring);
                        Player.SendMessage(p, "Joueurs : " + playerlist.Remove(playerlist.Length - 2));
                    }
                }
            }
            else 
            { 
                Player.SendMessage(p, "&cCTF " + Server.DefaultColor + "- La partie est en cours");

                if (friendlyfire) { Player.SendMessage(p, "Vous pouvez tirer sur vos alliers"); }
                else { Player.SendMessage(p, "Impossible de tirer sur ses alliers"); }
                Player.SendMessage(p, "La partie se joue en " + maxPoints + "points");

                string playerlist = "";
                foreach (Team team in teams)
                {
                    playerlist = "";
                    foreach (Player pl in team.players) { playerlist += pl.name + ", "; }
                    if (playerlist == "") { Player.SendMessage(p, "Team : " + team.teamstring + Server.DefaultColor + " - Pas de joueurs"); }
                    else
                    {
                        Player.SendMessage(p, "Team : " + team.teamstring + Server.DefaultColor + " - points " + team.points + "/" + maxPoints );
                        Player.SendMessage(p, "Joueurs : " + playerlist.Remove(playerlist.Length - 2));
                    }
                }
            }
        }

        public void addPlayer(Player p)
        {
            pinfo pi = new pinfo(p);

            pi.nbFlags = 0;
            pi.nbDeaths = 0;
            pi.nbGames = 0;
            pi.nbKills = 0;
            pi.nbLoose = 0;
            pi.nbPoints = 0;
            pi.nbWin = 0;

            DataTable playerDb = MySQL.fillData("SELECT * FROM ctf WHERE Name='" + p.name + "'");
            
            if (playerDb.Rows.Count == 0)
            {
                MySQL.executeQuery("INSERT INTO ctf (Name, nbKills, nbDeaths, nbJeux, nbWins, nbLooses, nbFlags, nbPoints)" +
                    "VALUES ('" + p.name + "', " + pi.nbKills + ", " + pi.nbDeaths + ", " + pi.nbGames + ", " + pi.nbWin + ", " + pi.nbLoose + ", " + pi.nbFlags + ", " + pi.nbPoints + ")");
            }
            else
            {
                pi.nbKills = int.Parse(playerDb.Rows[0]["nbKills"].ToString());
                pi.nbDeaths = int.Parse(playerDb.Rows[0]["nbDeaths"].ToString());
                pi.nbGames = int.Parse(playerDb.Rows[0]["nbJeux"].ToString());
                pi.nbWin = int.Parse(playerDb.Rows[0]["nbWins"].ToString());
                pi.nbLoose = int.Parse(playerDb.Rows[0]["nbLooses"].ToString());
                pi.nbFlags = int.Parse(playerDb.Rows[0]["nbFlags"].ToString());
                pi.nbPoints = int.Parse(playerDb.Rows[0]["nbPoints"].ToString());
            }
            playerDb.Dispose();

            players.Add(pi);
        }

        public void savePlayer(pinfo pi)
        {
            MySQL.executeQuery("UPDATE ctf SET nbKills= " + pi.nbKills + ", nbDeaths= " + pi.nbDeaths + ", nbJeux= " + pi.nbGames + ", nbWins= " + pi.nbWin + ", nbLooses= " + pi.nbLoose + ", nbFlags= " + pi.nbFlags + ", nbPoints=" + pi.nbPoints + " WHERE Name='" + pi.p.name + "'");
        }

        public void removePlayer(Player p, bool end = true)
        {
            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null) { return; }

            if (end) { savePlayer(pi); }
            players.Remove(pi);
        }

        public class pinfo
        {
            public Player p;
            public int nbFlags;
            public int nbKills;
            public int nbDeaths;
            public int nbGames;
            public int nbWin;
            public int nbLoose;
            public int nbPoints;

            public pinfo(Player pl)
            { p = pl; }
        }
    }
}
