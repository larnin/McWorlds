using System;
using System.IO;

namespace MCWorlds
{
    public class CmdFollow : Command
    {
        public override string name { get { return "follow"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdFollow() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (!p.canBuild)
            {
                Player.SendMessage(p, "Vous etes deja en train de &4hanter " + Server.DefaultColor + " un joueur !");
                return;
            }
            try
            {
                bool stealth = false;

                if (message != "")
                {
                    if (message == "#")
                    {
                        if (p.following != "")
                        {
                            stealth = true;
                            message = "";
                        }
                        else
                        {
                            Help(p);
                            return;
                        }
                    }
                    else if (message.IndexOf(' ') != -1)
                    {
                        if (message.Split(' ')[0] == "#")
                        {
                            if (p.hidden) stealth = true;
                            message = message.Split(' ')[1];
                        }
                    }
                }

                Player who = Player.Find(message);
                if (message == "" && p.following == "") {
                    Help(p);
                    return;
                }
                else if (message == "" && p.following != "" || message == p.following)
                {
                    who = Player.Find(p.following);
                    p.following = "";
                    if (p.hidden)
                    {
                        if (who != null)
                            p.SendSpawn(who.id, who.color + who.name, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1]);
                        if (!stealth)
                        {
                            Command.all.Find("hide").Use(p, "");
                        }
                        else
                        {
                            if (who != null)
                            {
                                Player.SendMessage(p, "Vous suivez " + who.color + who.name + Server.DefaultColor + " et devenez invisible.");
                            }
                            else
                            {
                                Player.SendMessage(p, "Suivage arette.");
                            }
                        }
                        return;
                    }
                }
                if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur."); return; }
                else if (who == p) { Player.SendMessage(p, "Impossible de vous suivre vous meme."); return; }
                else if (who.group.Permission >= p.group.Permission) { Player.SendMessage(p, "Impossible de suivre une personne de rang egal ou superieur."); return; }
                else if (who.following != "") { Player.SendMessage(p, who.name + " est deja suivi par " + who.following); return; }

                if (!p.hidden) Command.all.Find("hide").Use(p, "");

                if (p.level != who.level) Command.all.Find("tp").Use(p, who.name);
                if (p.following != "")
                {
                    who = Player.Find(p.following);
                    p.SendSpawn(who.id, who.color + who.name, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1]);
                }
                who = Player.Find(message);
                p.following = who.name;
                Player.SendMessage(p, "Suivie demarre " + who.name + ". Utilise \"/follow\" pour aretter.");
                p.SendDie(who.id);
            }
            catch (Exception e) { Server.ErrorLog(e); Player.SendMessage(p, "Erreur"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/follow [nom] - Suivre un joueur, reutilise la commande pour arreter");
            Player.SendMessage(p, "/follow # [nom] - Permet de ne pas activer /hide");
        }
    }
}