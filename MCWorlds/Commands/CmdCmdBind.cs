using System;

namespace MCWorlds
{
    public class CmdCmdBind : Command
    {
        public override string name { get { return "cmdbind"; } }
        public override string shortcut { get { return "cb"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdCmdBind() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            string foundcmd, foundmessage = ""; int foundnum = 0;

            if (message.IndexOf(' ') == -1)
            {
                bool OneFound = false;
                for (int i = 0; i < 10; i++)
                {
                    if (p.cmdBind[i] != null)
                    {
                        Player.SendMessage(p, "&c/" + i + Server.DefaultColor + " lier a &b" + p.cmdBind[i] + " " + p.messageBind[i]);
                        OneFound = true;
                    }
                }
                if (!OneFound) Player.SendMessage(p, "Vous n'avez pas de commandes lier");
                return;
            }

            if (message.Split(' ').Length == 1)
            {
                try
                {
                    foundnum = Convert.ToInt16(message);
                    if (p.cmdBind[foundnum] == null) { Player.SendMessage(p, "Pas de commande enregistre ici."); return; }
                    foundcmd = "/" + p.cmdBind[foundnum] + " " + p.messageBind[foundnum];
                    Player.SendMessage(p, "Commande enregistre: &b" + foundcmd);
                }
                catch { Help(p); }
            }
            else if (message.Split(' ').Length > 1)
            {
                try
                {
                    foundnum = Convert.ToInt16(message.Split(' ')[message.Split(' ').Length - 1]);
                    foundcmd = message.Split(' ')[0];
                    if (message.Split(' ').Length > 2)
                    {
                        foundmessage = message.Substring(message.IndexOf(' ') + 1);
                        foundmessage = foundmessage.Remove(foundmessage.LastIndexOf(' '));
                    }

                    p.cmdBind[foundnum] = foundcmd;
                    p.messageBind[foundnum] = foundmessage;

                    Player.SendMessage(p, "Lie &b/" + foundcmd + " " + foundmessage + " a &c/" + foundnum);
                }
                catch { Help(p); }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/cmdbind [commande] [num] - Lie une commande a un numero");
            Player.SendMessage(p, "[num] doit etre entre 0 et 9");
            Player.SendMessage(p, "Reutilise la commande avec '/[num]'");
            Player.SendMessage(p, "/cmdbind - liste toutes les commandes lie");
            Player.SendMessage(p, "utilise /cmdbind [num] pour voir la commande enregistre.");
        }
    }
}