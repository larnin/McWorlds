using System;
using System.IO;

namespace MCWorlds
{
    public class CmdMap : Command
    {
        public override string name { get { return "map"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMap() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") message = p.level.name;

            Level foundLevel;

            if (message.IndexOf(' ') == -1)
            {
                foundLevel = Level.Find(message, p.level.world);
                if (foundLevel == null)
                {
                    if (p != null)
                    {
                        foundLevel = p.level;
                    }
                }
                else
                {
                    Player.SendMessage(p, "MOTD: &b" + foundLevel.motd);
                    Player.SendMessage(p, "Mode Finite: " + FoundCheck(foundLevel.finite));
                    Player.SendMessage(p, "IA animaux: " + FoundCheck(foundLevel.ai));
                    Player.SendMessage(p, "Bord d'eau: " + FoundCheck(foundLevel.edgeWater));
                    Player.SendMessage(p, "pousse de l'herbe: " + FoundCheck(foundLevel.GrassGrow));
                    Player.SendMessage(p, "Vitesse physics: &b" + foundLevel.speedPhysics);
                    Player.SendMessage(p, "Surcharge physics: &b" + foundLevel.overload);
                    Player.SendMessage(p, "Mode survie: " + FoundCheck(foundLevel.Death) + "(Chute: " + foundLevel.fall + ", Noye: " + foundLevel.drown + ")");
                    Player.SendMessage(p, "Blocs mortels: " + FoundCheck(foundLevel.Killer));
                    Player.SendMessage(p, "Dechargement: " + FoundCheck(foundLevel.unload));
                    Player.SendMessage(p, "Physics auto: " + FoundCheck(foundLevel.rp));
                    Player.SendMessage(p, "Construction instantanee: " + FoundCheck(foundLevel.Instant));
                    Player.SendMessage(p, "Tchat local: " + FoundCheck(!foundLevel.worldChat));
                    return;
                }
            }
            else
            {
                foundLevel = Level.Find(message.Split(' ')[0], p.level.world);

                if (foundLevel == null || message.Split(' ')[0].ToLower() == "ps" || message.Split(' ')[0].ToLower() == "rp") foundLevel = p.level;
                else message = message.Substring(message.IndexOf(' ') + 1);
            }

            if (p != null)
                if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "La modifications des options est reserve aux OP+"); return; }

            string foundStart;
            if (message.IndexOf(' ') == -1) foundStart = message.ToLower();
            else foundStart = message.Split(' ')[0].ToLower();

