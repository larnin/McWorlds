using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    public class CmdZombieattaque2 : Command
    {
        public override string name { get { return "zombieattaque"; } }
        public override string shortcut { get { return "zatt"; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public CmdZombieattaque2() { }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }



        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/zatt [nom] - Active/desactive l'attaque de zombies sur la map");
            Player.SendMessage(p, "/zatt start - Lance le jeu");
            Player.SendMessage(p, "/zatt stop - Arette le jeu");
            Player.SendMessage(p, "/zatt info - Donne des informations sur la partie");
            Player.SendMessage(p, "/zatt list - Liste les configurations disponible");
            Player.SendMessage(p, "/zatt save [file] - Sauvegarde les configurations de partie");
            Player.SendMessage(p, "/zatt load [file] - Charge une configuration");
            Player.SendMessage(p, "/zatt spawn - Ajoute un lieux de spawn des zombie");
            Player.SendMessage(p, "/zatt waves [nombre] - Change le nombre de vagues");
            Player.SendMessage(p, "/zatt monstres [nombre] - Change le nombre de monstres par vagues");
            Player.SendMessage(p, "/zatt types [mob1] [mob2] .... - Determine les monstres a envoyer");
            Player.SendMessage(p, "Mobs valide : creeper zombie blue_bird red_robin killer_phoenix ");
            Player.SendMessage(p, "/zatt temps [seconde] - Change le temps entre chaque vague");
            Player.SendMessage(p, "/zatt augmente - Determine si le nombre de monstres augmente entre chaque vague");

        }

    }
}
