﻿using System;
using System.Collections.Generic;

namespace MCWorlds
{
    public class CmdFill : Command
    {
        public override string name { get { return "fill"; } }
        public override string shortcut { get { return "f"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdFill() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            CatchPos cpos;

            int number = message.Split(' ').Length;
            if (number > 2) { Help(p); return; }
            if (number == 2)
            {
                int pos = message.IndexOf(' ');
                string t = message.Substring(0, pos).ToLower();
                string s = message.Substring(pos + 1).ToLower();
                cpos.type = Block.Byte(t);
                if (cpos.type == 255) { Player.SendMessage(p, "Il n'existe pas de bloc \"" + t + "\"."); return; }

                if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }

                if (s == "up") cpos.FillType = 1;
                else if (s == "down") cpos.FillType = 2;
                else if (s == "layer") cpos.FillType = 3;
                else if (s == "vertical_x") cpos.FillType = 4;
                else if (s == "vertical_z") cpos.FillType = 5;
                else { Player.SendMessage(p, "Type de peinture invalide"); return; }
            }
            else if (message != "")
            {
                message = message.ToLower();
                if (message == "up") { cpos.FillType = 1; cpos.type = Block.Zero; }
                else if (message == "down") { cpos.FillType = 2; cpos.type = Block.Zero; }
                else if (message == "layer") { cpos.FillType = 3; cpos.type = Block.Zero; }
                else if (message == "vertical_x") { cpos.FillType = 4; cpos.type = Block.Zero; }
                else if (message == "vertical_z") { cpos.FillType = 5; cpos.type = Block.Zero; }
                else
                {
                    cpos.type = Block.Byte(message);
                    if (cpos.type == (byte)255) { Player.SendMessage(p, "Bloc ou peinture invalide"); return; }
                    if (!Block.canPlace(p, cpos.type)) { Player.SendMessage(p, "Impossible de placer ca."); return; }

                    cpos.FillType = 0;
                }
            }
            else
            {
                cpos.type = Block.Zero; cpos.FillType = 0;
            }

            cpos.x = 0; cpos.y = 0; cpos.z = 0; p.blockchangeObject = cpos;

            Player.SendMessage(p, "Detruisez un bloc ou vous voulez peindre."); p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        public override void Help(Player p, string message = "")
        {
            if (message == "types")
            {
                Player.SendMessage(p, "up - permet de ne peindre que les blocs situe au dessus du bloc clique");
                Player.SendMessage(p, "down - permet de ne peindre que les blocs situe en dessous");
                Player.SendMessage(p, "layer - permet de ne peindre que les blocs a la meme hauteur que celui clique");
                Player.SendMessage(p, "vertical_x - ne peint que les blocs en vertical sur le meme x");
                Player.SendMessage(p, "vertical_z - pareil que vertical_x sauf qu'il ne peint que sur le meme z");
            }
            Player.SendMessage(p, "/fill <bloc> <type> - Peint une zone avec [bloc].");
            Player.SendMessage(p, "<types> - up, down, layer, vertical_x, vertical_z");
            Player.SendMessage(p, "/help fill types - pour plus d'infos sur les types");
        }
        
        public void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type)
        {
            try
            {
                p.ClearBlockchange();
                CatchPos cpos = (CatchPos)p.blockchangeObject;
                if (cpos.type == Block.Zero) cpos.type = p.bindings[type];

                byte oldType = p.level.GetTile(x, y, z);
                p.SendBlockchange(x, y, z, oldType);

                if (cpos.type == oldType) { Player.SendMessage(p, "Impossible de peindre avec le meme type"); return; }
                if (!Block.canPlace(p, oldType) && !Block.BuildIn(oldType)) { Player.SendMessage(p, "Impossible de peindre ca."); return; }

                byte[] mapBlocks = new byte[p.level.blocks.Length];
                List<Pos> buffer = new List<Pos>();
                p.level.blocks.CopyTo(mapBlocks, 0);

                fromWhere.Clear();
                deep = 0;
                FloodFill(p, x, y, z, cpos.type, oldType, cpos.FillType, ref mapBlocks, ref buffer);

                int totalFill = fromWhere.Count;
                for (int i = 0; i < totalFill && buffer.Count < p.maxblocsbuild(); i++)
                {
                    totalFill = fromWhere.Count;
                    Pos pos = fromWhere[i];
                    deep = 0;
                    FloodFill(p, pos.x, pos.y, pos.z, cpos.type, oldType, cpos.FillType, ref mapBlocks, ref buffer);
                    totalFill = fromWhere.Count;
                }
                fromWhere.Clear();

                if (buffer.Count > p.maxblocsbuild())
                {
                    Player.SendMessage(p, "Vous essayez de peindre " + buffer.Count + " blocs.");
                    Player.SendMessage(p, "Vous ne pouvez pas peindre plus de " + p.maxblocsbuild() + ".");
                    buffer.Clear();
                    return;
                }

                foreach (Pos pos in buffer)
                {
                    p.level.Blockchange(p, pos.x, pos.y, pos.z, cpos.type);
                }

                Player.SendMessage(p, "Peint " + buffer.Count + " blocs.");
                buffer.Clear();

                if (p.staticCommands) p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
            }
            catch (Exception e)
            {
                Server.ErrorLog(e);
            }
        }

