using System;

namespace MCWorlds
{
    public class CmdWhisper : Command
    {
        public override string name { get { return "whisper"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdWhisper() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "")
            {
                p.whisper = !p.whisper; p.whisperTo = "";
                if (p.whisper)
                {
                    Player.SendMessage(p, "Tous les messages envoyers seront automatiquement des messages perso");
                    Player.SendMessage(p, "Ecrivez - <joueur> <message>");
                }
                else Player.SendMessage(p, "Envois auto desactive");
            }
            else
            {
                Player who = Player.Find(message);
                if (who == null) { p.whisperTo = ""; p.whisper = false; Player.SendMessage(p, "Joueur introuvable."); return; }

                p.whisper = true;
                p.whisperTo = who.name;
                Player.SendMessage(p, "Tous les messages seront envoye a " + who.name + ".");
            }


        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/whisper [pseudo] - Envois tous les messages ecris au joueur cible");
        }
    }
}