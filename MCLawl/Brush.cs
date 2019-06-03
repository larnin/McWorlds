using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class Brush
    {
        public byte blocBrush = Block.Zero;
        public byte blocSet = Block.Zero;
        public List<byte> blocks = new List<byte>();
        public bool not = false;
        public bool mask = false;
        public string type = "";
        public List<int> size = new List<int>();

        public Brush() 
        {
            blocBrush = Block.Zero;
            blocSet = Block.Zero;
            blocks = new List<byte>();
            not = false;
            mask = false;
            type = "";
            size = new List<int>();
        }

        public bool setMask(string bs, bool notbs = false, Player p = null)
        {
            if (bs == "") { if (p != null) { Player.SendMessage(p, "Aucun blocs dans la liste"); } return false; }

            List<byte> blocksBuff = new List<byte>();
            byte b = Block.Zero;

            foreach (string btxt in bs.Split(' '))
            {
                b = Block.Byte(btxt);
                if (b != Block.Zero) { blocksBuff.Add(b); }
            }

            if (blocksBuff.Count == 0) { if (p != null) { Player.SendMessage(p, "Aucun blocs valide dans la liste"); } return false; }
            blocks.Clear(); blocks = blocksBuff;
            not = notbs;
            mask = true;

            return true;
        }

        public void clearMask()
        {
            blocks.Clear();
            not = false;
            mask = false;
        }

        public bool setBloc(string btxt)
        {
            byte b = Block.Byte(btxt);
            if (Block.AnyBuild(b)) { blocBrush = b; }
            else { return false; }
            return true;
        }

        public void clearBloc()
        { blocBrush = Block.Zero; }

        public bool setBrush(string typetxt, string sizetxt, byte blocnex, Player p)
        {
            List<int> sizeBuff = new List<int>();
            int value;

            foreach (string s in sizetxt.Split(' '))
            {
                try 
                { 
                    value = int.Parse(s);
                    if (value < 1 || value > 8) 
                    {
                        Player.SendMessage(p, "La taille doit etre comprise entre 1 et 8");
                        return false; 
                    } 
                    sizeBuff.Add(value);
                }
                catch { return false; }
            }

            switch (typetxt)
            {
                case "sphere":
                case "hsphere":
                case "cube":
                case "hcube":
                case "pyramide":
                case "hpyramide":
                    if (sizeBuff.Count != 1) { goto sizeFalse; }
                    break;
                case "cylindre":
                case "hcylindre":
                case "lisse":
                case "surligne":
                    if (sizeBuff.Count != 2) { goto sizeFalse; }
                    break;
                default:
                    Player.SendMessage(p, "Impossible de creer le brush de type " + typetxt);
                    return false;
                sizeFalse:
                    Player.SendMessage(p, "Tailles sur le brush errone");
                    return false;
            }

            type = typetxt;
            size = sizeBuff;
            blocSet = blocnex;

            return true;
        }

        public void clearBrush()
        {
            type = "";
            size.Clear();
            blocSet = Block.Zero;
        }

        public void exec(Level l, Player p, ushort x, ushort y, ushort z)
        {
            switch (type)
            {
                case "sphere":
                    execSphere(l, p, x, y, z);
                    break;
                case "hsphere":
                    execHSphere(l, p, x, y, z);
                    break;
                case "cube":
                    execCube(l, p, x, y, z);
                    break;
                case "hcube":
                    execHCube(l, p, x, y, z);
                    break;
                case "pyramide":
                    execPyramide(l, p, x, y, z);
                    break;
                case "hpyramide":
                    execHPyramide(l, p, x, y, z);
                    break;
                case "cylindre":
                    execCylindre(l, p, x, y, z);
                    break;
                case "hcylindre":
                    execHCylindre(l, p, x, y, z);
                    break;
                case "lisse":
                    execLisse(l, p, x, y, z);
                    break;
                case "surligne":
                    execSurligne(l, p, x, y, z);
                    break;
                default:
                    l.Blockchange(p, x, y, z, blocBrush);
                    Player.SendMessage(p, "ERREUR : brush inconnu");
                    return;
            }
            return;
        }

        private void execSphere(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }
            
            double rayoncarre = (size[0] + 0.5) * (size[0] + 0.5);
            
            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int yy = -size[0]; yy <= size[0]; yy++)
                {
                    for (int zz = -size[0]; zz <= size[0]; zz++)
                    {
                        int distanceCentre = xx  * xx + yy * yy + zz * zz ;
                        if (distanceCentre <= rayoncarre)
                        { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } }
                    }
                }
            }

            Player.SendMessage(p, "Brush sphere " + count + " blocs");
        }

        private void execHSphere(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }
            
            double rayoncarre = (size[0] + 0.5) * (size[0] + 0.5);
            double rayonInt = (size[0] - 0.5) * (size[0] - 0.5);

            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int yy = -size[0]; yy <= size[0]; yy++)
                {
                    for (int zz = -size[0]; zz <= size[0]; zz++)
                    {
                        int distanceCentre = xx * xx + yy * yy + zz * zz;
                        if (distanceCentre <= rayoncarre && distanceCentre >= rayonInt)
                        { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } }
                    }
                }
            }

            Player.SendMessage(p, "Brush Hsphere " + count + " blocs");
        }

        private void execCube(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }

            for (int xx = -size[0]; xx < size[0]; xx++)
            {
                for (int yy = -size[0]; yy < size[0]; yy++)
                {
                    for (int zz = -size[0]; zz < size[0]; zz++)
                    { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } }
                }
            }

            Player.SendMessage(p, "Brush cube " + count + " blocs");
        }

        private void execHCube(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }

            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int yy = -size[0]; yy <= size[0]; yy++)
                {
                    for (int zz = -size[0]; zz <= size[0]; zz++)
                    {
                        if (xx != - size[0] && xx != size[0] && yy != - size[0] && yy != size[0] && zz != - size[0] && zz != size[0]) { continue; }
                        if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } 
                    }
                }
            }

            Player.SendMessage(p, "Brush Hcube " + count + " blocs");
        }
        
        private void execPyramide(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }

            for (int i = 0; i <= size[0]; i++)
            {
                for (int j = - size[0] + i; j <= size[0] - i; j++)
                {
                    for (int k = - size[0] + i; k <= size[0] - i; k++)
                    {
                        if (blocChange(l, p, (ushort)(x + j), (ushort)(y + i), (ushort)(z + k), blocSet)) { count++; } 
                    }
                }
            }
            
            Player.SendMessage(p, "Brush pyramide " + count + " blocs");
        }

        private void execHPyramide(Level l, Player p, ushort x, ushort y, ushort z)
        {
            int count = 0;

            if (size.Count == 0) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }

            for (int i = 0; i <= size[0]; i++)
            {
                for (int j = - size[0] + i; j <= size[0] - i; j++)
                {
                    for (int k = - size[0] + i; k <= size[0] - i; k++)
                    {
                        if (j == - size[0] + i || j == size[0] - i || k == - size[0] + i || k == size[0] - i)
                        { if (blocChange(l, p, (ushort)(x + j), (ushort)(y + i), (ushort)(z + k), blocSet)) { count++; } }
                    }
                }
            }

            Player.SendMessage(p, "Brush Hpyramide " + count + " blocs");
        }

        private void execCylindre(Level l, Player p, ushort x, ushort y, ushort z)
        {   // size[0] == rayon | size[1] == hauteur 
            int count = 0;

            if (size.Count < 2) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }
            
            double rayoncarre = (size[0] + 0.5) * (size[0] + 0.5);
            
            
            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int zz = -size[0]; zz <= size[0]; zz++)
                {
                    int distanceCentre = xx * xx + zz * zz;
                    if (distanceCentre <= rayoncarre)
                    {
                        for (int yy = -size[1] / 2; yy <= size[1] / 2; yy++)
                        { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } }
                    }
                }
            }

            Player.SendMessage(p, "Brush cylindre " + count + " blocs");
        }

        private void execHCylindre(Level l, Player p, ushort x, ushort y, ushort z)
        {   // size[0] == rayon | size[1] == hauteur 
            int count = 0;

            if (size.Count < 2) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }
            
            double rayoncarre = (size[0] + 0.5) * (size[0] + 0.5);
            double rayonInt = (size[0] - 0.5) * (size[0] - 0.5);

            
            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int zz = -size[0]; zz <= size[0]; zz++)
                {
                    int distanceCentre = xx * xx + zz * zz;
                    if (distanceCentre <= rayoncarre && distanceCentre >= rayonInt)
                    {
                        for (int yy = -size[1] / 2; yy <= size[1] / 2; yy++)
                        { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), blocSet)) { count++; } }
                    }
                }
            }

            Player.SendMessage(p, "Brush Hcylindre " + count + " blocs");
        }

        private void execLisse(Level l, Player p, ushort x, ushort y, ushort z)
        {   // size[0] == largeur | size[1] == hauteur 
            int count = 0;

            if (size.Count < 2) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }
            int sizeL = 2 * size[0] + 1, sizeH = 2 * size[1] + 1;
            List<byte> blocsLisse = new List<byte>(sizeL * sizeL * sizeH);

            List<blocCount> blocsid = new List<blocCount>();
            blocCount bMax = new blocCount();
            int blocsIn = 0, nbValide = 0;
            byte bl = Block.Zero;
            //bool blocFind = false;

            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int yy = -size[1]; yy <= size[1]; yy++)
                {
                    for (int zz = -size[0]; zz <= size[0]; zz++)
                    {
                        blocsIn = 0; nbValide = 0; blocsid.Clear(); //blocFind = false;                        

                        for (int i = -2; i <= 2; i++)
                        {
                            for (int j = -2; j <= 2; j++)
                            {
                                for (int k = -2; k <= 2; k++)
                                {
                                    bl = p.level.GetTile((ushort)(x + xx + i), (ushort)(y + yy + j), (ushort)(z + zz + k));

                                    if (bl == Block.Zero) { continue; }
                                    if (bl == Block.air) { blocsIn++; continue; }
                                    
                                    blocsIn++; nbValide++;

                                    bMax = blocsid.Find(bi => bi.id == bl);
                                    if (bMax == null)
                                    { blocCount bc = new blocCount(bl,1); blocsid.Add(bc); }
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
                        bl = p.level.GetTile((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                        
                        if (bl == Block.Zero) { blocsLisse.Add(Block.Zero); }
                        
                        if (nbValide < blocsIn / 2) { blocsLisse.Add(Block.air); }
                        else 
                        {
                            if (bl != Block.air) { blocsLisse.Add(bl); }
                            else
                            {
                                bMax.id = 1; bMax.nbBlocs = 0;

                                foreach (blocCount bi in blocsid)
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

            for (int yy = 0; yy < sizeH; yy++)
            {
                for (int xx = 0; xx < sizeL; xx++)
                {
                    for (int zz = 0; zz < sizeL; zz++)
                    {
                        if (blocsLisse[xx * sizeH * sizeL + yy * sizeL + zz] != Block.Zero)
                        { if (blocChange(l, p, (ushort)(x + xx - size[0]), (ushort)(y + yy - size[1]), (ushort)(z + zz - size[0]), blocsLisse[xx * sizeH * sizeL + yy * sizeL + zz])) { count++; }; }
                    }
                }
            }
            Player.SendMessage(p, "Brush lisse " + count + " blocks");
        }

        private void execSurligne(Level l, Player p, ushort x, ushort y, ushort z)
        { // size[0] == largeur | size[1] == epaisseur du surlignage
            int count = 0;

            if (size.Count < 2) { Player.SendMessage(p, "Erreur de taille dans le brush"); return; }

            for (int xx = -size[0]; xx <= size[0]; xx++)
            {
                for (int yy = -size[0]; yy <= size[0]; yy++)
                {
                    for (int zz = -size[0]; zz <= size[0]; zz++)
                    {
                        double rayoncarre = (size[0] + 0.5) * (size[0] + 0.5);
                        int distanceCentre = xx * xx + yy * yy + zz * zz;
                        if (distanceCentre <= rayoncarre)
                        {
                            if (p.level.GetTile((ushort)(x + xx), (ushort)(y + yy + 1), (ushort)(z + zz)) == Block.air)
                            {
                                for (int i = 0; i < size[1]; i++)
                                {
                                    if (p.level.GetTile((ushort)(x + xx), (ushort)(y + yy - i), (ushort)(z + zz)) == Block.air) { break; }
                                    else { if (blocChange(l, p, (ushort)(x + xx), (ushort)(y + yy - i), (ushort)(z + zz), blocSet)) { count++; } }
                                }
                            }
                        }
                    }
                }
            }

            Player.SendMessage(p, "Brush surligne " + count + " blocs");
        }

        private bool blocChange(Level l, Player p, ushort x, ushort y, ushort z, byte bl)
        {
            if (blocks.Count == 0) { l.Blockchange(p, x, y, z, bl); return true; }
            byte b = l.GetTile(x, y, z);

            if (not)
            {
                bool bInList = false;
                foreach (byte bb in blocks)
                { if (bb == b) { bInList = true; continue; } }
                if (!bInList) { l.Blockchange(p, x, y, z, bl); return true; }
            }
            else
            {
                bool bInList = false;
                foreach (byte bb in blocks)
                { if (bb == b) { bInList = true; continue; } }
                if (bInList) { l.Blockchange(p, x, y, z, bl); return true; }
            }
            return false;
        }

        class blocCount
        {
            public byte id;
            public int nbBlocs;

            public blocCount(byte idi = 0, int blocs = 0) { id = idi; nbBlocs = blocs; }
        }        
    }
}
