using System;
using System.Collections.Generic;


namespace MCWorlds
{
    public class Team
    {
        public char color;
        public int points = 0;
        public ushort[] flagBase = { 0, 0, 0 };
        public ushort[] flagLocation = { 0, 0, 0 };
        public List<Spawn> spawns = new List<Spawn>();
        public List<Player> players = new List<Player>();
        public BaseGame game;
        public bool flagishome;
        public bool spawnset;
        public string teamstring = "";
        public Player holdingFlag = null;
        public CatchPos tempFlagblock;
        public CatchPos tfb;
        public int ftcount = 0;

        public void AddMember(Player p)
        {
            if (p.team != this)
            {
                if (p.carryingFlag) { p.spawning = true; ((CTFGame2)game).DropFlag(p, p.hasflag); p.spawning = false; }
                if (p.team != null) { p.team.RemoveMember(p); }
                p.team = this;
                Player.GlobalDie(p, false);
                p.GameTempcolor = p.color;
                p.GameTempprefix = p.prefix;
                p.color = "&" + color;
                p.carryingFlag = false;
                p.hasflag = null;
                p.prefix = p.color + "[" + c.Name("&" + color).ToUpper() + "] ";
                players.Add(p);
                game.lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " a rejoin l'equipe " + teamstring + ".");
                Player.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
                if (game.gameOn)
                {
                    SpawnPlayer(p);
                }
            }
        }

        public void RemoveMember(Player p)
        {
            if (p.team == this)
            {
                if (p.carryingFlag)
                {
                    ((CTFGame2)game).DropFlag(p, p.hasflag);
                }
                p.team = null;
                Player.GlobalDie(p, false);
                p.color = p.GameTempcolor;
                p.prefix = p.GameTempprefix;
                p.carryingFlag = false;
                p.hasflag = null;
                players.Remove(p);
                game.lvl.ChatLevel(p.color + p.prefix + p.name + Server.DefaultColor + " a quitte l'equipe " + teamstring + ".");
                Player.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
                
                if (p.ingame)
                {
                    p.sendAll();
                    p.ingame = false;
                    p.gameMessages.Clear();
                    p.tailleBufferGame = 0;
                }
            }
        }

        public void SpawnPlayer(Player p)
        {
            p.lastTp = DateTime.Now;
            p.spawning = true;
            if (spawns.Count != 0)
            {
                Random random = new Random();
                int rnd = random.Next(0, spawns.Count);
                ushort x, y, z, rotx;

                x = spawns[rnd].x;
                y = spawns[rnd].y;
                z = spawns[rnd].z;

                ushort x1 = (ushort)((0.5 + x) * 32);
                ushort y1 = (ushort)((1 + y) * 32);
                ushort z1 = (ushort)((0.5 + z) * 32);
                rotx = spawns[rnd].rotx;
                unchecked
                {
                    p.SendSpawn((byte)-1, p.name, x1, y1, z1, (byte)rotx, 0);
                }
                p.health = 100;
            }
            else
            {
                ushort x = (ushort)((0.5 + game.lvl.spawnx) * 32);
                ushort y = (ushort)((1 + game.lvl.spawny) * 32);
                ushort z = (ushort)((0.5 + game.lvl.spawnz) * 32);
                ushort rotx = game.lvl.rotx;
                ushort roty = game.lvl.roty;

                unchecked
                {
                    p.SendSpawn((byte)-1, p.name, x, y, z, (byte)rotx, (byte)roty);
                }
            }
            p.spawning = false;
        }

        public void AddSpawn(ushort x, ushort y, ushort z, ushort rotx, ushort roty)
        {
            Spawn workSpawn = new Spawn();
            workSpawn.x = x;
            workSpawn.y = y;
            workSpawn.z = z;
            workSpawn.rotx = rotx;
            workSpawn.roty = roty;

            spawns.Add(workSpawn);
        }

        public void Drawflag()
        {
            
            ushort x = flagLocation[0];
            ushort y = flagLocation[1];
            ushort z = flagLocation[2];

            if (game.lvl.GetTile(x, (ushort)(y - 1), z) == Block.air)
            {
                flagLocation[1] = (ushort)(flagLocation[1] - 1);
            }

            game.lvl.Blockchange(tfb.x, tfb.y, tfb.z, tfb.type);
            game.lvl.Blockchange(tfb.x, (ushort)(tfb.y + 1), tfb.z, Block.air);
            game.lvl.Blockchange(tfb.x, (ushort)(tfb.y + 2), tfb.z, Block.air);

            if (holdingFlag == null)
            {
                //DRAW ON GROUND SHIT HERE

                tfb.type = game.lvl.GetTile(x, y, z);

                if (game.lvl.GetTile(x, y, z) != Block.flagbase) { game.lvl.Blockchange(x, y, z, Block.flagbase); }
                if (game.lvl.GetTile(x, (ushort)(y + 1), z) != Block.mushroom) { game.lvl.Blockchange(x, (ushort)(y + 1), z, Block.mushroom); }
                if (game.lvl.GetTile(x, (ushort)(y + 2), z) != GetColorBlock(color)) { game.lvl.Blockchange(x, (ushort)(y + 2), z, GetColorBlock(color)); }

                tfb.x = x;
                tfb.y = y;
                tfb.z = z;

            }
            else
            {
                //DRAW ON PLAYER HEAD
                x = (ushort)(holdingFlag.pos[0] / 32);
                y = (ushort)(holdingFlag.pos[1] / 32 + 3);
                z = (ushort)(holdingFlag.pos[2] / 32);

                if (tempFlagblock.x == x && tempFlagblock.y == y && tempFlagblock.z == z) { return; }


                game.lvl.Blockchange(tempFlagblock.x, tempFlagblock.y, tempFlagblock.z, tempFlagblock.type);

                tempFlagblock.type = game.lvl.GetTile(x, y, z);

                game.lvl.Blockchange(x, y, z, GetColorBlock(color));

                tempFlagblock.x = x;
                tempFlagblock.y = y;
                tempFlagblock.z = z;
            }
            

        }

        public static byte GetColorBlock(char color)
        {
            if (color == '2')
                return Block.green;
            if (color == '5')
                return Block.purple;
            if (color == '8')
                return Block.darkgrey;
            if (color == '9')
                return Block.blue;
            if (color == 'c')
                return Block.red;
            if (color == 'e')
                return Block.yellow;
            if (color == 'f')
                return Block.white;
            else
                return Block.air;
        }

        public struct CatchPos { public ushort x, y, z; public byte type; }
        public struct Spawn { public ushort x, y, z, rotx, roty; }
    }
}