        int deep;
        List<Pos> fromWhere = new List<Pos>();
        public void FloodFill(Player p, ushort x, ushort y, ushort z, byte b, byte oldType, int fillType, ref byte[] blocks, ref List<Pos> buffer)
        {
            try
            {
                Pos pos;
                pos.x = x; pos.y = y; pos.z = z;

                if (deep > 4000)
                {
                    fromWhere.Add(pos);
                    return;
                }

                blocks[x + p.level.width * z + p.level.width * p.level.height * y] = b;
                buffer.Add(pos);

                //x
                if (fillType != 4)
                {
                    if (GetTile((ushort)(x + 1), y, z, p.level, blocks) == oldType)
                    {
                        deep++;
                        FloodFill(p, (ushort)(x + 1), y, z, b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }

                    if (x - 1 > 0)
                        if (GetTile((ushort)(x - 1), y, z, p.level, blocks) == oldType)
                        {
                            deep++;
                            FloodFill(p, (ushort)(x - 1), y, z, b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }

                //z
                if (fillType != 5)
                {
                    if (GetTile(x, y, (ushort)(z + 1), p.level, blocks) == oldType)
                    {
                        deep++;
                        FloodFill(p, x, y, (ushort)(z + 1), b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }

                    if (z - 1 > 0)
                        if (GetTile(x, y, (ushort)(z - 1), p.level, blocks) == oldType)
                        {
                            deep++;
                            FloodFill(p, x, y, (ushort)(z - 1), b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }

                //y
                if (fillType == 0 || fillType == 1 || fillType > 3)
                {
                    if (GetTile(x, (ushort)(y + 1), z, p.level, blocks) == oldType)
                    {
                        deep++;
                        FloodFill(p, x, (ushort)(y + 1), z, b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }
                }

                if (fillType == 0 || fillType == 2 || fillType > 3)
                {
                    if (y - 1 > 0)
                        if (GetTile(x, (ushort)(y - 1), z, p.level, blocks) == oldType)
                        {
                            deep++;
                            FloodFill(p, x, (ushort)(y - 1), z, b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }
            } catch (Exception e) { Server.ErrorLog(e); }
        }

        public byte GetTile(ushort x, ushort y, ushort z, Level l, byte[] blocks)
        {
            //if (PosToInt(x, y, z) >= blocks.Length) { return null; }
            //Avoid internal overflow
            if (x < 0) { return Block.Zero; }
            if (x >= l.width) { return Block.Zero; }
            if (y < 0) { return Block.Zero; }
            if (y >= l.depth) { return Block.Zero; }
            if (z < 0) { return Block.Zero; }
            if (z >= l.height) { return Block.Zero; }
            try
            {
                return blocks[l.PosToInt(x, y, z)];
            }
            catch (Exception e) { Server.ErrorLog(e); return Block.Zero; }
        }

        struct CatchPos { public ushort x, y, z; public byte type; public int FillType; }
        public struct Pos { public ushort x, y, z; }
    }
}