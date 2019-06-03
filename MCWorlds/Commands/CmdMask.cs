
using System;
using System.Collections.Generic;

namespace MCWorlds
{
    class CmdMask : Command
    {
        public override string name { get { return "mask"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdMask() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message.IndexOf(' ') == -1) { message += " del"; }
            if (message.Split(' ').Length < 2) { Help(p); return; }

            byte bloc = Block.Byte(message.Split(' ')[0]);
            if (bloc == Block.Zero) { Player.SendMessage(p, "Bloc '" + message.Split(' ')[0] + "' inconnu !"); return; }
            if (!Block.AnyBuild(bloc)) { Player.SendMessage(p, "Vous ne pouvez pas appliquer un brush a ce bloc"); return; }

            bool bfind = false;
            bool newBrush = false;
            Brush br = new Brush();
            foreach (Brush b in p.brushs)
            {
                if (b.blocBrush == bloc)
                { br = b; bfind = true; break; }
            }

            if (!bfind)
            {
                foreach (Brush b in p.brushs)
                {
                    if (b.blocBrush == Block.Zero)
                    { br = b; bfind = true; break; }
                }
                if (!bfind) { Player.SendMessage(p, "Vous ne pouvez pas creer de nouveau brush"); return; }

                br.blocBrush = bloc;
                newBrush = true;
            }

            if (message.Split(' ')[1] == "not")
            {
                if (message.Split(' ').Length < 3) { Help(p); return; }

                message = message.Substring(message.IndexOf("not") + 4);
                if (!br.setMask(message, true, p))
                { Player.SendMessage(p, "Erreur a la creation du mask"); }
            }
            else if (message.Split(' ')[1] == "del")
            { br.clearMask(); }
            else
            {
                message = message.Substring(message.IndexOf(' ') + 1);
                if (!br.setMask(message, false, p))
                { Player.SendMessage(p, "Erreur a la creation du mask"); }
            }
            if (newBrush)
            {
                Player.SendMessage(p, "Nouveau mask cree sur le bloc &b" + Block.Name(br.blocBrush));
                Player.SendMessage(p, "&cATTENTION: Le brush n'est pas cree, utilisez /brush pour le faire");
            }
            else
            { Player.SendMessage(p, "Mask modifie sur le bloc &b" + Block.Name(br.blocBrush)); }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/mask [bloc select] [bloc1] [bloc2] ... - cree un nouveau mask sur le bloc [bloc select]");
            Player.SendMessage(p, "Lors d'un brush, seul les blocs contenu mask seront modifie");
            Player.SendMessage(p, "/mask [bloc select] not [bloc1] [bloc2] ... - cree un mask ne contenant pas les blocs liste");
            Player.SendMessage(p, "/mask [bloc select] del - Reset le mask");
            Player.SendMessage(p, "Les masks fonctionnent avec les brushs");
            Player.SendMessage(p, "Lire l'help de /brush pour plus d'infos");
        }
    }
}
