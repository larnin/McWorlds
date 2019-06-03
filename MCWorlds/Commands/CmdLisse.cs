
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdLisse : Command
    {
        public override string name { get { return "lisse"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdLisse() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible de faire ca depuis l'irc ou la console"); return; }

            if (message != "") { Help(p); return; }

            CatchPos cpos;

            cpos.x = 0; cpos.y = 0; cpos.z = 0;
            p.blockchangeObject = cpos;

            Player.SendMessage(p, "Place 2 blocs pour determiner la zone a lisser.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "") // obtenue en tapant /help lisse
        {
            Player.SendMessage(p, "/lisse - Lisse le terrin entre les blocs selectionne.");
            Player.SendMessage(p, "&cATTENTION" + Server.DefaultColor + " : A ne pas utiliser a cote d'un bloc liquide");
        }

        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange2);
        }

        public void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type)
        {       //récupération des infos sur le dernier bloc cliqué 
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;

            int sizeX = Math.Abs(cpos.x - x) + 1, sizeY = Math.Abs(cpos.y - y) + 1, sizeZ = Math.Abs(cpos.z - z) + 1;
            int nbBlocs = sizeX * sizeY * sizeZ;

            if (nbBlocs > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayer de lisser une zone de " + nbBlocs + " blocs.");
                Player.SendMessage(p, "Vous n'avez pas le droit a plus de " + p.maxblocsbuild() + ".");
                return;
            }
            List<byte> blocsLisse = new List<byte>(nbBlocs);
           
            List<bloc> blocsid = new List<bloc>();
            bloc bMax = new bloc();
            //bool blocFind = false;
            int blocsIn = 0, nbValide = 0, tabX = 0, tabY = 0, tabZ = 0;
            byte bl = Block.Zero;


            //rechreche de tous les blocs et lissage
            for (int i = Math.Min(x, cpos.x); i <= Math.Max(x, cpos.x); i++)
            {
                for (int j = Math.Min(y, cpos.y); j <= Math.Max(y, cpos.y); j++)
                {
                    for (int k = Math.Min(z, cpos.z); k <= Math.Max(z, cpos.z); k++)
                    {
                        blocsIn = 0; nbValide = 0; blocsid.Clear(); //blocFind = false;
                        
                        //calcul du lissage
                        for (int l = -2; l <= 2; l++)
                        {
                            for (int m = -2; m <= 2; m++)
                            {
                                for (int n = -2; n <= 2; n++)
                                {
                                    bl = p.level.GetTile((ushort)(i + l), (ushort)(j + m), (ushort)(k + n));

                                    if (bl == Block.Zero) { continue; }
                                    if (bl == Block.air) { blocsIn++; continue; }

                                    blocsIn++; nbValide++;

                                    bMax = blocsid.Find(bi => bi.id == bl);
                                    if (bMax == null)
                                    { bloc bc = new bloc(bl, 1); blocsid.Add(bc); }
                                    else { bMax.nbBlocs++; }

                                    /*for (int a = 0; a < blocsid.Count; a++)
                                    {
                                        if (blocsid[a].id == bl) 
                                        { 
                                            bMax = blocsid[a];
                                            blocsid.Remove(bMax);
                                            bMax.nbBlocs++;
                                            blocsid.Add(bMax);
                                            blocFind = true;
                                        }
                                    }
                                    if (!blocFind)
                                    { bMax.id = bl; bMax.nbBlocs = 1; blocsid.Add(bMax); }*/
                                }
                            }
                        }
                        
                        // lissage (si 50% des blocs alentours sont air, le bloc devient air sinon il devient solide)
                        bl = p.level.GetTile((ushort)i, (ushort)j, (ushort)k);

                        if (bl == Block.Zero) { blocsLisse.Add(Block.Zero); }

                        if (nbValide < blocsIn / 2) { blocsLisse.Add(Block.air); }
                        else 
                        {
                            if (bl != Block.air) { blocsLisse.Add(bl); }
                            else
                            {
                                bMax.id = 1; bMax.nbBlocs = 0;

                                foreach (bloc bi in blocsid)
                                {
                                    if (bi.nbBlocs > bMax.nbBlocs) { bMax.id = bi.id; bMax.nbBlocs = bi.nbBlocs; }
                                }
                                blocsLisse.Add(bMax.id);
                            }
                        }
                    }
                }
            }
            
            blocsid.Clear();
             
            //Poussage de l'herbe et application du lissage
            byte bAct = Block.Zero;
            for (int i = Math.Min(x, cpos.x); i <= Math.Max(x, cpos.x); i++)
            {
                tabX = i - Math.Min(x, cpos.x); 
                for (int j = Math.Min(y, cpos.y); j <= Math.Max(y, cpos.y); j++)
                {
                    tabY = j - Math.Min(y, cpos.y); 
                    for (int k = Math.Min(z, cpos.z); k <= Math.Max(z, cpos.z); k++)
                    {
                        tabZ = k - Math.Min(z, cpos.z);
                        bAct = blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ];
                        
                        if (j + 1 > Math.Max(y, cpos.y))
                        {
                            if (bAct == Block.dirt && p.level.GetTile((ushort)i, (ushort)(j + 1), (ushort)k) == Block.air)
                            { blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ] = Block.grass; }
                            if (bAct == Block.grass && p.level.GetTile((ushort)i, (ushort)(j + 1), (ushort)k) != Block.air)
                            { blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ] = Block.dirt; }
                        }
                        else
                        {
                            if (bAct == Block.dirt && blocsLisse[tabX * sizeY * sizeZ + (tabY + 1) * sizeZ + tabZ] == Block.air)
                            { blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ] = Block.grass; }
                            if (bAct == Block.grass && blocsLisse[tabX * sizeY * sizeZ + (tabY + 1) * sizeZ + tabZ] != Block.air)
                            { blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ] = Block.dirt; }
                        }
                        
                            //Application
                        if (bAct != Block.Zero) { p.level.Blockchange(p, (ushort)i, (ushort)j, (ushort)k, bAct); }
                    }
                }
            }

            /*//aplication du lissage
            for (int i = Math.Min(x, cpos.x); i <= Math.Max(x, cpos.x); i++)
            {
                tabX = i - Math.Min(x, cpos.x); 
                for (int j = Math.Min(y, cpos.y); j <= Math.Max(y, cpos.y); j++)
                {
                    tabY = j - Math.Min(y, cpos.y);
                    for (int k = Math.Min(z, cpos.z); k <= Math.Max(z, cpos.z); k++)
                    {
                        tabZ = k - Math.Min(z, cpos.z);

                        if (blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ] != Block.Zero)
                        { p.level.Blockchange(p, (ushort)i, (ushort)j, (ushort)k, blocsLisse[tabX * sizeY * sizeZ + tabY * sizeZ + tabZ]); }
                    }
                }
            }*/

            Player.SendMessage(p, "Lissage termine");

            blocsLisse.Clear();

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1); //si répétition commande activé
        }

        struct CatchPos
        {
            public ushort x, y, z;
        }

        class bloc
        {
            public byte id;
            public int nbBlocs;

            public bloc(byte idi = 0, int blocs = 0) { id = idi; nbBlocs = blocs; }
        }        
    }
}