/*Block.lightgreen  -> stick piston
 *Block.yellow      -> piston normal
 *Block.red         -> redstone
 *Block.redmushroom -> torche
 */

using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class Redstone
    {
        public static bool poseRed(Player p, ushort x, ushort y, ushort z, byte type, byte b)
        {
            if (!p.canBuild) { p.SendBlockchange(x, y, z, b); return false; }

            switch (type)
            {
                case Block.red:
                    redstone(p, x, y, z);
                    break;
                case Block.redmushroom:
                    torche(p, x, y, z);
                    break;
                default:
                    return true;
            }
            return false;
        }

        public static bool posePiston(Player p, ushort x, ushort y, ushort z, byte type, byte b)
        {
            if (!p.canBuild) { p.SendBlockchange(x, y, z, b); return false; }

            switch (type)
            {
                case Block.lightgreen:
                    piston(p, x, y, z, true, b);
                    break;
                case Block.yellow:
                    piston(p, x, y, z, false, b);
                    break;
                default:
                    return true;
            }
            return false;
        }

        public static bool delRed(Player p, ushort x, ushort y, ushort z, byte b)
        {
            switch (b)
            {
                case Block.redstone_off:
                    delRedstone(p, x, y, z, false);
                    break;
                case Block.redstone_on:
                    delRedstone(p, x, y, z, true);
                    break;
                case Block.red_torche_off:
                    delTorche(p, x, y, z, false);
                    break;
                case Block.red_torche_on:
                    delTorche(p, x, y, z, true);
                    break;
                default:
                    if (Block.AnyBuild(b) && !Block.isRedstone(b) && !Block.Walkthrough(b) && !Block.LightPass(b))
                    { delOther(p, x, y, z, b); return false; }
                    return true;
            }
            return false;
        }

        public static bool delPiston(Player p, ushort x, ushort y, ushort z, byte b)
        {
            switch (b)
            {
                case Block.piston_body:
                case Block.piston_middle:
                case Block.piston_push:
                case Block.piston_stick:

                    break;
                default:
                    return true;
            }
            return false;
        }

        private static void redstone(Player p, ushort x, ushort y, ushort z)
        {
            if (p.level.physics == 0)
            { p.level.Blockchange(p, x, y, z, Block.redstone_off); return; }

            bool redOn = false;
            byte bC = Block.Zero;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if (redOn) { continue; }
                        if (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) != 1) { continue; }
                        bC = p.level.GetTile((ushort)(x + i), (ushort)(y + j), (ushort)(z + k));
                        if (bC == Block.redstone_on || bC == Block.red_torche_on) { redOn = true; }
                    }
                }
            }
            if (redOn) { p.level.Blockchange(p, x, y, z, Block.redstone_on); }
            else { p.level.Blockchange(p, x, y, z, Block.redstone_off); }
            p.level.AddCheck(p.level.PosToInt(x, y, z), "", true);
        }

        private static void torche(Player p, ushort x, ushort y, ushort z)
        {
            if (p.level.physics == 0)
            { p.level.Blockchange(p, x, y, z, Block.red_torche_on); return; }

            p.level.Blockchange(p, x, y, z, Block.red_torche_off);
            p.level.AddCheck(p.level.PosToInt(x, y, z), "", true);
        }

        private static void piston(Player p, ushort x, ushort y, ushort z, bool stick, byte b)
        {
                /*bool redProx = false;

                if (p.level.GetTile((ushort)(x - 1), y, z) == Block.redstone_on) { redProx = true; }
                if (p.level.GetTile((ushort)(x + 1), y, z) == Block.redstone_on) { redProx = true; }
                if (p.level.GetTile(x, (ushort)(y - 1), z) == Block.redstone_on) { redProx = true; }
                if (p.level.GetTile(x, (ushort)(y + 1), z) == Block.redstone_on) { redProx = true; }
                if (p.level.GetTile(x, y, (ushort)(z - 1)) == Block.redstone_on) { redProx = true; }
                if (p.level.GetTile(x, y, (ushort)(z + 1)) == Block.redstone_on) { redProx = true; }*/

            int dx = 0, dy = 0, dz = 0;

            if (p.rot[0] >= 224 || p.rot[0] < 32) { dz = 1; }
            if (p.rot[0] >= 32 && p.rot[0] < 96) { dx = -1; }
            if (p.rot[0] >= 96 && p.rot[0] < 160) { dz = -1; }
            if (p.rot[0] >= 160 && p.rot[0] < 224) { dx = 1; }
            if (p.rot[1] > 32 && p.rot[1] <= 64) { dx = 0; dy = 1; dz = 0; }
            if (p.rot[1] >= 192 && p.rot[1] < 224) { dx = 0; dy = -1; dz = 0; }

            if (dx + dy + dz == 0) { Player.SendMessage(p, "Erreur d'orientation (" + p.rot[0] + " " + p.rot[1] + ")"); return; }

            byte b1 = p.level.GetTile((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz));
                //byte b2 = p.level.GetTile((ushort)(x + 2 * dx), (ushort)(y + 2 * dy), (ushort)(z + 2 * dz));

                /*if (b2 != Block.air && redProx || b1 != Block.air)
                { Player.SendMessage(p, "Place insufisante pour placer le piston"); return; }

                p.level.Blockchange(p, x, y, z, Block.piston_body);
                if (redProx)
                {
                    p.level.Blockchange(p, (ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Block.piston_middle);
                    if (stick) { p.level.Blockchange(p, (ushort)(x + 2 * dx), (ushort)(y + 2 * dy), (ushort)(z + 2 * dz), Block.piston_stick); }
                    else { p.level.Blockchange(p, (ushort)(x + 2 * dx), (ushort)(y + 2 * dy), (ushort)(z + 2 * dz), Block.piston_push); }
                }
                else
                {
                    if (stick) { p.level.Blockchange(p, (ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Block.piston_stick); }
                    else { p.level.Blockchange(p, (ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Block.piston_push); }
                }*/

            if (b1 != Block.air)
            { 
                Player.SendMessage(p, "Place insufisante pour placer le piston");
                p.SendBlockchange(x, y, z, b); 
                return;
            }

            p.level.Blockchange(p, x, y, z, Block.piston_body);
            
            if (stick) { p.level.Blockchange(p, (ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Block.piston_stick); }
            else { p.level.Blockchange(p, (ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Block.piston_push); }

            if (p.level.physics != 0) { p.level.AddCheck(p.level.PosToInt(x, y, z), "", true); }
            Player.SendMessage(p, "Piston place");
        }

        private static void delRedstone(Player p, ushort x, ushort y, ushort z, bool on)
        {
            if (on)
            {

            }
            else
            {p.level.Blockchange(p, x, y, z, Block.air);}
        }

        private static void delTorche(Player p, ushort x, ushort y, ushort z, bool on)
        {

        }

        private static void delOther(Player p, ushort x, ushort y, ushort z, byte type)
        {

        }
    }
}
