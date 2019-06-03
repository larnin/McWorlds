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
using System.IO;

namespace MCWorlds
{
    public class CmdBotSummon : Command
    {
        public override string name { get { return "botsummon"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBotSummon() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }
            PlayerBot who = PlayerBot.Find(message);
            if (who == null) { Player.SendMessage(p, "Il n'y a pas de bot" + message + "!"); return; }
            if (p.level != who.level) { Player.SendMessage(p, who.name + " est dans une map differente."); return; }
            who.SetPos(p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/botsummon [nom] - Deplace le bot a votre position.");
        }
    }
}