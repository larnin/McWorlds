using System;

namespace MCWorlds
{
    public class CmdLimit : Command
    {
        public override string name { get { return "limit"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdLimit() { }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length != 2) { Help(p); return; }
            int newLimit;
            try { newLimit = int.Parse(message.Split(' ')[1]); }
            catch { Player.SendMessage(p, "Limite invalide"); return; }
            if (newLimit < 1) { Player.SendMessage(p, "Impossible de definire inferieure a 1."); return; }

            Group foundGroup = Group.Find(message.Split(' ')[0]);
            if (foundGroup != null)
            {
                foundGroup.maxBlocks = newLimit;
                Player.GlobalChat(null, "La limite de construction des " + foundGroup.color + foundGroup.name + Server.DefaultColor + " a ete mis a &b" + newLimit, false);
                Group.saveGroups(Group.GroupList);
            }
            else
            {
                switch (message.Split(' ')[0].ToLower())
                {
                    case "rp":
                    case "restartphysics":
                        Server.rpLimit = newLimit;
                        Player.GlobalMessage("La limite des /rp customise a change a &b" + newLimit.ToString());
                        break;
                    case "rpnorm":
                    case "rpnormal":
                        Server.rpNormLimit = newLimit;
                        Player.GlobalMessage("La limite du /rp normal  a change a &b" + newLimit.ToString());
                        break;

                    default:
                        Player.SendMessage(p, "Parametre inconnu");
                        break;
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/limit [type] [valeur] - Modifie la limite de blocs .");
            Player.SendMessage(p, "Types - " + Group.concatList(true, true) + ", RP, RPNormal");
        }
    }
}