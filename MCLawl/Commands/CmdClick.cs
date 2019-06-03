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

namespace MCWorlds
{
    public class CmdClick : Command
    {
        public override string name { get { return "click"; } }
        public override string shortcut { get { return "x"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdClick() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            string[] parameters = message.Split(' ');
            ushort[] click = p.lastClick;

            if (message.IndexOf(' ') != -1)
            {
                if (parameters.Length != 3)
                {
                    Help(p);
                    return;
                }
                else
                {
                    for (int value = 0; value < 3; value++)
                    {
                        if (parameters[value].ToLower() == "x" || parameters[value].ToLower() == "y" || parameters[value].ToLower() == "z")
                            click[value] = p.lastClick[value];
                        else if (isValid(parameters[value], value, p))
                            click[value] = ushort.Parse(parameters[value]);
                        else
                        {
                            Player.SendMessage(p, "\"" + parameters[value] + "\" n'est pas valide");
                            return;
                        }
                    }
                }
            }

            if (Block.mb(p.level.GetTile(click[0], click[1], click[2])))
            { Player.SendMessage(p, "Ici mb, ne pas utiliser un /click dessus"); return; }

            p.manualChange(click[0], click[1], click[2], 0, Block.rock);
            Player.SendMessage(p, "Clic &b(" + click[0] + ", " + click[1] + ", " + click[2] + ")");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/click [x y z] - Simule un clic");
            Player.SendMessage(p, "Si aucun xyz est donne, il utilise le dernier point clique");
            Player.SendMessage(p, "/click 200 y 200 fait un clic aux coordonnes 200x, precedent y et 200z");
        }

        private bool isValid(string message, int dimension, Player p)
        {
            ushort testValue;
            try {
                testValue = ushort.Parse(message);
            } catch { return false; }

            if (testValue < 0)
                return false;

            if (testValue >= p.level.width && dimension == 0) return false;
            else if (testValue >= p.level.depth && dimension == 1) return false;
            else if (testValue >= p.level.height && dimension == 2) return false;

            return true;
        }
    }
}