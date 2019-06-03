using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdNewLvl : Command
    {
        public override string name { get { return "newlvl"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdNewLvl() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.group.Permission < LevelPermission.Admin && p.level.world.ToLower() != p.name.ToLower())
            { Player.SendMessage(p, "Vous ne pouvez pas creer de map dans ce monde"); return; }

            if (!Directory.Exists("levels/" + p.name.ToLower())) { Player.SendMessage(p, "Votre monde n'existe pas, creez le avec /home"); return; }

            if (p.nbMaps >= p.nbMapsMax && p.group.Permission < LevelPermission.Admin)
            { Player.SendMessage(p, "Vous ne pouvez plus creer de maps, vous avez attein votre limite"); return; }

            if (message == "") { Help(p); return; }

            if (message.IndexOf(' ') == -1) { message += " 128 64 128 flat"; }

            string[] parameters = message.Split(' '); // Grab the parameters from the player's message
            if (parameters.Length == 5) // make sure there are 5 params
            {
                switch (parameters[4])
                {
                    case "flat":
                    case "pixel":
                    case "island":
                    case "mountains":
                    case "ocean":
                    case "forest":
                    case "desert":
                        break;

                    default:
                        Player.SendMessage(p, "Types valides: island, mountains, forest, ocean, flat, pixel, desert"); return;
                }

                string name = parameters[0].ToLower();
                ushort x = 1, y = 1, z = 1;
                
                if (name.Length > 16) { Player.SendMessage(p, "Le nom de la map doit faire moins de 16 caracteres"); return; }
                
                try
                {
                    x = Convert.ToUInt16(parameters[1]);
                    y = Convert.ToUInt16(parameters[2]);
                    z = Convert.ToUInt16(parameters[3]);
                }
                catch { Player.SendMessage(p, "Dimentions invalides."); return; }
                if (!isGood(x)) { Player.SendMessage(p, x + " n'est pas une bonne dimension! Utilisez une puissance de 2 la prochaine fois."); }
                if (!isGood(y)) { Player.SendMessage(p, y + " n'est pas une bonne dimension! Utilisez une puissance de 2 la prochaine fois."); }
                if (!isGood(z)) { Player.SendMessage(p, z + " n'est pas une bonne dimension! Utilisez une puissance de 2 la prochaine fois."); }

                if (!Player.ValidName(name)) { Player.SendMessage(p, "Nom invalide!"); return; }

                if (x > 1024 || y > 1024 || z > 1024)
                { Player.SendMessage(p, "Dimentions trop grandes, prenez des valeurs inferieurs a 1024"); return; }

                try
                {
                    if (p != null)
                    if (p.group.Permission < LevelPermission.Vip)
                    {
                        if (x * y * z > 16777216) { Player.SendMessage(p, "Vous ne pouvez pas creer une map de plus de 16millions de blocs."); return; }
                    }
                    else if (p.group.Permission < LevelPermission.Operator)
                    {
                        if (x * y * z > 33554432) { Player.SendMessage(p, "Vous ne pouvez pas creer une map de plus de 33millions de blocs."); return; }
                    }
                    else if (p.group.Permission < LevelPermission.Admin)
                    {
                        if (!p.vip && x * y * z > 33554432) { Player.SendMessage(p, "Vous ne pouvez pas creer une map de plus de 33millions de blocs."); return; }
                        if (x * y * z > 67108864) { Player.SendMessage(p, "Vous ne pouvez pas creer une map de plus de 67millions de blocs."); return; }
                    }
                    else
                    {
                        if (x * y * z > 225000000) { Player.SendMessage(p, "Vous ne pouvez pas creer une map de plus de 225millions de blocs."); return; }
                    }
                }
                catch 
                { 
                    Player.SendMessage(p, "Erreur!"); 
                }

                if (name.IndexOf('.') != -1)
                {
                    name = name.Replace(".", "_");
                    Player.SendMessage(p, "Remplacement automatique des points par des tirets");
                }

                if (File.Exists("levels/" + p.level.world + "/" + name + ".lvl")) { Player.SendMessage(p, "La map existe deja"); return; } 

                string world = p.level.world;
                
                // create a new level...
                try
                {
                    Level lvl = new Level(name, x, y, z, parameters[4]);
                    lvl.world = world;
                    lvl.Save(true); //... and save it.
                }
                finally
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                
                foreach (Player pl in Player.players)
                {
                    if (pl.level.world == p.level.world) { Player.SendMessage(pl, "La map " + name + " vient d'etre creee"); } // The player needs some form of confirmation.
                }

                if (p.level.world.ToLower() == p.name.ToLower()) { p.nbMaps++; }
                else
                {
                    Player who = Player.Find(p.level.world.ToLower());
                    if ( who != null) { who.nbMaps ++;}
                }
            }
            else
                Help(p);
        }
        public override void Help(Player p, string message = "")
        {
            if (message == "types")
            {
                Player.SendMessage(p, "island - Une ile entouree d'eau");
                Player.SendMessage(p, "mountains - Un paysage avec de grandes montagnes et quelques fois un lac");
                Player.SendMessage(p, "forest - Une map couverte d'arbres");
                Player.SendMessage(p, "ocean - Un grand ocean, pour les constructions sous marines");
                Player.SendMessage(p, "flat - Une map plate couverte d'herbe");
                Player.SendMessage(p, "pixel - Une map avec seulement 4 grands murs pour creer des pixelarts");
                Player.SendMessage(p, "desert - Un desert avec des cactus");
                return;
            }
            Player.SendMessage(p, "/newlvl [nom map] [x] [y] [z] [type] - Cree une nouvelle map.");
            Player.SendMessage(p, "/newlvl [nom map] - Permet de creer une map de taille et type standart.");
            Player.SendMessage(p, "Types valides: island, mountains, forest, ocean, flat, pixel, desert");
            Player.SendMessage(p, "/help newlvl types - Pour plus d'infos sur les types de maps");
        }

        public bool isGood(ushort value)
        {
            switch (value)
            {
                case 2:
                case 4:
                case 8:
                case 16:
                case 32:
                case 64:
                case 128:
                case 256:
                case 512:
                case 1024:
                case 2048:
                case 4096:
                case 8192:
                    return true;
            }

            return false;
        }
    }
}
