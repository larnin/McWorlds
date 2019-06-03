using System;

namespace MCWorlds
{
    public class CmdRepeat : Command
    {
        public override string name { get { return "repeat"; } }
        public override string shortcut { get { return "m"; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRepeat() { }

        public override void Use(Player p, string message)
        {
            try
            {
                if (p.lastCMD == "") { Player.SendMessage(p, "Aucunes commandes a ete utilise precedement."); return; }
                if (p.lastCMD.Length > 5)
                    if (p.lastCMD.Substring(0, 6) == "static") { Player.SendMessage(p, "Impossible de repeter le mode static"); return; }

                Player.SendMessage(p, "Utilise &b/" + p.lastCMD);

                if (p.lastCMD.IndexOf(' ') == -1)
                {
                    Command.all.Find(p.lastCMD).Use(p, "");
                }
                else
                {
                    Command.all.Find(p.lastCMD.Substring(0, p.lastCMD.IndexOf(' '))).Use(p, p.lastCMD.Substring(p.lastCMD.IndexOf(' ') + 1));
                }
            }
            catch { Player.SendMessage(p, "Erreur!"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/repeat - Repete la derniere commande utilise.");
        }
    }
}