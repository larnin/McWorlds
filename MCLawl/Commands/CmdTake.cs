using System;

namespace MCWorlds
{
    public class CmdTake : Command
    {
        public override string name { get { return "take"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdTake() { }

        public override void Use(Player p, string message)
        {
            if (message.IndexOf(' ') == -1) { Help(p); return; }
            if (message.Split(' ').Length != 2) { Help(p); return; }

            Player who = Player.Find(message.Split(' ')[0]);
            if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur entre"); return; }
            if (who == p) { Player.SendMessage(p, "Desole. Vous ne pouvez pas vous enlever de l'argent"); return; }

            int amountTaken;
            try { amountTaken = int.Parse(message.Split(' ')[1]); }
            catch { Player.SendMessage(p, "Valeur invalide"); return; }

            if (who.money - amountTaken < 0) { Player.SendMessage(p, "Le joueur ne peut pas avoir moins de 0 " + Server.moneys); return; }
            if (amountTaken < 0) { Player.SendMessage(p, "Impossible d'enlever une valeur negative " + Server.moneys); return; }

            who.money -= amountTaken;
            Player.GlobalMessage(who.color + who.prefix + who.Name() + Server.DefaultColor + " a perdu " + amountTaken + " " + Server.moneys);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/take [joueur] [valeur] - Enleve des " + Server.moneys + " au joueur.");
        }
    }
}