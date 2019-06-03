using System;
using System.Collections.Generic;
using System.Data;
//using MySql.Data.MySqlClient;
//using MySql.Data.Types;

namespace MCWorlds
{
    public class CmdMessageBlock : Command
    {
        public override string name { get { return "mb"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMessageBlock() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }

            CatchPos cpos;
            cpos.message = "";

            try
            {
                switch (message.Split(' ')[0])
                {
                    case "air": cpos.type = Block.MsgAir; break;
                    case "water": cpos.type = Block.MsgWater; break;
                    case "lava": cpos.type = Block.MsgLava; break;
                    case "black": cpos.type = Block.MsgBlack; break;
                    case "white": cpos.type = Block.MsgWhite; break;
                    case "show": showMBs(p); return;
                    default: cpos.type = Block.MsgWhite; cpos.message = message; break;
                }
            }
            catch { cpos.type = Block.MsgWhite; cpos.message = message; }

            if (cpos.message == "") cpos.message = message.Substring(message.IndexOf(' ') + 1);
            p.blockchangeObject = cpos;
            
            if (cpos.message[0] == '/')
            {
                if (p.group.Permission < LevelPermission.Operator)
                {
                    Player.SendMessage(p, "Les mb fonctions sont reserve aux op+.");
                    return;
                }
                Command cmd =  Command.all.Find(cpos.message.Split(' ')[0].Remove(0,1));
                if (cmd != null)
                {
                    LevelPermission perm = GrpCommands.allowedCommands.Find(grpComm => grpComm.commandName == cmd.name).lowestRank;
                    if (perm > LevelPermission.Operator)
                    { Player.SendMessage(p, "Cette commande ne peut pas etre mise en mb"); return; }
                }
            }

            if (message.Length > 255)
            {
                Player.SendMessage(p, "Message trop long (superieur a 255 caracteres) suppretion du surplus");
                cpos.message = cpos.message.Remove(256);
            }

            Player.SendMessage(p, "Placez un bloc pour determiner la position du message.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/mb <bloc> [message] - Place un message dans un bloc.");
            Player.SendMessage(p, "Blocs valides: white, black, air, water, lava");
            Player.SendMessage(p, "/mb <bloc> /[commande] - Place un mb pouvant executer des commandes (MODO+)");
            Player.SendMessage(p, "/mb show : affiche ou cache les MB");
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            CatchPos cpos = (CatchPos)p.blockchangeObject;

            cpos.message = cpos.message.Replace("'", "\\'");

            DataTable Messages = MySQL.fillData("SELECT * FROM `Messages" + p.level.name + "." + p.level.world + "` WHERE X=" + (int)x + " AND Y=" + (int)y + " AND Z=" + (int)z);
            Messages.Dispose();

            if (Messages.Rows.Count == 0)
            {
                MySQL.executeQuery("INSERT INTO `Messages" + p.level.name + "." + p.level.world + "` (X, Y, Z, Message) VALUES (" + (int)x + ", " + (int)y + ", " + (int)z + ", '" + cpos.message + "')");
            }
            else
            {
                MySQL.executeQuery("UPDATE `Messages" + p.level.name + "." + p.level.world + "` SET Message='" + cpos.message + "' WHERE X=" + (int)x + " AND Y=" + (int)y + " AND Z=" + (int)z);
            }

            Player.SendMessage(p, "Bloc message place.");
            p.level.Blockchange(p, x, y, z, cpos.type);
            p.SendBlockchange(x, y, z, cpos.type);

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        struct CatchPos { public string message; public byte type; }

        public void showMBs(Player p)
        {
            p.showMBs = !p.showMBs;

            DataTable Messages = new DataTable("Messages");
            Messages = MySQL.fillData("SELECT * FROM `Messages" + p.level.name + "." + p.level.world + "`");

            int i;

            if (p.showMBs)
            {
                for (i = 0; i < Messages.Rows.Count; i++)
                    p.SendBlockchange((ushort)Messages.Rows[i]["X"], (ushort)Messages.Rows[i]["Y"], (ushort)Messages.Rows[i]["Z"], Block.MsgWhite);
                Player.SendMessage(p, "Affiche &a" + i.ToString() + Server.DefaultColor + " MBs.");
            }
            else
            {
                for (i = 0; i < Messages.Rows.Count; i++)
                    p.SendBlockchange((ushort)Messages.Rows[i]["X"], (ushort)Messages.Rows[i]["Y"], (ushort)Messages.Rows[i]["Z"], p.level.GetTile((ushort)Messages.Rows[i]["X"], (ushort)Messages.Rows[i]["Y"], (ushort)Messages.Rows[i]["Z"]));
                Player.SendMessage(p, "Cache les MBs.");
            }
            Messages.Dispose();
        }
    }
}