            try
            {
                switch (foundStart)
                {
                    case "theme": foundLevel.theme = message.Substring(message.IndexOf(' ') + 1); foundLevel.ChatLevel("Theme map: &b" + foundLevel.theme); break;
                    case "finite": foundLevel.finite = !foundLevel.finite; foundLevel.ChatLevel("Mode finite: " + FoundCheck(foundLevel.finite)); break;
                    case "ai": foundLevel.ai = !foundLevel.ai; foundLevel.ChatLevel("IA animaux: " + FoundCheck(foundLevel.ai)); break;
                    case "edge": foundLevel.edgeWater = !foundLevel.edgeWater; foundLevel.ChatLevel("Bord d'eau: " + FoundCheck(foundLevel.edgeWater)); break;
                    case "grass": foundLevel.GrassGrow = !foundLevel.GrassGrow; foundLevel.ChatLevel("pousse de l'herbe: " + FoundCheck(foundLevel.GrassGrow)); break;
                    case "ps":
                    case "physicspeed":
                        if (int.Parse(message.Split(' ')[1]) < 10) { Player.SendMessage(p, "Impossible en dessous de 10"); return; }
                        foundLevel.speedPhysics = int.Parse(message.Split(' ')[1]);
                        foundLevel.ChatLevel("Vitesse physics: &b" + foundLevel.speedPhysics);
                        break;
                    case "overload":
                        if (int.Parse(message.Split(' ')[1]) < 500) { Player.SendMessage(p, "Impossible en dessous de 500 (1500 par defaut)"); return; }
                        if (p.group.Permission < LevelPermission.Admin && int.Parse(message.Split(' ')[1]) > 2500) { Player.SendMessage(p, "Seullement les SuperOPs peuvent mettre plus de 2500"); return; }
                        foundLevel.overload = int.Parse(message.Split(' ')[1]);
                        foundLevel.ChatLevel("Surcharge physics: &b" + foundLevel.overload);
                        break;
                    case "motd":
                        if (message.Split(' ').Length == 1) foundLevel.motd = "ignore";
                        else foundLevel.motd = message.Substring(message.IndexOf(' ') + 1);
                        foundLevel.ChatLevel("MOTD map: &b" + foundLevel.motd);
                        break;
                    case "death": foundLevel.Death = !foundLevel.Death; foundLevel.ChatLevel("Mode survie: " + FoundCheck(foundLevel.Death)); break;
                    case "killer": foundLevel.Killer = !foundLevel.Killer; foundLevel.ChatLevel("Blocs mortels: " + FoundCheck(foundLevel.Killer)); break;
                    case "fall": foundLevel.fall = int.Parse(message.Split(' ')[1]); foundLevel.ChatLevel("Distance de chute: &b" + foundLevel.fall); break;
                    case "drown": foundLevel.drown = int.Parse(message.Split(' ')[1]) * 10; foundLevel.ChatLevel("Temps de noyade: &b" + (foundLevel.drown / 10)); break;
                    case "unload": foundLevel.unload = !foundLevel.unload; foundLevel.ChatLevel("chargement auto: " + FoundCheck(foundLevel.unload)); break;
                    case "rp":
                    case "restartphysics": foundLevel.rp = !foundLevel.rp; foundLevel.ChatLevel("Physics auto: " + FoundCheck(foundLevel.rp)); break;
                    case "instant":
                        if (p.group.Permission < LevelPermission.Admin) { Player.SendMessage(p, "C'est reserve aux admin"); return; }
                        foundLevel.Instant = !foundLevel.Instant; foundLevel.ChatLevel("Construction instantanee: " + FoundCheck(foundLevel.Instant)); break;
                    case "chat":
                        foundLevel.worldChat = !foundLevel.worldChat; foundLevel.ChatLevel("Tchat local: " + FoundCheck(!foundLevel.worldChat)); break;
                    default:
                        Player.SendMessage(p, "Impossible de trouver l'option entre.");
                        return;
                }
                foundLevel.changed = true;
                if (p.level != foundLevel) Player.SendMessage(p, "/map termine!");
            }
            catch { Player.SendMessage(p, "ENTREE INVALIDE"); }
        }
        public string FoundCheck(bool check)
        {
            if (check) return "&aON";
            else return "&cOFF";
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/map <map> [option] <parametre> - Modifie option sur une map");
            Player.SendMessage(p, "Vous devez etre dans le meme monde que la map pour voir ou modifier ses options");
            Player.SendMessage(p, "Options possibles: theme, finite, ai, edge, ps, overload, motd, death, fall, drown, unload, rp, instant, killer, chat");
            Player.SendMessage(p, "Edge : modifie l'ecoulement de l'eau.");
            Player.SendMessage(p, "Grass : Permet a l'herbe de ne pas pousser sans la physics");
            Player.SendMessage(p, "Finite : Tous les liquides sont finite.");
            Player.SendMessage(p, "AI : Les animaux vous suivent ou vous fuient.");
            Player.SendMessage(p, "PS : Change la vitesse de la physics.");
            Player.SendMessage(p, "Overload : Fixe les limites de puissances de la physics.");
            Player.SendMessage(p, "MOTD : Change le motd de la map.(mettre un blanc pour reset)");
            Player.SendMessage(p, "Death : Active/desactive le mode survie (chute, noyade)");
            Player.SendMessage(p, "Fall/drown : Change la distance/temps avant de mourir.");
            Player.SendMessage(p, "Killer : Active/desactive les blocs mortels.");
            Player.SendMessage(p, "Unload : Definit si la map se decharge si personne est dessus.");
            Player.SendMessage(p, "RP : Definit si la physics demarre automatiquement.");
            Player.SendMessage(p, "Instant : n'affiche pas toutes les modifications des joueurs.");
            Player.SendMessage(p, "Chat : Le tchat ne se fait plus avec les autres maps.");
        }
    }
}