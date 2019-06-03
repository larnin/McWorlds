using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCWorlds
{
    public class CmdCTF : Command
    {
        public override string name { get { return "ctf"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public CmdCTF() { }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            int num = message.Split(' ').Length;
            if (num == 3)
            {
                string[] strings = message.Split(' ');

                for (int i = 0; i < num; i++)
                {
                    strings[i] = strings[i].ToLower();
                }

                if (strings[0] == "team")
                {
                    if (strings[1] == "add")
                    {
                        string color = c.Parse(strings[2]);
                        if (color == ""){Player.SendMessage(p, "Couleur d'equipe invalide."); return;}
                        char teamCol = (char)color[1];
                        switch (teamCol)
                        {
                            case '2':
                            case '5':
                            case '8':
                            case '9':
                            case 'c':
                            case 'e':
                            case 'f':
                                AddTeam(p, color);
                                break;
                            default:
                                Player.SendMessage(p, "Couleur d'equipe invalide.");
                                return;
                        }
                    }
                    else if (strings[1] == "remove")
                    {
                        string color = c.Parse(strings[2]);
                        if (color == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                        char teamCol = (char)color[1];
                        switch (teamCol)
                        {
                            case '2':
                            case '5':
                            case '8':
                            case '9':
                            case 'c':
                            case 'e':
                            case 'f':
                                RemoveTeam(p, color);
                                break;
                            default:
                                Player.SendMessage(p, "Couleur d'equipe invalide.");
                                return;
                        }
                    }
                }
            }
            else if (num == 2)
            {
                string[] strings = message.Split(' ');

                for (int i = 0; i < num; i++)
                {
                    strings[i] = strings[i].ToLower();
                }

                if (strings[0] == "debug")
                {
                    Debug(p, strings[1]);
                }
                else if (strings[0] == "flag")
                {
                    string color = c.Parse(strings[1]);
                    if (color == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    char teamCol = (char)color[1];
                    if (p.level.ctfgame.teams.Find(team => team.color == teamCol) == null) { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    CatchPos cpos;
                    cpos.x = 0; cpos.y = 0; cpos.z = 0; cpos.color = color; p.blockchangeObject = cpos;
                    Player.SendMessage(p, "Place un bloc pour determiner la position du drapeau.");
                    p.ClearBlockchange();
                    p.Blockchange += new Player.BlockchangeEventHandler(FlagBlockChange);
                }
                else if (strings[0] == "spawn")
                {
                    string color = c.Parse(strings[1]);
                    if (color == "") { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    char teamCol = (char)color[1];
                    if (p.level.ctfgame.teams.Find(team => team.color == teamCol) == null) { Player.SendMessage(p, "Couleur d'equipe invalide."); return; }
                    AddSpawn(p, color);
                    
                }
                else if (strings[0] == "points")
                {
                    int i = 0;
                    Int32.TryParse(strings[1], out i);
                    if (i == 0) { Player.SendMessage(p, "Vous devez choisir un nombre de points superieur a 0 !"); return; }
                    p.level.ctfgame.maxPoints = i;
                    Player.SendMessage(p, "Le nombre de points a ete mis a " + i);
                }
            }
            else if (num == 1)
            {
                if (message.ToLower() == "start")
                {
                    if (!p.level.ctfmode)
                    {
                        p.level.ctfmode = true;
                    }
                    p.level.ctfgame.gameOn = true;
                    p.level.ctfgame.GameStart();
                }
                else if (message.ToLower() == "stop")
                {
                    if (p.level.ctfmode)
                    {
                        p.level.ctfmode = false;
                    }
                    p.level.ctfmode = false;
                    p.level.ctfgame.gameOn = false;
                    p.level.ChatLevel(p.color + p.name + Server.DefaultColor + " a arrete le jeu");
                }
                else if (message.ToLower() == "ff")
                {
                    if (p.level.ctfgame.friendlyfire)
                    {
                        p.level.ChatLevel("Les tirs alliers ont ete activer.");
                        p.level.ctfgame.friendlyfire = false;
                    }
                    else
                    {
                        p.level.ChatLevel("Les tirs alliers ont ete activer.");
                        p.level.ctfgame.friendlyfire = true;
                    }
                }
                else if (message.ToLower() == "clear")
                {
                    List<Team> storedT = new List<Team>();
                    for (int i = 0; i < p.level.ctfgame.teams.Count; i++)
                    {
                        storedT.Add(p.level.ctfgame.teams[i]);
                    }
                    foreach (Team t in storedT)
                    {
                        p.level.ctfgame.RemoveTeam("&" + t.color);
                    }
                    p.level.ctfgame.onTeamCheck.Stop();
                    p.level.ctfgame.onTeamCheck.Dispose();
                    p.level.ctfgame.gameOn = false;
                    p.level.ctfmode = false;
                    p.level.ctfgame = new CTFGame();
                    p.level.ctfgame.mapOn = p.level;
                    Player.SendMessage(p, "Les parametres du CTF ont ete remis a 0.");
                }

                else if (message.ToLower() == "")
                {
                    if (p.level.ctfmode)
                    {
                        p.level.ctfmode = false;
                        p.level.ChatLevel("Le mode CTF a ete desactive.");

                    }
                    else if (!p.level.ctfmode)
                    {
                        p.level.ctfmode = true;
                        p.level.ChatLevel("Le mode ctf a ete active.");
                    }
                }
            }
        }
        public void AddSpawn(Player p, string color)
        {
            char teamCol = (char)color[1];
            ushort x, y, z, rotx;
            x = (ushort)(p.pos[0] / 32);
            y = (ushort)(p.pos[1] / 32);
            z = (ushort)(p.pos[2] / 32);
            rotx = (ushort)(p.rot[0]);
            p.level.ctfgame.teams.Find(team => team.color == teamCol).AddSpawn(x, y, z, rotx, 0);
            Player.SendMessage(p, "Spawn ajoute pour " + p.level.ctfgame.teams.Find(team => team.color == teamCol).teamstring);
        }

        public void AddTeam(Player p, string color)
        {
            char teamCol = (char)color[1];
            if (p.level.ctfgame.teams.Find(team => team.color == teamCol)!= null){Player.SendMessage(p, "Cet equipe existe deja."); return;}
            p.level.ctfgame.AddTeam(color);
        }

        public void RemoveTeam(Player p, string color)
        {
            char teamCol = (char)color[1];
            if (p.level.ctfgame.teams.Find(team => team.color == teamCol) == null) { Player.SendMessage(p, "Cet equipe n'existe pas."); return; }
            p.level.ctfgame.RemoveTeam(color);
        }

        public void AddFlag(Player p, string col, ushort x, ushort y, ushort z)
        {
            char teamCol = (char)col[1];
            Team workTeam = p.level.ctfgame.teams.Find(team => team.color == teamCol);

            workTeam.flagBase[0] = x;
            workTeam.flagBase[1] = y;
            workTeam.flagBase[2] = z;

            workTeam.flagLocation[0] = x;
            workTeam.flagLocation[1] = y;
            workTeam.flagLocation[2] = z;
            workTeam.Drawflag();
        }

        public void Debug(Player p, string col)
        {
            if (col.ToLower() == "flags")
            {
                foreach (Team team in p.level.ctfgame.teams)
                {
                    Player.SendMessage(p, "Drapeau dessine pour " + team.teamstring);
                    team.Drawflag();
                }
                return;
            }
            else if (col.ToLower() == "spawn")
            {
                foreach (Team team in p.level.ctfgame.teams)
                {
                    foreach (Player player in team.players)
                    {
                        team.SpawnPlayer(player);
                    }
                }
                return;
            }
            string color = c.Parse(col);
            char teamCol = (char)color[1];
            Team workTeam = p.level.ctfgame.teams.Find(team => team.color == teamCol);
            string debugteams = "";
            for (int i = 0; i < p.level.ctfgame.teams.Count; i++)
            {
                debugteams += p.level.ctfgame.teams[i].teamstring + ", ";
            }
            Player.SendMessage(p, "Joueur debug: Equipe: " + p.team.teamstring/* + ", hasFlag: " + p.hasflag.teamstring + ", carryingFlag: " + p.carryingFlag*/);
            Player.SendMessage(p, "Equipe de jeu CTF: " + debugteams);
            string playerlist = "";
            foreach (Player player in workTeam.players)
            {
                playerlist += player.name + ", ";
            }
            Player.SendMessage(p, "Liste des joueurs " + playerlist);
            Player.SendMessage(p, "Points: " + workTeam.points + ", MapOn: " + workTeam.mapOn.name + ", flagishome: " + workTeam.flagishome + ", spawnset: " + workTeam.spawnset);
            Player.SendMessage(p, "Base drapeau[0]: " + workTeam.flagBase[0] + ", [1]: " + workTeam.flagBase[1] + ", [2]: " + workTeam.flagBase[2]);
            Player.SendMessage(p, "Position drapeau[0]: " + workTeam.flagLocation[0] + ", [1]: " + workTeam.flagLocation[1] + ", [2]: " + workTeam.flagLocation[2]);
         //   Player.SendMessage(p, "Spawn[0]: " + workTeam.spawn[0] + ", [1]: " + workTeam.spawn[1] + ", [2]: " + workTeam.spawn[2] + ", [3]: " + workTeam.spawn[3] + ", [4]: " + workTeam.spawn[4]);
        }


        void FlagBlockChange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            CatchPos bp = (CatchPos)p.blockchangeObject;
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            p.ClearBlockchange();
            AddFlag(p, bp.color, x, y, z);
        }


        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/ctf - Active ou desactive le mode CTF sur la map.");
            Player.SendMessage(p, "A besoin d'etre activer pour jouer.");
            Player.SendMessage(p, "/ctf start - Demarre le jeu!");
            Player.SendMessage(p, "/ctf stop - Arrete le jeu.");
            Player.SendMessage(p, "/ctf ff - Active/desactive les tirs allier. Desactive par defaut.");
            Player.SendMessage(p, "/ctf flag [couleur] - Place le drapeau pour l'equipe [couleur].");
            Player.SendMessage(p, "/ctf spawn [couleur] - Place le spawn pour l'equipe [couleur].");
            Player.SendMessage(p, "/ctf points [num] - Modifie le nombre de round. 3 par defaut.");
            Player.SendMessage(p, "/ctf team add [couleur] - Ajoute une equipe.");
            Player.SendMessage(p, "/ctf team remove [couleur] - Supprime une equipe.");
            Player.SendMessage(p, "/ctf clear - Supprime toutes les donnees du CTF.");
        }

        public struct CatchPos { public ushort x, y, z; public string color;}
    }
}
