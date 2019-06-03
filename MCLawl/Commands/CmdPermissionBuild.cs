using System;

namespace MCWorlds
{
    public class CmdPermissionBuild : Command
    {
        public override string name { get { return "perbuild"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPermissionBuild() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if ( message.Split(' ').Length > 3) { Help(p); return; }

            if (message == "") { message = "list"; }

            Level lvl = p.level ;

            if (message.Split(' ')[0] == "list")
            {
                if (message.Split(' ').Length >= 3) { Help(p); return; }
                if (message.Split(' ').Length == 2) { lvl = Level.Find(message.Split(' ')[1], p.level.world); }

                if (lvl == null) { Player.SendMessage(p, "Map introuvable"); return; }

                if (lvl.perbuild)
                { Player.SendMessage(p, "Tout le monde peut construire sur cette map"); }

                string listepl = "";

                foreach (string pn in lvl.Perbuildliste)
                { listepl += ", " + pn; }

                if (listepl != "")
                {
                    listepl.Remove(0, 2);
                    Player.SendMessage(p, "Liste des joueurs permi a construire sur " + lvl.name + " : &2" + listepl);
                }
                else if (!lvl.perbuild) { Player.SendMessage(p, "Personne ne peut construire sur cette map"); }
                return;
            }

            if (p.level.world.ToLower() != p.name.ToLower() && p.group.Permission < LevelPermission.Admin)
            { Player.SendMessage(p, "Vous n'etes pas autoriser a utiliser cette commande dans ce monde"); return; }

            if (message.Split(' ')[0] == "all")
            {
                if (message.Split(' ').Length >= 3) { Help(p); return; }
                if (message.Split(' ').Length == 2) { lvl = Level.Find(message.Split(' ')[1], p.level.world); }
                if (lvl == null) { Player.SendMessage(p, "Map introuvable"); return; }

                bool perbuild = false;

                if (lvl.perbuild)
                {
                    lvl.perbuild = false;
                    foreach (Player pl in Player.players)
                    {
                        perbuild = false;
                        if (pl.level == lvl)
                        {
                            foreach (string pName in lvl.Perbuildliste)
                            {
                                if (pl.name.ToLower() == pName.ToLower()) { perbuild = true; }
                            }

                            if (pl.name.ToLower() == p.level.world.ToLower()) { perbuild = true; }

                            if (perbuild == false)
                            {
                                pl.perbuild = false;
                                Player.SendMessage(pl, "Vous ne pouvez plus construire dans cette map");
                            }
                        }
                    }
                    if (lvl == p.level)
                    { Player.SendMessage(p, "La permition generale de construire est maintenant desactive"); }
                    else { Player.SendMessage(p, "La permition generale de construire est maintenant desactive sur " + lvl.name); }
                    Server.s.Log("Perbuild all OFF sur " + lvl.name + " (" + lvl.world + ")");    
                }
                else
                {
                    lvl.perbuild = true;
                    foreach (Player pl in Player.players)
                    {
                        perbuild = true;
                        if (pl.level == lvl)
                        {
                            foreach (string pName in lvl.Perbuildliste)
                            {
                                if (pl.name.ToLower() == pName.ToLower())
                                    perbuild = false;
                            }

                            if (perbuild == true)
                            {
                                pl.perbuild = true;
                                Player.SendMessage(pl, "Vous pouvez maintenant construire sur cette map");
                            }
                        }
                    }
                    if (lvl == p.level)
                    { Player.SendMessage(p, "La permition generale de construire est maintenant active"); }
                    else { Player.SendMessage(p, "La permition generale de construire est maintenant active sur " + lvl.name); }
                    Server.s.Log("Perbuild all ON sur " + lvl.name + " (" + lvl.world + ")");
                }
                return;
            }
            else
            {
                if (message.Split(' ')[0] != "add" && message.Split(' ')[0] != "del") { Help(p); return; }

                if (message.IndexOf(' ') == -1) { Help(p); return; }

                string name = message.Split(' ')[1].ToLower();
                
                if (message.Split(' ').Length == 3) { lvl = Level.Find(message.Split(' ')[2], p.level.world); }
                if (lvl == null) { Player.SendMessage(p, "Map introuvable"); return; }

                bool playerfind = false ;

                foreach (string pName in lvl.Perbuildliste)
                {
                    if (pName.ToLower() == name)
                    { playerfind = true; }
                }

                if (playerfind)
                {
                    if (message.Split(' ')[0] == "add")
                    { Player.SendMessage(p, "Le joueur a deja la permition de construire dans cette map"); return; }

                    lvl.Perbuildliste.Remove(name);

                    foreach (Player pl in Player.players)
                    {
                        if (pl.name.ToLower() == name && pl.level == lvl)
                        {
                            pl.perbuild = false;
                            Player.SendMessage(pl, "Vous ne pouvez plus construire sur cette map");
                        }
                    }
                    Player.SendMessage(p, "Vous avez enlever la permition de construire a " + name + " sur " + lvl.name);
                    Server.s.Log("Perbuild " + name + " OFF sur " + lvl.name + " (" + lvl.world + ")");
                }
                else
                {
                    if (message.Split(' ')[0] == "del")
                    { Player.SendMessage(p, "Le joueur n'a pas les autorisations de construire"); return; }

                    lvl.Perbuildliste.Add(name);

                    foreach (Player pl in Player.players)
                    {
                        if (pl.name.ToLower() == name.ToLower() && pl.level == lvl)
                        {
                            pl.perbuild = true;
                            Player.SendMessage(pl, "Vous pouvez maintenant construire sur cette map");
                        }
                    }
                    Player.SendMessage(p, "Vous autorisez a " + name + " de construire sur " + lvl.name);
                    Server.s.Log("Perbuild " + name + " ON sur " + lvl.name + " (" + lvl.world + ")");
                }
            }

        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/perbuild add [joueur] <map> - Autorise au joueur de construire sur la map.");
            Player.SendMessage(p, "/perbuild del [joueur] <map> - Annule l'autorisitation de construire pour le joueur.");
            Player.SendMessage(p, "Le nom exacte doit etre donne");
            Player.SendMessage(p, "/perbuild all <map> - Permet a tous les joueurs de construire sur la map.");
            Player.SendMessage(p, "/perbuild list <map> - affiche la liste des joueurs autorise a construire");
            Player.SendMessage(p, "Si la map n'est pas indique, les modifications se feront sur la map actuelle");
            Player.SendMessage(p, "La map doit etre dans le meme monde que le votre");
        }
    }
}