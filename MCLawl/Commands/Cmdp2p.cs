using System;
using System.Collections.Generic;

namespace MCWorlds
    {
        public class CmdPTeleport : Command
        {
            public override string name { get { return "pteleport"; } }
            public override string shortcut { get { return "p2p"; } }
            public override string type { get { return "mod"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
            public override bool museumUsable { get { return true; } }
            public CmdPTeleport() { }

            public override void Help(Player p, string message = "")
            {
                Player.SendMessage(p, "/pteleport [joueur] - Envois le joueur au spawn");
                Player.SendMessage(p, "/pteleport [joueur1] [joueur2] - Teleporte le joueur1 vers le joueur2");
            }
            public override void Use(Player p, string message)
            {
                if (message == "") { Help(p); }
                else
                {
                    string[] split = message.Split(' ');
                    Player player1 = Player.Find(split[0]);
                    if (player1 != null)
                    {
                        if (player1 != p)
                        {
                            int length = split.Length;
                            if (length > 1)
                            {
                                Player player2 = Player.Find(split[1]);
                                if (player2 != null)
                                {
                                    if (player2 != p)
                                    {
                                        if (player1.level != player2.level)
                                        {
                                            if (player2.level.name.Contains("cMuseum"))
                                            {
                                                Player.SendMessage(p, "Le joueur '" + player2.name + "' est dans le musee !");
                                                return;
                                            }
                                            Group pGroup = player1.group;
                                            Group reqGroup = Group.findPerm(LevelPermission.Admin);
                                            player1.group = reqGroup;
                                            Command.all.Find("goto").Use(player1, player2.level.name + " " + player2.level.world);
                                            player1.group = pGroup;
                                            Player.SendMessage(p, "Envois de " + player1.name + " vers " + player2.level.name + "...");
                                        }
                                        while (player1.Loading)
                                        {
                                        }
                                        if (player1.level == player2.level)
                                        {
                                            if (player2.Loading)
                                            {
                                                Player.SendMessage(p, "Attendez que " + player2.color + player2.name + Server.DefaultColor + " finisse de charger..." );
                                                while (player2.Loading)
                                                {
                                                }
                                            }
                                            while (player1.Loading)
                                            {
                                            }
                                            player1.SendPos(0xff, player2.pos[0], player2.pos[1], player2.pos[2], player2.rot[0], 0);
                                            Player.SendMessage(p, "Envois reussi de " + player1.name + " vers " + player2.name);
                                            Player.SendMessage(player1, "Tu a ete teleporter vers " + player2.name);
                                        }
                                    }
                                    else
                                    {
                                        while (player1.Loading)
                                        {
                                        }
                                        Group pGroup = player1.group;
                                        Group reqGroup = Group.findPerm(LevelPermission.Admin);
                                        player1.group = reqGroup;
                                        Command.all.Find("spawn").Use(player1, "");
                                        player1.group = pGroup;
                                        Player.SendMessage(p, "Envois au spawn reussi " + player1.name);
                                        Player.SendMessage(player1, "Vous avez ete teleporter au spawn");
                                    }
                                }
                                else { Player.SendMessage(p, "Vous ne pouvez pas utiliser /pteleport sur vous"); }
                            }
                            else { Player.SendMessage(p, "il n'existe pas de joueur nomme '" + split[1] + "'"); }
                        }
                        else { Player.SendMessage(p, "Vous ne pouvez pas utiliser /pteleport sur vous"); }
                    }
                    else { Player.SendMessage(p, "il n'existe pas de joueur nomme '" + split[0] + "'"); }
                }
            }
        }
    }

