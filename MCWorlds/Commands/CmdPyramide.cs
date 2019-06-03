using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdPyramide : Command
    {
        public override string name { get { return "pyramide"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPyramide() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.Split(' ').Length > 3) { Help(p); return; }

            CatchPos cpos;
            cpos.x = 0; cpos.y = 0; cpos.z = 0; cpos.bas = false; cpos.vide = false; cpos.type = Block.Zero;

            if (message.Split(' ').Length == 1)
            {
                if (message == "vide")
                {cpos.vide = true;}
                else if (message == "bas")
                { cpos.bas = true; }
                else
                {
                    cpos.type = Block.Byte(message);
                    if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + message + "\"."); return; }
                    if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                }
            }
            else if (message.Split(' ').Length == 2)
            {
                string param1 = message.Split(' ')[0];
                string param2 = message.Split(' ')[1];

                if (param1 == "vide")
                { cpos.vide = true; }
                else if (param1 == "bas")
                { cpos.bas = true; }
                else
                {
                    cpos.type = Block.Byte(param1);
                    if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + param1 + "\"."); return; }
                    if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                }

                if (param2 == "vide")
                { cpos.vide = true; }
                else if (param2 == "bas")
                { cpos.bas = true; }
                else { Help(p); return; }

            }
            else
            {
                string param1 = message.Split(' ')[1];
                string param2 = message.Split(' ')[2];

                byte t = Block.Byte(message.Split(' ')[0]);
                if (t == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + message + "\"."); return; }
                if (!Block.canPlace(p, t)) { Player.SendMessage(p, "Impossible de placer ca."); return; }
                cpos.type = t;

                if (param1 == "vide")
                { cpos.vide = true; }
                else if (param1 == "bas")
                { cpos.bas = true; }
                else { Help(p); return; }
                
                if (param2 == "vide")
                { cpos.vide = true; }
                else if (param2 == "bas")
                { cpos.bas = true; }
                else { Help(p); return; }
            }

            p.blockchangeObject = cpos;
            
            Player.SendMessage(p, "Place 2 blocs pour determiner les tailles.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/pyramide [type] <vide> <bas> - Cree une pyramide");
            Player.SendMessage(p, "La base est place a la hauteur du point le plus bas");
            Player.SendMessage(p, "Pour la mettre a l'envert ajoutez 'bas'");
            Player.SendMessage(p, "'vide' cree une pyramide creuse");

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
            unchecked { if (cpos.type != Block.Zero) type = cpos.type; else type = p.bindings[type]; }

            int nbBlocs = 0;
            

            int xmin = Math.Min(cpos.x, x), xmax = Math.Max(cpos.x, x);
            int ymin = Math.Min(cpos.y, y), ymax = Math.Max(cpos.y, y);
            int zmin = Math.Min(cpos.z, z), zmax = Math.Max(cpos.z, z);
            
            int longeurMin = Math.Min(xmax - xmin + 1 ,zmax - zmin + 1);
            int hauteur = (longeurMin + 1) /2 ;

            if (cpos.vide)
            { nbBlocs = (xmax - xmin + 1) * (zmax - zmin + 1); }
            else
            {
                for (int i = 0; i < hauteur; i++)
                {
                    nbBlocs += (xmax - xmin + 1 - i) * (zmax - zmin + 1 - i);
                }
            }

            if (nbBlocs >= p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayer de faire une pyramide de " + nbBlocs + " blocs.");
                Player.SendMessage(p, "Vous n'avez pas le droit a plus de " + p.maxblocsbuild() + ".");
                return;
            }

            if (!cpos.bas)
            {
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = xmin + i; j <= xmax - i; j++)
                    {
                        for (int k = zmin + i; k <= zmax - i; k++)
                        {
                            if (j == xmin + i || j == xmax - i || k == zmin + i || k == zmax - i || !cpos.vide)
                            { p.level.Blockchange(p, (ushort)j, (ushort)(ymin + i), (ushort)k, type); }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = xmin + i; j <= xmax - i; j++)
                    {
                        for (int k = zmin + i; k <= zmax - i; k++)
                        {
                            if (j == xmin + i || j == xmax - i || k == zmin + i || k == zmax - i || !cpos.vide)
                            { p.level.Blockchange(p, (ushort)j, (ushort)(ymin + hauteur - i), (ushort)k, type); }
                        }
                    }
                }
            }

            Player.SendMessage(p, nbBlocs + " blocs");
            
            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        struct CatchPos
        {
            public bool vide;
            public bool bas;
            public byte type;
            public ushort x, y, z;
        }
    }
}