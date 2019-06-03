using System;

namespace MCWorlds
{
    public class CmdBind : Command
    {
        public override string name { get { return "bind"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBind() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") { Help(p); return; }
            if (message.Split(' ').Length > 2) { Help(p); return; }
            message = message.ToLower();
            if (message == "clear")
            {
                for (byte d = 0; d < 128; d++) p.bindings[d] = d;
                Player.SendMessage(p, "Tous les liens ont ete defait.");
                return;
            }

            int pos = message.IndexOf(' ');
            if (pos != -1)
            {
                byte b1 = Block.Byte(message.Substring(0, pos));
                byte b2 = Block.Byte(message.Substring(pos + 1));
                if (b1 == Block.Zero) { Player.SendMessage(p, "Il n'y a pas de bloc '" + message.Substring(0, pos) + "'."); return; }
                if (b2 == Block.Zero) { Player.SendMessage(p, "Il n'y a pas de bloc '" + message.Substring(pos + 1) + "'."); return; }

                if (!Block.Placable(b1)) { Player.SendMessage(p, Block.Name(b1) + " n'est pas un bloc special."); return; }
                if (!Block.canPlace(p, b2)) { Player.SendMessage(p, "Vous ne pouvez pas lier " + Block.Name(b2) + "."); return; }
                if (b1 > (byte)64) { Player.SendMessage(p, "Vous ne pouvez rien lier sur ce bloc."); return; }

                if (p.bindings[b1] == b2) { Player.SendMessage(p, Block.Name(b1) + " est deja lier avec " + Block.Name(b2) + "."); return; }

                p.bindings[b1] = b2;
                message = Block.Name(b1) + " est maintenant lier a " + Block.Name(b2) + ".";

                Player.SendMessage(p, message);
            }
            else
            {
                byte b = Block.Byte(message);
                if (b > 100) { Player.SendMessage(p, "Ce bloc ne peut pas être lié"); return; }

                if (p.bindings[b] == b) { Player.SendMessage(p, Block.Name(b) + " n'est pas lier."); return; }
                p.bindings[b] = b; Player.SendMessage(p, "Delie " + Block.Name(b) + ".");
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/bind [bloc] [type] - Remplace bloc avec type.");
            Player.SendMessage(p, "/bind clear - Efface toutes les liaisons.");
        }
    }
}