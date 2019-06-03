using System;

namespace MCWorlds
{
    public class CmdPossess : Command
    {
        public override string name { get { return "possess"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdPossess() { }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length > 2) { Help(p); return; }
            if (p == null) { Player.SendMessage(p, "Possession par la console ?  Nope.avi."); return; }
            try
            {
                string skin = (message.Split(' ').Length == 2) ? message.Split(' ')[1] : "";
                message = message.Split(' ')[0];
                if (message == "")
                {
                    if (p.possess == "")
                    {
                        Help(p);
                        return;
                    }
                    else
                    {
                        Player who = Player.Find(p.possess);
                        if (who == null)
                        {
                            p.possess = "";
                            Player.SendMessage(p, "Possession desactive.");
                            return;
                        }
                        who.following = "";
                        who.canBuild = true;
                        p.possess = "";
                        if (!who.MarkPossessed())
                        {
                            return;
                        }
                        p.invincible = false;
                        Command.all.Find("hide").Use(p, "");
                        Player.SendMessage(p, "Possession de " + who.color + who.name + Server.DefaultColor + " arette.");
                        return;
                    }
                }
                else if (message == p.possess)
                {
                    Player who = Player.Find(p.possess);
                    if (who == null)
                    {
                        p.possess = "";
                        Player.SendMessage(p, "Possession desactive.");
                        return;
                    }
                    if (who == p)
                    {
                        Player.SendMessage(p, "Impossible de vous posseder vous meme!");
                        return;
                    }
                    who.following = "";
                    who.canBuild = true;
                    p.possess = "";
                    if (!who.MarkPossessed())
                    {
                        return;
                    }
                    p.invincible = false;
                    Command.all.Find("hide").Use(p, "");
                    Player.SendMessage(p, "possession de " + who.color + who.name + Server.DefaultColor + " arette.");
                    return;
                }
                else
                {
                    Player who = Player.Find(message);
                    if (who == null)
                    {
                        Player.SendMessage(p, "Impossible de trouver le joueur.");
                        return;
                    }
                    if (who.group.Permission >= p.group.Permission)
                    {
                        Player.SendMessage(p, "Impossible de posseder un joueur de rang egal ou superieur au votre.");
                        return;
                    }
                    if (who.possess != "")
                    {
                        Player.SendMessage(p, "Ce joueur est deja possede!");
                        return;
                    }
                    if (who.following != "")
                    {
                        Player.SendMessage(p, "Ce joueur suis un joueur ou est deja possede.");
                        return;
                    }
                    if (p.possess != "")
                    {
                        Player oldwho = Player.Find(p.possess);
                        if (oldwho != null)
                        {
                            oldwho.following = "";
                            oldwho.canBuild = true;
                            if (!oldwho.MarkPossessed())
                            {
                                return;
                            }
                            //p.SendSpawn(oldwho.id, oldwho.color + oldwho.name, oldwho.pos[0], oldwho.pos[1], oldwho.pos[2], oldwho.rot[0], oldwho.rot[1]);
                        }
                    }
                    Command.all.Find("tp").Use(p, who.name);
                    if (!p.hidden)
                    {
                        Command.all.Find("hide").Use(p, "");
                    }
                    p.possess = who.name;
                    who.following = p.name;
                    if (!p.invincible)
                    {
                        p.invincible = true;
                    }
                    bool result = (skin == "#") ? who.MarkPossessed() : who.MarkPossessed(p.name);
                    if (!result)
                    {
                        return;
                    }
                    p.SendDie(who.id);
                    who.canBuild = false;
                    Player.SendMessage(p, "Possession de " + who.color + who.name + Server.DefaultColor + " reussi.");
                }
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
                Player.SendMessage(p, "Une erreur est arrive.");
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/possess <joueur> [#] - POSSETION DEMONIAQUE (niark)");
            Player.SendMessage(p, "Utilisez # apres le nom du joueur pour garder son skin pendant la possession.");
            Player.SendMessage(p, "Sinon la cible perd son skin et son nom change en \"joueur (votrenom)\".");
        }
    }
}
