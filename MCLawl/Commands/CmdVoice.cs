using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdVoice : Command
    {
        public override string name { get { return "voice"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdVoice() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.Split(' ').Length > 2) { Help(p); return; }
            Player who = Player.Find(message.Split(' ')[0]);
            string voix = "";
            if (message.Split(' ').Length > 1)
            { voix = message.Split(' ')[1]; }
            
            if (who != null)
            {
                if (who.voice)
                {
                    who.voice = false;
                    Player.SendMessage(p, "Vous avez enleve le statue de voix a " + who.name);
                    who.SendMessage("Vous avez perdu le statue de voix.");
                    who.voicestring = "";
                }
                else
                {
                    who.voice = true;
                    Player.SendMessage(p, "Vous avez donne le statue de voix a " + who.name);
                    who.SendMessage("Vous avez gagne le statue de voix.");
                    if (voix == "" || voix.Length > 15)
                    {
                        if (voix != "")
                        { Player.SendMessage(p, "Le caractere de voix doit etre inferieur a 15 caracteres"); }
                        who.voicestring = "&f+";
                    }
                    else
                        who.voicestring = "&f" + voix + " ";
                }
            }
            else
            {
                Player.SendMessage(p, "Il n'y a personne de connecte appelle \"" + message + "\"");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/voice [nom] <caracteredevoix> - Donne le statu de voix au joueur.");
        }
    }
}