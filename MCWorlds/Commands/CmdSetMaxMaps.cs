using System;

namespace MCWorlds
{
    public class CmdSetMaxMaps : Command
    {
        public override string name { get { return "setmaxmaps"; } }
        public override string shortcut { get { return "smm"; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdSetMaxMaps() { }

        public override void Use(Player p, string message)
        {
            if (p != null) { if (p.group.Permission < LevelPermission.Admin) { Player.SendMessage(p, "Vous n'avez pas les droits pour faire ca"); return; } }

            if (message.IndexOf(' ') == -1 || message.Split(' ').Length > 2) { Help(p); return; }

            Player who = null;
            who = Player.Find(message.Split(' ')[0]);

            if (who == null) { Player.SendMessage(p, "Joueur introuvable"); return; }

            string action = message.Split(' ')[1];

            if (action == "del")
            {
                if (who.nbMapsMax == 1) { Player.SendMessage(p, "Le joueur ne peut pas avoir moins de 5 maps"); return; }
                who.nbMapsMax--;

                Player.SendMessage(p, "Vous enlevez une map a " + who.name + ", il lui en reste " + who.nbMapsMax);
                Player.SendMessage(who, "Vous perdez une map, il vous en reste " + who.nbMapsMax);
            }
            else if (action == "add")
            {
                if (who.nbMapsMax == 100) { Player.SendMessage(p, "Le joueur ne peut pas avoir plus de 50 maps"); return; }
                who.nbMapsMax++;

                Player.SendMessage(p, "Vous ajoutez une map a " + who.name + ", il en a maintenant " + who.nbMapsMax);
                Player.SendMessage(who, "Vous gagnez une map, vous en avez maintenant " + who.nbMapsMax);
            }
            else
            {
                int nb = 0;
                try { nb = int.Parse(action); }
                catch { Player.SendMessage(p, "Valeur incorrecte"); }

                if (nb < 1) { Player.SendMessage(p, "Un joueur ne peut pas avoir moins une maps"); }
                if (nb > 100) { Player.SendMessage(p, "Un joueur ne peut pas avoir plus de 100 maps"); }

                who.nbMapsMax = nb;
                Player.SendMessage(p, "Vous fixez la limite de " + who.name + " a " + nb + " maps");
                Player.SendMessage(who, "Votre limite de maps a ete fixee a " + nb);
            }
            who.save();
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/setmaxmaps [joueur] [nombre] - Change le nombre max de maps que le joueur peut avoir.");
            Player.SendMessage(p, "/setmaxmaps [joueur] add - Ajoute une map au joueur");
            Player.SendMessage(p, "/setmaxmaps [joueur] del - Enleve une map au joueur");
        }
    }
}