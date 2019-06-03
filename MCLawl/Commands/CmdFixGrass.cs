using System;

namespace MCWorlds
{
    public class CmdFixGrass : Command
    {
        public override string name { get { return "fixgrass"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "moderation"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdFixGrass() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            int totalFixed = 0;

            switch (message.ToLower())
            {
                case "":
                    for (int i = 0; i < p.level.blocks.Length; i++)
                    {
                        try
                        {
                            ushort x, y, z;
                            p.level.IntToPos(i, out x, out y, out z);

                            if (p.level.blocks[i] == Block.dirt)
                            {
                                if (Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)]))
                                {
                                    p.level.Blockchange(p, x, y, z, Block.grass);
                                    totalFixed++;
                                }
                            }
                            else if (p.level.blocks[i] == Block.grass)
                            {
                                if (!Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)]))
                                {
                                    p.level.Blockchange(p, x, y, z, Block.dirt);
                                    totalFixed++;
                                }
                            }
                        }
                        catch { }
                    } break;
                case "light":
                    for (int i = 0; i < p.level.blocks.Length; i++)
                    {
                        try
                        {
                            ushort x, y, z; bool skipMe = false;
                            p.level.IntToPos(i, out x, out y, out z);

                            if (p.level.blocks[i] == Block.dirt)
                            {
                                for (int iL = 1; iL < (p.level.depth - y); iL++)
                                {
                                    if (!Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, iL, 0)]))
                                    {
                                        skipMe = true; break;
                                    }
                                }
                                if (!skipMe)
                                {
                                    p.level.Blockchange(p, x, y, z, Block.grass);
                                    totalFixed++;
                                }
                            }
                            else if (p.level.blocks[i] == Block.grass)
                            {
                                for (int iL = 1; iL < (p.level.depth - y); iL++)
                                {
                                    if (Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, iL, 0)]))
                                    {
                                        skipMe = true; break;
                                    }
                                }
                                if (!skipMe)
                                {
                                    p.level.Blockchange(p, x, y, z, Block.dirt);
                                    totalFixed++;
                                }
                            }
                        }
                        catch { }
                    } break;
                case "grass":
                    for (int i = 0; i < p.level.blocks.Length; i++)
                    {
                        try
                        {
                            ushort x, y, z;
                            p.level.IntToPos(i, out x, out y, out z);

                            if (p.level.blocks[i] == Block.grass)
                                if (!Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)]))
                                {
                                    p.level.Blockchange(p, x, y, z, Block.dirt);
                                    totalFixed++;
                                }
                        }
                        catch { }
                    } break;
                case "dirt":
                    for (int i = 0; i < p.level.blocks.Length; i++)
                    {
                        try
                        {
                            ushort x, y, z;
                            p.level.IntToPos(i, out x, out y, out z);

                            if (p.level.blocks[i] == Block.dirt)
                                if (Block.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)]))
                                {
                                    p.level.Blockchange(p, x, y, z, Block.grass);
                                    totalFixed++;
                                }
                        }
                        catch { }
                    } break;
                default:
                    Help(p);
                    return;
            }

            Player.SendMessage(p, "Fixed " + totalFixed + " blocks.");
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/fixgrass <type> - Fixe l'herbe en fonction du type");
            Player.SendMessage(p, "<type> est \"\": Toute l'herbe avec quelque chose sur le dessus est fait de terre, la terre sans rien dessus est fait d'herbe");
            Player.SendMessage(p, "<type> est \"light\": Seule la terre en plein soleil devient de l'herbe");
            Player.SendMessage(p, "<type> est \"grass\": Seulement l'herbe sous un bloc est transformee en terre");
            Player.SendMessage(p, "<type> est \"dirt\": Ne change que la terre sans rien dessus en herbe");
        }
    }
}