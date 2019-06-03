using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdSphereCentre : Command
    {
        public override string name { get { return "spherecentre"; } }
        public override string shortcut { get { return "ec"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdSphereCentre() { }

        public override void Use(Player p, string message)
        {
            if (p == null)
            { Player.SendMessage(p, "Vous ne pouvez pas creer une sphere depuis la console ou l'irc"); return; }

            CatchPos cpos; cpos.rand = false; cpos.rayon = 0; cpos.type = Block.Zero; cpos.vide = false;

            if (message == "")
            { Help(p); return; }
            else if (message.IndexOf(' ') == -1) // ec <rayon>
            {
                try { cpos.rayon = UInt16.Parse((message.Split(' ')[0])); }
                catch { Player.SendMessage(p, "Rayon trop petit ou valeur incorrecte"); return; }
            }
            else if (message.Split(' ').Length == 2) // ec <rayon> <type> ou ec <rayon> <creuse> ou ec <rayon> <rand>
            {
                try { cpos.rayon = UInt16.Parse((message.Split(' ')[0])); }
                catch { Player.SendMessage(p, "Rayon trop petit ou valeur incorrecte"); return; }

                string param = message.Split(' ')[1];
                if (param == "creuse")
                { cpos.vide = true;}
                else if (param == "random" || param == "rand")
                { cpos.rand = true; }
                else
                {
                    cpos.type = Block.Byte(param);
                    if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + param + "\"."); return; }
                    if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                }
            }
            else if (message.Split(' ').Length == 3) //ec <rayon> <type> <creuse> ou ec <rayon> <type> <rand> ou ec <rayon> <creuse> <rand>
            {
                try { cpos.rayon = UInt16.Parse((message.Split(' ')[0])); }
                catch { Player.SendMessage(p, "Rayon trop petit ou valeur incorrecte"); return; }

                string param1 = message.Split(' ')[1];
                string param2 = message.Split(' ')[2];

                if (param1 == "creuse")
                { cpos.vide = true; }
                else if (param1 == "random" || param1 == "rand")
                { cpos.rand = true; }
                else
                {
                    cpos.type = Block.Byte(param1);
                    if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + param1 + "\"."); return; }
                    if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                }

                if (param2 == "creuse")
                { cpos.vide = true; }
                else if (param2 == "random" || param2 == "rand")
                { cpos.rand = true; }
                else
                { Help(p); return; }
            }
            else if (message.Split(' ').Length == 4) //ec <rayon> <type> <creuse> <rand>
            {
                try { cpos.rayon = UInt16.Parse((message.Split(' ')[0])); }
                catch { Player.SendMessage(p, "Rayon trop petit ou valeur incorrecte"); return; }

                string param0 = message.Split(' ')[1];
                string param1 = message.Split(' ')[2];
                string param2 = message.Split(' ')[3];

                cpos.type = Block.Byte(param0);
                if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + param0 + "\"."); return; }
                if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }

                if (param1 == "creuse")
                { cpos.vide = true; }
                else if (param1 == "random" || param1 == "rand")
                { cpos.rand = true; }
                else
                { Help(p); return; }

                if (param2 == "creuse")
                { cpos.vide = true; }
                else if (param2 == "random" || param2 == "rand")
                { cpos.rand = true; }
                else
                { Help(p); return; }
            }
            else
            { Help(p); return; }

            if (cpos.rayon < 1)
            {
                Player.SendMessage(p, "Le rayon doit etre un nombre positif non nul");
                return;
            }
            
            p.blockchangeObject = cpos;
            
            Player.SendMessage(p, "Place un bloc pour determiner le centre de la sphere.");

            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/spherecentre [rayon] <type> <creuse> <rand>- Permet de creer une sphere en placant le centre et en donnant le rayon");
            Player.SendMessage(p, "Si 'creuse' est marque, la sphere sera vide");
            Player.SendMessage(p, "Si 'rand' est marque, les blocs seront place aleatoirement");
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);

            CatchPos cpos = (CatchPos)p.blockchangeObject;
            if (cpos.type != Block.Zero) { type = cpos.type; }

            double rayon2 = cpos.rayon + 0.5;

            int xmin = x - cpos.rayon - 1, xmax = x + cpos.rayon + 1;
            int ymin = y - cpos.rayon - 1, ymax = y + cpos.rayon + 1;
            int zmin = z - cpos.rayon - 1, zmax = z + cpos.rayon + 1;

            int totalBlocs = (int)((4 / 3) * Math.PI * rayon2 * rayon2 * rayon2); // volume d'une sphere : 4/3*PI*R^3
            if (cpos.vide && cpos.rayon > 1.5)
            { totalBlocs -= (int)((4 / 3) * Math.PI * (rayon2 - 1) * (rayon2 - 1) * (rayon2 - 1)); }
            if (cpos.rand) { totalBlocs /= 2; }

            if (totalBlocs > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayez de faire une sphere de " + totalBlocs + " blocs.");
                Player.SendMessage(p, "Vous ne pouvez pas faire une sphere de plus de " + p.maxblocsbuild() + " blocs.");
                return;
            }

            Player.SendMessage(p, totalBlocs + " blocs.");

            bool dansSphere = false;
            double rayoncarre = rayon2 * rayon2;
            Random rand = new Random();

            for (int i = xmin; i < xmax; i++)
            {
                for (int j = ymin; j < ymax; j++)
                {
                    for (int k = zmin; k < zmax; k++)
                    {
                        if (i >= 0 && i < p.level.width && j >= 0 && j < p.level.depth && k >= 0 && k < p.level.height) //si le point est dans la map
                        {
                            dansSphere = false;

                            int distanceCentre = (i - x) * (i - x) + ( j - y ) * ( j - y ) + ( k - z ) * ( k - z ); 
                            
                            if (distanceCentre <= rayoncarre)
                            { dansSphere = true; }
                            if (cpos.vide && distanceCentre < (rayon2 - 1) * (rayon2 - 1))
                            { dansSphere = false; }
                            if (dansSphere) //si le point est dans la sphere (vide ou pleine)
                            {
                                if (!cpos.rand || rand.Next(0, 2) == 0) { p.level.Blockchange(p, (ushort)(i), (ushort)(j), (ushort)(k), type); }
                            }
                        }
                    }
                }
            }

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }



        struct CatchPos
        {
            public byte type;
            public ushort rayon;
            public bool vide;
            public bool rand;
        }
    }
}