using System;

namespace MCWorlds
{
    public class CmdHelp : Command
    {
        public override string name { get { return "help"; } }
        public override string shortcut { get { return "?"; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public CmdHelp() { }

        public override void Use(Player p, string message)
        {
            try
            {
                string param = "";
                if (message.IndexOf(' ') != -1)
                {
                    param = message.Substring(message.IndexOf(' ') + 1); ;
                    message = message.Split(' ')[0];
                }

                message.ToLower();
                switch (message)
                {
                    case "":
                        if (Server.oldHelp)
                        {
                            goto case "old";
                        }
                        else
                        {
                            Player.SendMessage(p, "&b/help ranks" + Server.DefaultColor + " : Pour une liste des rangs.");
                            Player.SendMessage(p, "&b/help build" + Server.DefaultColor + " : Pour la liste des commandes des construction.");
                            Player.SendMessage(p, "&b/help mod" + Server.DefaultColor + " : Pour la liste des commandes de moderation.");
                            Player.SendMessage(p, "&b/help information" + Server.DefaultColor + " : Pour la liste des commandes d'information.");
                            Player.SendMessage(p, "&b/help jeux" + Server.DefaultColor + " : Pour la liste des commandes de jeu."); 
                            Player.SendMessage(p, "&b/help other" + Server.DefaultColor + " : Pour la liste des autres commandes.");
                            Player.SendMessage(p, "&b/help short" + Server.DefaultColor + " : Pour la liste des racourcits.");
                            Player.SendMessage(p, "&b/help old" + Server.DefaultColor + " : Pour voir l'ancien menu d'aide.");
                            Player.SendMessage(p, "&b/help [commande] ou /help [bloc] " + Server.DefaultColor + " : Pour avoir plus d'infos.");
                            Player.SendMessage(p, "Serveur traduit par &cnico69");
                            Player.SendMessage(p, "Si vous trouvez des erreurs ou avez des suggestions merci de les faire parvenir");
                        } break;
                    case "ranks":
                        message = "";
                        foreach (Group grp in Group.GroupList)
                        {
                            if (grp.name != "nobody")
                                Player.SendMessage(p, grp.color + grp.name + " - &bLimite de commandes: " + grp.maxBlocks + " blocs - &cPermission: " + (int)grp.Permission);
                        }
                        break;
                    case "build":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.type.Contains("build")) message += ", " + getColor(comm.name) + comm.name;
                            }
                        }

                        if (message == "") { Player.SendMessage(p, "Aucunes commandes dans cette categorie est disponible pour vous."); break; }
                        Player.SendMessage(p, "Les commandes de construction que vous pouvez utiliser:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "mod": case "moderation":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.type.Contains("mod")) message += ", " + getColor(comm.name) + comm.name;
                            }
                        }

                        if (message == "") { Player.SendMessage(p, "Aucunes commandes dans cette categorie est disponible pour vous."); break; }
                        Player.SendMessage(p, "Les commandes de moderation que vous pouvez utiliser:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "information":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.type.Contains("info")) message += ", " + getColor(comm.name) + comm.name;
                            }
                        }

                        if (message == "") { Player.SendMessage(p, "Aucunes commandes dans cette categorie est disponible pour vous."); break; }
                        Player.SendMessage(p, "Les commandes d'information que vous pouvez utiliser:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "jeux":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.type.Contains("jeu")) message += ", " + getColor(comm.name) + comm.name;
                            }
                        }

                        if (message == "") { Player.SendMessage(p, "Aucunes commandes dans cette categorie est disponible pour vous."); break; }
                        Player.SendMessage(p, "Les commandes de jeu que vous pouvez utiliser:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "other":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.type.Contains("other")) message += ", " + getColor(comm.name) + comm.name;
                            }
                        }

                        if (message == "") { Player.SendMessage(p, "Aucunes commandes dans cette categorie est disponible pour vous."); break; }
                        Player.SendMessage(p, "Les autres commandes que vous pouvez utiliser:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "short":
                        message = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                if (comm.shortcut != "") message += ", &b" + comm.shortcut + " " + Server.DefaultColor + "[" + comm.name + "]";
                            }
                        }
                        Player.SendMessage(p, "Racourcits disponibles:");
                        Player.SendMessage(p, message.Remove(0, 2) + ".");
                        break;
                    case "old":
                        string commandsFound = "";
                        foreach (Command comm in Command.all.commands)
                        {
                            if (p == null || p.group.commands.All().Contains(comm))
                            {
                                try { commandsFound += ", " + comm.name; } catch { }
                            }
                        }
                        Player.SendMessage(p, "commandes disponibles:");
                        Player.SendMessage(p, commandsFound.Remove(0, 2));
                        Player.SendMessage(p, "\"/help <command>\" pour plus d'informations.");
                        Player.SendMessage(p, "\"/help shortcuts\" pour avoir la liste des racourcits.");
                        break;
                    default:
                        Command cmd = Command.all.Find(message);
                        if (cmd != null)
                        {
                            cmd.Help(p, param);
                            string foundRank = Level.PermissionToName(GrpCommands.allowedCommands.Find(grpComm => grpComm.commandName == cmd.name).lowestRank);
                            Player.SendMessage(p, "Rang minimum: " + getColor(cmd.name) + foundRank);
                            return;
                        }
                        byte b = Block.Byte(message);
                        if (b != Block.Zero)
                        {
                            Player.SendMessage(p, "Bloc \"" + message + "\" apparais comme &b" + Block.Name(Block.Convert(b)));
                            string foundRank = Level.PermissionToName(Block.BlockList.Find(bs => bs.type == b).lowestRank);
                            Player.SendMessage(p, "Rang minimum: " + foundRank);
                            return;
                        }
                        Player.SendMessage(p, "Impossible de trouver la commande ou le bloc demande");
                        break;
                }
            }
            catch (Exception e) { Server.ErrorLog(e); Player.SendMessage(p, "Erreur"); }
        }

        private string getColor(string commName)
        {
            foreach (GrpCommands.rankAllowance aV in GrpCommands.allowedCommands)
            {
                if (aV.commandName == commName)
                {
                    if (Group.findPerm(aV.lowestRank) != null)
                        return Group.findPerm(aV.lowestRank).color;
                }
            }

            return "&f";
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "Vraiment ? Vous etes tres fort !.");
        }
    }
}