
using System;
using System.Data;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdAbout : Command
    {
        public override string name { get { return "about"; } }
        public override string shortcut { get { return "b"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdAbout() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            Player.SendMessage(p, "Casse/construit un bloc pour avoir des infos sur le bloc.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(AboutBlockchange);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/about - Donne des informations sur un bloc.");
        }

        public void AboutBlockchange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            if (!p.staticCommands) p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            if (b == Block.Zero) { Player.SendMessage(p, "Bloc invalide (" + x + "," + y + "," + z + ")!"); return; }
            p.SendBlockchange(x, y, z, b);

            string message = "Block (" + x + "," + y + "," + z + "): ";
            message += "&f" + b + " = " + Block.Name(b);
            Player.SendMessage(p, message + Server.DefaultColor + ".");
            message = p.level.foundInfo(x, y, z);
            if (message != "") Player.SendMessage(p, "Physiques informations: &a" + message);

            DataTable Blocks = MySQL.fillData("SELECT * FROM `Block" + p.level.name + "." + p.level.world + "` WHERE X=" + (int)x + " AND Y=" + (int)y + " AND Z=" + (int)z);

            string Username, TimePerformed, BlockUsed;
            bool Deleted, foundOne = false;

            for (int i = 0; i < Blocks.Rows.Count; i++)
            {
                foundOne = true;
                Username = Blocks.Rows[i]["Username"].ToString();
                TimePerformed = DateTime.Parse(Blocks.Rows[i]["TimePerformed"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                BlockUsed = Block.Name((byte)Blocks.Rows[i]["Type"]).ToString();
                Deleted = (bool)Blocks.Rows[i]["Deleted"];

                if (!Deleted)
                    Player.SendMessage(p, "&3Cree par " + Server.FindColor(Username.Trim()) + Username.Trim() + Server.DefaultColor + ", using &3" + BlockUsed);
                else
                    Player.SendMessage(p, "&4Detruit par " + Server.FindColor(Username.Trim()) + Username.Trim() + Server.DefaultColor + ", using &3" + BlockUsed);
                Player.SendMessage(p, "Date et heure de modifications: &2" + TimePerformed);
            }

            List<Level.BlockPos> inCache = p.level.blockCache.FindAll(bP => bP.x == x && bP.y == y && bP.z == z);

            for (int i = 0; i < inCache.Count; i++)
            {
                foundOne = true;
                Deleted = inCache[i].deleted;
                Username = inCache[i].name;
                TimePerformed = inCache[i].TimePerformed.ToString("yyyy-MM-dd HH:mm:ss");
                BlockUsed = Block.Name(inCache[i].type);

                if (!Deleted)
                    Player.SendMessage(p, "&3Cree par " + Server.FindColor(Username.Trim()) + Username.Trim() + Server.DefaultColor + ", using &3" + BlockUsed);
                else
                    Player.SendMessage(p, "&4detruit par " + Server.FindColor(Username.Trim()) + Username.Trim() + Server.DefaultColor + ", using &3" + BlockUsed);
                Player.SendMessage(p, "Date et heure de modifications: &2" + TimePerformed);
            }

            if (!foundOne)
                Player.SendMessage(p, "Ce bloc n'a jamais ete modifie.");
 
            Blocks.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}