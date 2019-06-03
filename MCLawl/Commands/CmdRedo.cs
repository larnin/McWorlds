using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdRedo : Command
    {
        public override string name { get { return "redo"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRedo() { }

        public override void Use(Player p, string message)
        {
            if (message != "") { Help(p); return; }
            byte b;

            p.RedoBuffer.ForEach(delegate(Player.UndoPos Pos)
            {
                Level foundLevel = Level.FindExact(Pos.mapName, Pos.worldName);
                if (foundLevel != null)
                {
                    b = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);
                    foundLevel.Blockchange(Pos.x, Pos.y, Pos.z, Pos.type);
                    Pos.newtype = Pos.type;
                    Pos.type = b;
                    Pos.timePlaced = DateTime.Now;
                    p.UndoBuffer.Add(Pos);
                }
            });

            Player.SendMessage(p, "Reconstruction termine.");
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/redo - Reconstruit l'annulation que vous venez de faire.");
            Player.SendMessage(p, "Voir /undo pour plus d'infos");
        }
    }
}