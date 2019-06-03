
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MCWorlds
{
    public class CmdZombieattaque : Command
    {
        public override string name { get { return "zombieattaque"; } }
        public override string shortcut { get { return "zatt"; } }
        public override string type { get { return "jeu"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdZombieattaque() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;
            cpos.map = p.level;
            cpos.nbMonstresAjouter = 0;
            

            if (message == "off" && p.zombiespawn)
            {
                p.zombiespawn = false;
                Command.all.Find("replaceall").Use(p, "zombie air");
                Command.all.Find("replaceall").Use(p, "creeper air");
                Player.GlobalMessage("L'attaque zombie est aretter, tous les monstres sont supprimer");
                return;
            }
            if (message == "off" && !p.zombiespawn)
            { Player.SendMessage(p, "Aucune attaque de zombie lancee"); return; }

            if (message.Split(' ').Length != 4)
            { Help(p); return; }

            try
            {
                cpos.parametre = int.Parse((message.Split(' ')[0]));
                cpos.nbVaguesTotal = int.Parse((message.Split(' ')[1]));
                cpos.temps = int.Parse((message.Split(' ')[2]));
                cpos.nbMonstres = int.Parse((message.Split(' ')[3]));
            }
            catch { Player.SendMessage(p, "Parametres invalides"); return; }

            if (cpos.parametre > 3 | cpos.parametre < 1)
            {
                Player.SendMessage(p, "Le parametre doit etre entre 1 et 3");
                Player.SendMessage(p, "1 = que des zombies, 2 = zombies+creeper, 3 = que des creepers");
                return;
            }

            if (cpos.nbVaguesTotal > 100 || cpos.nbVaguesTotal < 2)
            { Player.SendMessage(p, "Le nombre de vagues doit etre entre 2 et 100"); return;  }

            if (cpos.temps > 300 || cpos.temps < 10)
            { Player.SendMessage(p, "Le temps entre chaques vagues doit etre entre 10 et 300 secondes"); return; }

            if (cpos.nbMonstres == -1)
            {
                cpos.nbMonstres = 1;
                cpos.nbMonstresAjouter = 23 / (cpos.nbVaguesTotal - 1);
            }

            if (cpos.nbMonstres > 24 || cpos.nbMonstres < 1)
            { Player.SendMessage(p, "Le nombre de monstres doit etre entre 1 et 24"); return; }

            p.blockchangeObject = cpos;

            Player.SendMessage(p, "Placez un bloc au centre d'une surface plane, qui sera la zone de spawn des monstres");

            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);     
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);

            CatchPos cpos = (CatchPos)p.blockchangeObject;

            if (cpos.parametre == 1)
            { Player.GlobalMessageLevel(cpos.map, "Attention des zombies envahissent la map !"); }
            if (cpos.parametre == 2)
            { Player.GlobalMessageLevel(cpos.map, "Attention des zombies et des creepers envahissent la map !"); }
            if (cpos.parametre == 3)
            { Player.GlobalMessageLevel(cpos.map, "Attention des creepers envahissent la map !"); }
            Player.GlobalMessageLevel(cpos.map, "&a" + cpos.nbVaguesTotal + Server.DefaultColor + " vagues et &a" + cpos.temps + Server.DefaultColor + " secondes entre chaques vagues.");
            Player.GlobalMessageLevel(cpos.map, "L'attaque débute dans 10 secondes");
            
            if ( x < 2 ) { x = 2; }
            if (x > cpos.map.width - 2) { x = (ushort)(cpos.map.width - 2); }

            if (y < 2) { y = 2; }
            if (y > cpos.map.depth - 2) { y = (ushort)(cpos.map.width - 2); }
            
            if (z < 2) { z = 2; }
            if (z > cpos.map.height - 2) { z = (ushort)(cpos.map.width - 2); }

            List<positions> positionsSpawnMonstres = new List<positions>() ;
            positions t;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    t.x = (ushort)(x + i - 2); t.y = y; ; t.z = (ushort)(z + j - 2);
                    positionsSpawnMonstres.Add(t);
                }
            }

            p.zombiespawn = true;
            Random rand = new Random();
            int vagueActuelle = 1;
            int nbcreeper = 0 ;
            int nbzombies = 0;
            double ajoutMonstres = 0;
            
            int a = 0, c = 0;
            
            Thread Threadattaque = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(10000);

                while (cpos.nbVaguesTotal >= vagueActuelle && p.zombiespawn)
                {
                    for (int i = 0; i < 25; i++) // place aleatoirement les monstres dans un carre en 5*5 autour du point clique
                    {
                        a = rand.Next(0, 24);
                        c = rand.Next(0, 24);

                        t = positionsSpawnMonstres[a];
                        positionsSpawnMonstres[a] = positionsSpawnMonstres[b];
                        positionsSpawnMonstres[b] = t;
                    }

                    if (cpos.parametre == 1) { nbcreeper = 0; }
                    if (cpos.parametre == 2) { nbcreeper = rand.Next(0, cpos.nbMonstres); }
                    if (cpos.parametre == 3) { nbcreeper = cpos.nbMonstres; }
                    nbzombies = cpos.nbMonstres - nbcreeper;

                    if (vagueActuelle == 1)
                    { Player.GlobalMessageLevel(cpos.map, "Premiere vague"); }
                    else if (vagueActuelle == cpos.nbVaguesTotal)
                    { Player.GlobalMessageLevel(cpos.map, "Derniere vague, plus que &a" + cpos.temps + Server.DefaultColor + " secondes a tenir !"); }
                    else
                    { Player.GlobalMessageLevel(cpos.map, "Vague &a" + vagueActuelle + Server.DefaultColor + " sur &a" + cpos.nbVaguesTotal + Server.DefaultColor + "."); }

                    if (nbcreeper == 0)
                    { Player.GlobalMessageLevel(cpos.map, "&a" + cpos.nbMonstres + Server.DefaultColor + " zombies."); }
                    else if (nbcreeper == cpos.nbMonstres)
                    { Player.GlobalMessageLevel(cpos.map, "&a" + cpos.nbMonstres + Server.DefaultColor + " creepers."); }
                    else
                    { Player.GlobalMessageLevel(cpos.map, "&a" + nbzombies + Server.DefaultColor + " zombies et &a" + nbcreeper + Server.DefaultColor + " creepers ."); }

                    for (int i = 0; i < cpos.nbMonstres; i++)
                    {
                        t = positionsSpawnMonstres[i];

                        if (i < nbcreeper)
                        { cpos.map.Blockchange(p, t.x, t.y, t.z, Block.creeper); }
                        else
                        { cpos.map.Blockchange(p, t.x, t.y, t.z, Block.zombiebody); }
                    }

                    Thread.Sleep(cpos.temps * 1000);

                    vagueActuelle++;

                    ajoutMonstres += cpos.nbMonstresAjouter;

                    while (ajoutMonstres > 1)
                    {
                        ajoutMonstres--;
                        cpos.nbMonstres++;
                        
                    }
                }

                if (p.zombiespawn)
                {
                    Player.GlobalMessageLevel(cpos.map, "L'attaque prend fin, tous les monstres restant meurent");
                    p.zombiespawn = false;
                    Command.all.Find("replaceall").Use(p, "zombie air");
                    Command.all.Find("replaceall").Use(p, "creeper air");
                }
            
                positionsSpawnMonstres.Clear();
            }));
            Threadattaque.Start();

        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/zombieattaque [parametre] [nbvagues] [temps] [nbzombie] - Permet de creer des attaques de monstres");
            Player.SendMessage(p, "Parametre : 1 = que des zombies, 2 = zombies+creeper, 3 = que des creepers");
            Player.SendMessage(p, "nbvagues : Le nombre de vagues de zombies (entre 2 et 100) ");
            Player.SendMessage(p, "temps : Le temps entre chaques vagues de zombies en secondes (entre 10 et 300)");
            Player.SendMessage(p, "nbzombie : Determine le nombre de zombies/creeper par vagues (entre 1 et 24)");
            Player.SendMessage(p, "Mettre -1 pour avoir un nombre de zombies/creeper croissant par vagues");
            Player.SendMessage(p, "/zombieattaque off - pour arreter les attaques");
            Player.SendMessage(p, "ceci supprime tous les zombies/creepers de la map");
        }

        private struct CatchPos
        {
            public int parametre; //  1 : zombies , 2 : zombies + creeper , 3 : creeper
            public int nbVaguesTotal; // nombre de vague
            public int temps; // delai entre chaques vagues
            public int nbMonstres; //nombre de monstres dans la vague
            public double nbMonstresAjouter; // nombre de monstres a ajouter entre chaques vagues
            public Level map; //map ou se deroule l'attaque
        }
        private struct positions
        {
            public ushort x, y, z;
        }
    }
}
