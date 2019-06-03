using System;
using System.IO;

namespace MCWorlds
{
    public class CmdBotRemove : Command
    {
        public override string name { get { return "botremove"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public string[,] botlist;
        public CmdBotRemove() { }

        public override void Use(Player p, string message)
        {
            if (message == "") { Help(p); return; }
            try
            {
                if (message.ToLower() == "all")
                {
                    for (int i = 0; i < PlayerBot.playerbots.Count; i++)
                    {
                        if (PlayerBot.playerbots[i].level == p.level)
                        {
                            //   PlayerBot.playerbots.Remove(PlayerBot.playerbots[i]);
                            PlayerBot Pb = PlayerBot.playerbots[i];
                            Pb.removeBot();
                            i--;
                        }
                    }
                    Player.SendMessage(p, "Tous les bots sont supprime");
                }
                else
                {
                    PlayerBot who = PlayerBot.Find(message);
                    if (who == null) { Player.SendMessage(p, "Il n'y a pas de bot " + who + "!"); return; }
                    if (p.level != who.level) { Player.SendMessage(p, who.name + " est dans une map differente."); return; }
                    who.removeBot();
                    Player.SendMessage(p, "Bot supprime.");
                }
            }
            catch (Exception e) { Server.ErrorLog(e); Player.SendMessage(p, "Erreur"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/botremove [nom] - Enleve un bot qui est sur la meme map que vous");
            Player.SendMessage(p, "/botremove all - Supprime tous les bots de la map");
        }
    }
}