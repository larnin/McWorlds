using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdRedstone : Command
    {
        public override string name { get { return "redstone"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdRedstone() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (p.poseRedstone)
            { 
                Player.SendMessage(p, "Vous ne posez plus de redstone");
                p.poseRedstone = false; return; 
            }

            p.poseRedstone = true;
            Player.SendMessage(p, "Vous posez maintenant la redstone");
            Player.SendMessage(p, "Equipez vous du bloc rouge et du champignon rouge !");

            p.ClearBlockchange();
            p.blockchangeObject = 157;
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/redstone - active le mode redstone");
            Player.SendMessage(p, "Le fonctionnement de la redstone est le meme que sur la beta");
            Player.SendMessage(p, "Posez un bloc rouge pour un fil (il devient blanc si il n'est pas alimente)");
            Player.SendMessage(p, "Posez un champignon rouge pour une torche");
            Player.SendMessage(p, "&cSysteme encore incomplet et innutilisable");
        }

        public void Blockchange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange);

            byte b = p.level.GetTile(x, y, z);
            bool bBreak = Block.RightClick(Block.Convert(b), true);

            if (type != Block.red && type != Block.redmushroom) 
            {
                if (bBreak) { p.level.Blockchange(p, x, y, z, type); }
                return; 
            }

            if (p.level.physics == 0)
            {
                if (!bBreak) { p.level.Blockchange(p, x, y, z, Block.air); }
                else if (type == Block.red) { p.level.Blockchange(p, x, y, z, Block.redstone_off); }
                else if (type == Block.redmushroom) { p.level.Blockchange(p, x, y, z, Block.red_torche_on); }
                return;
            }
            else
            {
                if (!bBreak) //bloc cassé
                {
                    if (!Block.isRedstone(b) || !Block.AnyBuild(b) || Block.Walkthrough(b) || Block.LightPass(b)) { goto jump; }

                    bool redOn = false;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            for (int k = -1; k <= 1; k++)
                            {
                                if (Math.Abs(i) + Math.Abs(j) + Math.Abs(k) != 1) { continue; }

                                byte bSolide = p.level.GetTile((ushort)(x + i), (ushort)(y + j), (ushort)(z + k));
                                if (bSolide == Block.redstone_on) { p.level.Blockchange((ushort)(x + i), (ushort)(y + j), (ushort)(z + k), Block.redstone_off, true); }

                                if (bSolide == Block.red_torche_off)
                                {
                                    byte bSolide2 = Block.Zero;
                                    if (Block.AnyBuild(bSolide) && !Block.Walkthrough(bSolide) && !Block.LightPass(bSolide) && !Block.isRedstone(bSolide))
                                    {
                                        for (int l = -1; l <= 1; l++)
                                        {
                                            for (int m = -1; m <= 1; m++)
                                            {
                                                for (int n = -1; n <= 1; n++)
                                                {
                                                    if (redOn) { continue; }
                                                    if (Math.Abs(l) + Math.Abs(m) + Math.Abs(n) != 1) { continue; }
                                                    bSolide2 = p.level.GetTile((ushort)(x + i + l), (ushort)(y + j + m), (ushort)(z + k + n));
                                                    if (Block.AnyBuild(bSolide2) && !Block.Walkthrough(bSolide2) && !Block.LightPass(bSolide2) && !Block.isRedstone(bSolide2))
                                                    {
                                                        for (int d = -1; d <= 1; d++)
                                                        {
                                                            for (int e = -1; e <= 1; e++)
                                                            {
                                                                for (int f = -1; f <= 1; f++)
                                                                {
                                                                    if (p.level.GetTile((ushort)(x + i + l + d), (ushort)(y + j + m + e), (ushort)(z + k + n + f)) == Block.redstone_on)
                                                                    { redOn = true; }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (!redOn)
                                        { 
                                            p.level.Blockchange(p, (ushort)(x + i), (ushort)(y + j), (ushort)(z + k), Block.red_torche_on);
                                            p.level.AddCheck(p.level.PosToInt((ushort)(x + i), (ushort)(y + j), (ushort)(z + k)), "", true);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    jump:
                    p.level.Blockchange(p, x, y, z, Block.air);

                }
                else //bloc posé
                {
                    bool redOn = false;
                    byte bC = Block.Zero;
                    if (type == Block.red)
                    {
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

                    if (type == Block.redmushroom)
                    {
                        p.level.Blockchange(p, x, y, z, Block.red_torche_off);
                        p.level.AddCheck(p.level.PosToInt(x, y, z), "", true);
                    }

                }
            }
        }
    }
}
