using System;

namespace MCWorlds
{
    public class CmdPay : Command
    {
        public override string name { get { return "pay"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdPay() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (message.Split(' ').Length != 2) { Help(p); return; }

            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur entre"); return; }
            
            int amountPaid;
            try { amountPaid = int.Parse(message.Split(' ')[1]); }
            catch { Player.SendMessage(p, "Quantitee invalide"); return; }

            if (who.money + amountPaid > 16777215) { Player.SendMessage(p, "Les joueurs ne peuvent pas avoir plus de 16777215 " + Server.moneys); return; }
            if (p.money - amountPaid < 0) { Player.SendMessage(p, "Vous n'avez pas assez de " + Server.moneys); return; }
            if (amountPaid < 0) { Player.SendMessage(p, "Impossible de payer en negatif "); return; }

            who.money += amountPaid;
            p.money -= amountPaid;
            Player.GlobalMessage(p.color + p.Name() + Server.DefaultColor + " paye "  + amountPaid + " " + Server.moneys + " a " + who.color + who.Name() + Server.DefaultColor + ".");
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pay [joueur] [valeur] - Vous permet de payer un autre joueur");
        }
    }
}