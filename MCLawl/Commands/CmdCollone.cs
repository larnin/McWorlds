
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdCollone : Command
    {
        public override string name { get { return "colonne"; } }
        public override string shortcut { get { return "col"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdCollone() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible de faire ca depuis l'irc ou la console"); return; }

            if (message.Split(' ').Length > 2) { Help(p); return; }

            CatchPos cpos;

            cpos.taille = 0;
            cpos.type = Block.Zero;
            cpos.air = false;

            if (message == "") //collone -> parametres par defaut
            { }
            else if (message.IndexOf(' ') == -1) // collone [type] ou collone <taille>
            {
                if (message == "air") { cpos.air = true; }
                else
                {
                    cpos.type = Block.Byte(message);
                    if (cpos.type == Block.Zero)
                    {
                        try { cpos.taille = UInt16.Parse(message); }
                        catch { Player.SendMessage(p, "Bloc inconnu ou taille invalide"); return; }
                    }
                }
            }
            else
            {
                cpos.type = Block.Byte(message.Split(' ')[0]);
                if (cpos.type == 255) { Player.SendMessage(p, "Bloc inconnu"); return; }

                if (message.Split(' ')[1] == "air")
                { cpos.air = true; }
                else
                {
                    try { cpos.taille = UInt16.Parse((message.Split(' ')[1])); }
                    catch { Player.SendMessage(p, "Taille invalide"); return; }
                }
            }
            if (cpos.taille < 0) { cpos.taille = 0; }

            Player.SendMessage(p, "Place un bloc determinant la base de la colonne.");
            p.blockchangeObject = cpos;
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange);
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/colonne [type] [taille] - Cree une colonne.");
            Player.SendMessage(p, "/colonne [type] - Fait une colonne jusqu'au sommet de la map.");
            Player.SendMessage(p, "/colonne [type] air - Fait une colonne dans l'air seulement (s'arette au 1er bloc solide)");
            Player.SendMessage(p, "Le bloc place determine la base de la colonne");
        }
        
        public void Blockchange(Player p, ushort x, ushort y, ushort z, byte type)
        {
            p.ClearBlockchange();
            byte b = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, b);
            CatchPos cpos = (CatchPos)p.blockchangeObject;
            if (cpos.type != Block.Zero) { type = cpos.type; }

            if (cpos.air)
            {
                cpos.taille = 0;
                for (int i = 0; i < p.level.depth; i++)
                {
                    if (p.level.GetTile(x, (ushort)(y + i), z) == Block.air) { cpos.taille++; }
                    else { i = p.level.depth; }
                }
            }
            else if (cpos.taille == 0 || cpos.taille + y > p.level.depth) { cpos.taille = (ushort)(p.level.depth - y); }

            

            if (cpos.taille > p.maxblocsbuild())
            {
                Player.SendMessage(p, "Vous essayez de faire une colonne de " + cpos.taille + " blocs.");
                Player.SendMessage(p, "Vous ne pouvez pas faire une colonne de plus de " + p.maxblocsbuild() + " blocs.");
                return;
            }

            Player.SendMessage(p, cpos.taille + " blocs.");

            for (int i = 0; i < cpos.taille; i++)
            { p.level.Blockchange(p, x, (ushort)(i + y), z, type); }

            if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange);
        }

        struct CatchPos
        {
            public ushort taille;
            public byte type;
            public bool air;
        }
    }
}