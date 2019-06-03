using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdRestartPhysics : Command
    {
        public override string name { get { return "restartphysics"; } }
        public override string shortcut { get { return "rp"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdRestartPhysics() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;
            cpos.x = 0; cpos.y = 0; cpos.z = 0;

            message = message.ToLower();
            cpos.extraInfo = "";

            if (message != "")
            {
                int currentLoop = 0; string[] storedArray; bool skip = false;

            retry: foreach (string s in message.Split(' '))
                {
                    if (currentLoop % 2 == 0)
                    {
                        switch (s)
                        {
                            case "drop":
                            case "explode":
                            case "dissipate":
                            case "finite":
                            case "wait":
                            case "rainbow":
                                break;
                            case "revert":
                                if (skip) break;
                                storedArray = message.Split(' ');
                                try
                                {
                                    storedArray[currentLoop + 1] = Block.Byte(message.Split(' ')[currentLoop + 1].ToString().ToLower()).ToString();
                                    if (storedArray[currentLoop + 1].ToString() == "255") throw new OverflowException();
                                }
                                catch { Player.SendMessage(p, "Bloc invalide."); return; }

                                message = string.Join(" ", storedArray);
                                skip = true; currentLoop = 0;

                                goto retry;
                            default:
                                Player.SendMessage(p, s + " n'est pas supporte."); return;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (int.Parse(s) < 1) { Player.SendMessage(p, "Les valeurs doivent etre superieur a 0"); return; }
                        }
                        catch { Player.SendMessage(p, "/rp [type] [num] [type] [num]"); return; }
                    }

                    currentLoop++;
                }

                if (currentLoop % 2 != 1) cpos.extraInfo = message;
                else { Player.SendMessage(p, "Le nombre de parametres doit etre paire."); Help(p); return; }
            }

            p.blockchangeObject = cpos;
            Player.SendMessage(p, "Place deux blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/restartphysics <[type] [num]> <[type2] [num2]> ... - Redemarre la physique des blocs dans la zone.");
            Player.SendMessage(p, "[type] permet de placer une physique personnalise.");
            Player.SendMessage(p, "[types] Possible: drop, explode, dissipate, finite, wait, rainbow, revert");
            Player.SendMessage(p, "/rp revert prend le nom des blocs en [num]");
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
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;
            List<CatchPos> buffer = new List<CatchPos>();
            CatchPos pos = new CatchPos();
            //int totalChecks = 0;

            //if (Math.Abs(cpos.x - x) * Math.Abs(cpos.y - y) * Math.Abs(cpos.z - z) > 8000) { Player.SendMessage(p, "Tried to restart too many blocks. You may only restart 8000"); return; }

            for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
            {
                for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                {
                    for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    {
                        if (p.level.GetTile(xx, yy, zz) != Block.air)
                        {
                            pos.x = xx; pos.y = yy; pos.z = zz;
                            pos.extraInfo = cpos.extraInfo;
                            buffer.Add(pos);
                        }
                    }
                }
            }

            try
            {
                if (cpos.extraInfo == "")
                {
                    if (buffer.Count > Server.rpNormLimit)
                    {
                        Player.SendMessage(p, "Vous ne pouvez pas restart plus de " + Server.rpNormLimit + " blocs.");
                        Player.SendMessage(p, "Vous essayez de restart " + buffer.Count + " blocs.");
                        return;
                    }
                }
                else
                {
                    if (buffer.Count > Server.rpLimit)
                    {
                        Player.SendMessage(p, "Vous essayez d'ajouter une physique sur " + buffer.Count + " blocs.");
                        Player.SendMessage(p, "Vous ne pouvez pas ajouter une physique sur plus de " + Server.rpLimit + " blocs.");
                        return;
                    }
                }
            }
            catch { return; }

            foreach (CatchPos pos1 in buffer)
            {
                p.level.AddCheck(p.level.PosToInt(pos1.x, pos1.y, pos1.z), pos1.extraInfo, true);
            }

            Player.SendMessage(p, buffer.Count + " blocs active.");
            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        struct CatchPos { public ushort x, y, z; public string extraInfo; }
    }
}
