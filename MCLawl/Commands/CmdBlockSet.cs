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
    public class CmdBlockSet : Command
    {
        public override string name { get { return "blockset"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdBlockSet() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }

            byte foundBlock = Block.Byte(message.Split(' ')[0]);
            if (foundBlock == Block.Zero) { Player.SendMessage(p, "Ne trouve pas le bloc entre"); return; }
            LevelPermission newPerm = Level.PermissionFromName(message.Split(' ')[1]);
            if (newPerm == LevelPermission.Null) { Player.SendMessage(p, "Ne trouve pas le rank entre"); return; }
            if (p != null && newPerm > p.group.Permission) { Player.SendMessage(p, "Impossible de definir a un rang plus superieur au votre."); return; }

            if (p != null && !Block.canPlace(p, foundBlock)) { Player.SendMessage(p, "Impossible de modifier un bloc d'un pour un grade superieur au votre"); return; }

            Block.Blocks newBlock = Block.BlockList.Find(bs => bs.type == foundBlock);
            newBlock.lowestRank = newPerm;

            Block.BlockList[Block.BlockList.FindIndex(bL => bL.type == foundBlock)] = newBlock;

            Block.SaveBlocks(Block.BlockList);

            Player.GlobalMessage("La permition du bloc &d" + Block.Name(foundBlock) + Server.DefaultColor + " a ete change en " + Level.PermissionToName(newPerm));
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/blockset [bloc] [rang] - Change le rang minimum du bloc");
            Player.SendMessage(p, "Seulement les blocs que vous pouvez utiliser peuvent etre modifier");
            Player.SendMessage(p, "Rang disponibles: " + Group.concatList());
        }
    }
}