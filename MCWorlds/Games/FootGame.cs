using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Data;

namespace MCWorlds
{
    public class FootGame : BaseGame
    {
        public pos posBallon = new pos();

        public pos spawnBallon = new pos();
        public Team redTeam = new Team();
        public Team blueTeam = new Team();
        public int points = 3;

        public pos redButMin = new pos();
        public pos redButMax = new pos();
        public pos blueButMin = new pos();
        public pos blueButMax = new pos();

        public bool inBut = false;

        public List<pinfo> players = new List<pinfo>();

        public System.Timers.Timer playerCheck = new System.Timers.Timer(500);
        public System.Timers.Timer downBallonTimer = new System.Timers.Timer(200);

        public FootGame(Level l)
        {
            typeGame = "foot";
            lvl = l;
            loadCmds();

            redTeam.color = 'c';
            redTeam.points = 0;
            redTeam.game = this;
            char[] temp = c.Name("&" + redTeam.color).ToCharArray();
            temp[0] = char.ToUpper(temp[0]);
            string tempstring = new string(temp);
            redTeam.teamstring = "&" + redTeam.color + "equipe " + tempstring + Server.DefaultColor;

            blueTeam.color = '9';
            blueTeam.points = 0;
            blueTeam.game = this;
            temp = c.Name("&" + blueTeam.color).ToCharArray();
            temp[0] = char.ToUpper(temp[0]);
            tempstring = new string(temp);
            blueTeam.teamstring = "&" + blueTeam.color + "equipe " + tempstring + Server.DefaultColor;
        }

        public override void loadGame(Player p, string file)
        {
            
        }

        public override void saveGame(Player p, string file)
        {
            
        }

        public override void startGame(Player p)
        {
            if (redTeam.players.Count < 2 || blueTeam.players.Count < 2)
            { Player.SendMessage(p, "Il faut au moins 2 joueurs pas equipes"); }

            foreach (Player pl in redTeam.players)
            { addPlayer(pl); }

            foreach (Player pl in redTeam.players)
            { addPlayer(pl); }

            if (redButMin.x == 0 || redButMin.y == 0 || redButMin.z == 0
                || redButMax.x == 0 || redButMax.y == 0 || redButMax.z == 0)
            { Player.SendMessage(p, "Pas de cage definit pour l'equipe rouge"); return; }

            if (blueButMin.x == 0 || blueButMin.y == 0 || blueButMin.z == 0
                || blueButMax.x == 0 || blueButMax.y == 0 || blueButMax.z == 0)
            { Player.SendMessage(p, "Pas de cage definit pour l'equipe bleu"); return; }

            if (spawnBallon.x == 0 || spawnBallon.y == 0 || spawnBallon.z == 0)
            { Player.SendMessage(p, "Le spawn du ballon n'est pas definit"); return; }

            Player.GlobalMessageLevel(lvl, "Go !!!");

            respawnBallon();

            run(p);
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
                savePlayer(pi);
                delPlayer(pi);
            }

