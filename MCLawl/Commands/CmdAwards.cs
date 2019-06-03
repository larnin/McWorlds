/*
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl) Licensed under the
	Educational Community License, Version 2.0 (the "License"); you may
	not use this file except in compliance with the License. You may
	obtain a copy of the License at
	
	http://www.osedu.org/licenses/ECL-2.0
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the License is distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the License for the specific language governing
	permissions and limitations under the License.
*/
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdAwards : Command
    {
        public override string name { get { return "awards"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length > 2) { Help(p); return; }
            // /awards
            // /awards 1
            // /awards bob
            // /awards bob 1

            int totalCount = 0;
            string foundPlayer = "";

            if (message != "")
            {
                if (message.Split(' ').Length == 2)
                {
                    foundPlayer = message.Split(' ')[0];
                    Player who = Player.Find(foundPlayer);
                    if (who != null) foundPlayer = who.name;
                    try
                    {
                        totalCount = int.Parse(message.Split(' ')[1]);
                    }
                    catch
                    {
                        Help(p);
                        return;
                    }
                }
                else
                {
                    if (message.Length <= 3)
                    {
                        try
                        {
                            totalCount = int.Parse(message);
                        }
                        catch
                        {
                            foundPlayer = message;
                            Player who = Player.Find(foundPlayer);
                            if (who != null) foundPlayer = who.name;
                        }
                    }
                    else
                    {
                        foundPlayer = message;
                        Player who = Player.Find(foundPlayer);
                        if (who != null) foundPlayer = who.name;
                    }
                }
            }

            if (totalCount < 0)
            {
                Player.SendMessage(p, "Impossible d'afficher les pages inferieur à 0");
                return;
            }

            List<Awards.awardData> awardList = new List<Awards.awardData>();
            if (foundPlayer == "")
            {
                awardList = Awards.allAwards;
            }
            else
            {
                foreach (string s in Awards.getPlayersAwards(foundPlayer))
                {
                    Awards.awardData aD = new Awards.awardData();
                    aD.awardName = s;
                    aD.description = Awards.getDescription(s);
                    awardList.Add(aD);
                }
            }

            if (awardList.Count == 0)
            {
                if (foundPlayer != "")
                    Player.SendMessage(p, "Ce joueur n'a pas de trophe");
                else
                    Player.SendMessage(p, "Il n'y a pas de trophe dans ce serveur");

                return;
            }

            int max = totalCount * 5;
            int start = (totalCount - 1) * 5;
            if (start > awardList.Count)
            {
                Player.SendMessage(p, "Il n'y a pas de trophe a cette page");
                Player.SendMessage(p, "Entrez une plus petite valeur");
                return;
            }
            if (max > awardList.Count) 
                max = awardList.Count;

            if (foundPlayer != "")
                Player.SendMessage(p, Server.FindColor(foundPlayer) + foundPlayer + Server.DefaultColor + " a les trophes suivant:");
            else
                Player.SendMessage(p, "Trophes disponible: ");

            if (totalCount == 0)
            {
                foreach (Awards.awardData aD in awardList)
                    Player.SendMessage(p, "&6" + aD.awardName + ": &7" + aD.description);

                if (awardList.Count > 8) Player.SendMessage(p, "&5Utilise &b/awards " + message + " <1/2/3/...> &5pour une liste plus ordonnee");
            }
            else
            {
                for (int i = start; i < max; i++)
                {
                    Awards.awardData aD = awardList[i];
                    Player.SendMessage(p, "&6" + aD.awardName + ": &7" + aD.description);
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/awards - Affiche tous les trophes du serveur");
            Player.SendMessage(p, "/awards [joueur] - Donne la liste des trophes du joueur");
            Player.SendMessage(p, "/awards [joueur] <1/2/3...> ou /awards <1/2/3...>");
            Player.SendMessage(p, "Pour avoir des listes plus ordonnee");
        }
    }
}