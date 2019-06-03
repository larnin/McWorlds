using System;
using System.IO;

namespace MCWorlds
{
    public class CmdMode : Command
    {
        public override string name { get { return "mode"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMode() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "")
            {
                if (p.modeType != 0)
                {
                    Player.SendMessage(p, "&b" + Block.Name(p.modeType)[0].ToString().ToUpper() + Block.Name(p.modeType).Remove(0, 1).ToLower() + Server.DefaultColor + " mode: &cOFF");
                    p.modeType = 0;
                    p.BlockAction = 0;
                }
                else
                {
                    Help(p); return;
                }
            }
            else
            {
                byte b = Block.Byte(message);
                if (b == Block.Zero) { Player.SendMessage(p, "Impossible de trouver le bloc entre."); return; }
                if (b == Block.air) { Player.SendMessage(p, "Impossible d'utiliser le mode air."); return; }
                if (!Block.canPlace(p, b)) { Player.SendMessage(p, "Impossible de placer ce bloc a votre rang."); return; }

                if (p.modeType == b)
                {
                    Player.SendMessage(p, "Mode &b" + Block.Name(p.modeType)[0].ToString().ToUpper() + Block.Name(p.modeType).Remove(0, 1).ToLower() + Server.DefaultColor + " : &cOFF");
                    p.modeType = 0;
                    p.BlockAction = 0;
                }
                else
                {
                    p.BlockAction = 6;
                    p.modeType = b;
                    Player.SendMessage(p, "Mode &b" + Block.Name(p.modeType)[0].ToString().ToUpper() + Block.Name(p.modeType).Remove(0, 1).ToLower() + Server.DefaultColor + " : &aON");
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/mode [block] - Tous les bloc places seront [block].");
            Player.SendMessage(p, "/[block] donne la meme chose");
        }
    }
}