            playerCheck.Stop();
            downBallonTimer.Stop();
            lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.air);
            posBallon.x = 0; posBallon.y = 0; posBallon.z = 0;
        }

        public override void deleteGame(Player p)
        {
            removeGame(this);
            Player.GlobalMessageLevel(lvl, "Foot desactive");

            playerCheck.Close();
            downBallonTimer.Close();
        }

        public override bool changebloc(Player p, byte type, ushort x, ushort y, ushort z, byte action)
        {
            return false;
        }

        public override bool checkPos(Player p, ushort x, ushort y, ushort z)
        {
            if (!gameOn || inBut) { return true; }

            if (p.team == null) { return true; }
            pinfo pi = players.Find(pin => pin.p == p);
            if (pi == null) { return true; }

            double dist = Math.Sqrt((p.pos[0] - posBallon.x) * (p.pos[0] - posBallon.x) 
                + (p.pos[2] - posBallon.z) * (p.pos[2] - posBallon.z));

            if (dist > 3 * 32) { return true; }
            if (Math.Abs(p.pos[1] - posBallon.y) > 3 * 32) { return true; }

            for (int i = 0 ; i < players.Count ; i++)
            {
                double dist2 = Math.Sqrt((players[i].p.pos[0] - posBallon.x) * (players[i].p.pos[0] - posBallon.x)
                + (players[i].p.pos[2] - posBallon.z) * (players[i].p.pos[2] - posBallon.z));
                if (Math.Abs(players[i].p.pos[1] - posBallon.y) < 3 * 32)
                { if (dist2 < dist) { return true; } }
            }

            int dirX = 0, dirZ = 0;//angle de 32
            if (p.rot[0] >= 240 || p.rot[0] < 16) { dirZ = 3; }
            if (p.rot[0] >= 16 && p.rot[0] < 48) { dirZ = 2; dirX = -2; }
            if (p.rot[0] >= 48 && p.rot[0] < 80) { dirX = -3; }
            if (p.rot[0] >= 80 && p.rot[0] < 112) { dirX = -2; dirZ = -2; }
            if (p.rot[0] >= 112 && p.rot[0] < 144) { dirZ = -3; }
            if (p.rot[0] >= 144 && p.rot[0] < 176) { dirZ = -2; dirX = 2; }
            if (p.rot[0] >= 176 && p.rot[0] < 208) { dirX = 3; }
            if (p.rot[0] >= 208 && p.rot[0] < 240) { dirX = 2; dirZ = 2; }

            if (dirX == 0 && dirZ == 0) { return true; }

            pos newPosBallon = new pos();
            newPosBallon.x = (ushort)(x + dirX);
            newPosBallon.y = posBallon.y;
            newPosBallon.z = (ushort)(z + dirZ);

            if (newPosBallon.x == posBallon.x && newPosBallon.y == posBallon.y && newPosBallon.z == posBallon.z)
            { return true; }

            if (posBallon.y < y)
            {
                bool under = true;
                while (under)
                {
                    if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air) { newPosBallon.y++; }
                    else { under = false; }
                }
            }

            if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
            {
                newPosBallon.y++;
                if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                {
                    newPosBallon.y--;
                    newPosBallon.x = (ushort)(newPosBallon.x - Math.Sign(dirX));
                    newPosBallon.z = (ushort)(newPosBallon.z - Math.Sign(dirZ));
                    if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                    {
                        newPosBallon.y++;
                        if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                        {
                            newPosBallon.y--;
                            newPosBallon.x = (ushort)(newPosBallon.x - Math.Sign(dirX));
                            newPosBallon.z = (ushort)(newPosBallon.z - Math.Sign(dirZ));
                            if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                            {
                                newPosBallon.y++;
                                if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                                {

                                    if (dirX == 3 || dirZ == 3)
                                    {
                                        newPosBallon.y--;
                                        newPosBallon.x = (ushort)(newPosBallon.x - Math.Sign(dirX));
                                        newPosBallon.z = (ushort)(newPosBallon.z - Math.Sign(dirZ));
                                    }

                                    if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) != Block.air)
                                    {
                                        bool ballonIn = false;
                                        while (!ballonIn)
                                        {
                                            if (newPosBallon.y >= lvl.depth)
                                            {
                                                Player.GlobalMessageLevel(lvl, "Ballon sortit, respawn");
                                                respawnBallon();
                                                return true;
                                            }
                                            newPosBallon.y++;
                                            if (lvl.GetTile(newPosBallon.x, newPosBallon.y, newPosBallon.z) == Block.air)
                                            { ballonIn = true; }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (newPosBallon.x == posBallon.x && newPosBallon.y == posBallon.y && newPosBallon.z == posBallon.z)
            { return true; }

            lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.air);
            posBallon.x = newPosBallon.x;
            posBallon.y = newPosBallon.y;
            posBallon.z = newPosBallon.z;
            lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.white);

            checkBut(p);

            return true;
        }

        public override void death(Player p)
        { }

        public static void stats(Player p, string pname)
        {
           
        }

        public static void top(Player p)
        {
            
        }

        public void GameInfo(Player p)
        {
           
        }

        public void run(Player p)
        {
            gameOn = true;

            Thread sendMessagesThread = new Thread(new ThreadStart(delegate
            {
                while (gameOn)
                {
                    for (int i = 0; i < players.Count; i++)
                    { players[i].p.sendAll(); }
                    Thread.Sleep(1000);
                }
            })); sendMessagesThread.Start();

            playerCheck.Start();
            playerCheck.Elapsed += delegate
            {
                List<pinfo> stored = new List<pinfo>();
                for (int i = 0; i < players.Count; i++)
                {
                    stored.Add(players[i]);
                }
                foreach (pinfo pi in stored)
                { if (pi.p.level != lvl || pi.p.disconnected) { delPlayer(pi); continue; } }
            };

            downBallonTimer.Start();
            downBallonTimer.Elapsed += delegate
            {
                if (posBallon.y > 0)
                {
                    if (lvl.GetTile(posBallon.x, (ushort)(posBallon.y - 1), posBallon.z) == Block.air)
                    {
                        lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.air);
                        posBallon.y--;
                        lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.white);
                        checkBut(null);
                    }
                }
            };
        }

        public void respawnBallon()
        {
            if (posBallon.x != 0 && posBallon.y != 0 && posBallon.z != 0)
            { lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.air); }
            posBallon.x = spawnBallon.x;
            posBallon.y = spawnBallon.y;
            posBallon.z = spawnBallon.z;
            lvl.Blockchange(posBallon.x, posBallon.y, posBallon.z, Block.white);
        }

        public void addPlayer(Player p)
        {
            pinfo pi = new pinfo(p);

            pi.p = p;

            p.ingame = true;
            p.tailleBufferGame = 4;
            p.gameMessages.Clear();
            p.addMessage("&e------------- foot ---------------", true);
            p.addMessage("&ePoints : &9bleu " + blueTeam.points + " &e/ &crouge " + redTeam.points, true);
            p.addMessage("&e----------------------------------", true);
            abort(p);

            players.Add(pi);
        }

        public void delPlayer(pinfo pi)
        {
            pi.p.team.RemoveMember(pi.p);
            pi.p.sendAll();
            players.Remove(pi);
            pi.p.ingame = false;
            pi.p.tailleBufferGame = 0;
            pi.p.gameMessages.Clear();
        }

        public void savePlayer(pinfo pi)
        {

        }

        public void checkBut(Player p)
        {
            if (posBallon.x >= redButMin.x && posBallon.y >= redButMin.y && posBallon.z >= redButMin.z
                && posBallon.x <= redButMax.x && posBallon.y <= redButMax.y && posBallon.z <= redButMax.z)
            { but(p, blueTeam); }

            if (posBallon.x >= blueButMin.x && posBallon.y >= blueButMin.y && posBallon.z >= blueButMin.z
                && posBallon.x <= blueButMax.x && posBallon.y <= blueButMax.y && posBallon.z <= blueButMax.z)
            { but(p, redTeam); }
        }

        public void but(Player p, Team t)
        {
            if (p != null)
            {
                pinfo pi = players.Find(pin => pin.p == p);
                if (pi == null) { return; }
                if (p.team == null) { return; }
            }

            Thread butThread = new Thread(new ThreadStart(delegate
            {
                inBut = true;
                t.points++;
                foreach (pinfo pin in players)
                { pin.p.addMessage("&ePoints : &9bleu " + blueTeam.points + " &e/ &crouge " + redTeam.points, true, 1); }
                if (p != null)
                {
                    if (p.team == t)
                    { Player.GlobalMessageLevel(lvl, "But marque par l'" + p.team.teamstring); }
                    else
                    { Player.GlobalMessageLevel(lvl, "But contre son camp par " + p.team.color + p.name); }
                }
                else { Player.GlobalMessageLevel(lvl, "But marque par l'" + p.team.teamstring); }
                Thread.Sleep(2000);
                if (redTeam.points >= points) { endGame(redTeam); }
                else if (blueTeam.points >= points) { endGame(blueTeam); }
                else
                {
                    Player.GlobalMessageLevel(lvl, "Respawn du ballon");
                    respawnBallon();
                }
                inBut = false;
            })); butThread.Start();
        }

        public void endGame(Team winTeam)
        {
            Player.GlobalMessageLevel(lvl, "Partie terminee !");
            Player.GlobalMessageLevel(lvl, "L'equipe gagnante est l'" + winTeam.teamstring + " !");

            gameOn = false;

            stopGame(null);
        }

        public class pinfo
        {
            public Player p;

            public pinfo(Player pl)
            { p = pl; }
        }

        public struct pos
        {
            public ushort x, y, z;
        }
    }
}
