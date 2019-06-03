using System;

namespace MCWorlds
{
    public class CmdTrust : Command
    {
        public override string name { get { return "trust"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "moderation"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdTrust() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') != -1) { Help(p); return; }

            Player who = Player.Find(message);
            if (who == null)
            {
                Player.SendMessage(p, "Impossible de trouver le joueur entre");
                return;
            }
            else
            {
                who.ignoreGrief = !who.ignoreGrief;
                Player.SendMessage(p, "L'antigrief de " + who.color + who.name + Server.DefaultColor + " a change en : " + who.ignoreGrief);
                who.SendMessage("Votre antigrief a change en : " + who.ignoreGrief);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/trust <nom> - Desactive l'antigrief pour un joueur.");
        }
    }
}