using System;

namespace MCWorlds
{
    public class CmdGive : Command
    {
        public override string name { get { return "give"; } }
        public override string shortcut { get { return "gib"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdGive() { }

        public override void Use(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (message.Split(' ').Length != 2) { Help(p); return; }

            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur entre"); return; }
            if (who == p) { Player.SendMessage(p, "Desole. Vous ne pouvez pas vous donner " + Server.moneys + " vous meme"); return; }

            int amountGiven;
            try { amountGiven = int.Parse(message.Split(' ')[1]); }
            catch { Player.SendMessage(p, "Valeur non valide"); return; }

            if (who.money + amountGiven > 16777215) { Player.SendMessage(p, "Un joueur ne peut pas avoir plus de 16777215 " + Server.moneys); return; }
            if (amountGiven < 0) { Player.SendMessage(p, "Vous ne pouvez pas donner un nombre negatif de " + Server.moneys); return; }

            who.money += amountGiven;
            Player.GlobalMessage(who.color + who.prefix + who.Name() + Server.DefaultColor + " a gagne " + amountGiven + " " + Server.moneys);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/give [joueur] [" + Server.moneys + "] - Donne des " + Server.moneys + " au joueur");
        }
    }
}