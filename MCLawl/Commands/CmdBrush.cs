
using System;
using System.Collections.Generic;
using System.IO;

namespace MCWorlds
{
    class CmdBrush : Command
    {
        public override string name { get { return "brush"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdBrush() { }

        public override void Use(Player p, string message)
        {
            if (message == "types") { types(p); return; }

            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "list" || message == "liste") { list(p); return; }

            if (message.Split(' ')[0] == "clear")
            {
                foreach (Brush b in p.brushs)
                {
                    if (b.blocBrush == Block.Zero) { continue; }

                    b.clearBloc();
                    b.clearBrush();
                    b.clearMask();
                }

                Player.SendMessage(p, "Tous les brushs sont supprime");
                return;
            }

            if (message.Split(' ')[0] == "save")
            {
                bool brushOn = false;

                foreach (Brush b in p.brushs)
                { if (b.blocBrush != Block.Zero) { brushOn = true; } }
                if (!brushOn) { Player.SendMessage(p, "Vous avez aucun brush actif"); return; }

                if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
                if (!Directory.Exists("extra/brush")) { Directory.CreateDirectory("extra/brush"); }

                StreamWriter SW = new StreamWriter(File.Create("extra/brush/" + p.name.ToLower() + ".txt"));

                foreach (Brush b in p.brushs)
                {
                    if (b.blocBrush == Block.Zero) { continue; }

                    SW.WriteLine("brush");
                    SW.WriteLine("type = " + b.type);
                    SW.WriteLine("blocbrush = " + b.blocBrush);
                    SW.WriteLine("blocset = " + b.blocSet);
                    if (b.size.Count != 0)
                    {
                        string tailles = "";
                        foreach (int t in b.size)
                        { tailles += t + " "; }
                        SW.WriteLine("tailles = " + tailles);
                    }
                    if (b.mask)
                    {
                        if (b.not)
                        { SW.WriteLine("mask = not"); }
                        else
                        { SW.WriteLine("mask = normal"); }

                        string blocs = "";
                        foreach (byte bl in b.blocks)
                        { blocs += Block.Name(bl) + " "; }
                        SW.WriteLine("blocs = " + blocs);
                    }
                    SW.WriteLine("endbrush");
                }

                SW.Flush();
                SW.Close();
                SW.Dispose();

                Player.SendMessage(p, "brushs sauvegarde");
                return;
            }

            if (message.Split(' ')[0] == "load")
            {
                if (!Directory.Exists("extra")) { Directory.CreateDirectory("extra"); }
                if (!Directory.Exists("extra/brush")) { Directory.CreateDirectory("extra/brush"); }

                if (!File.Exists("extra/brush/" + p.name.ToLower() + ".txt")) { Player.SendMessage(p, "Vous n'avez pas de brush sauvegarde"); return; }

                string[] lignes = File.ReadAllLines("extra/brush/" + p.name.ToLower() + ".txt");

                Use(p, "clear");

                int numbrush = 0;
                int maxbrush = p.brushs.Length;
                Brush b = new Brush();

                string typeb = "", sizes = "", bs = "";
                byte blocset = Block.Zero, blocbrush = Block.Zero;
                bool notbs = false, mask = false;


                foreach (string l in lignes)
                {
                    if (numbrush > maxbrush) { continue; }
                    if (l == "") { continue; }
                    if (l[0] == '#') { continue; }

                    if (l == "brush")
                    {
                        numbrush++;
                        if (numbrush > maxbrush) { continue; }

                        b = p.brushs[numbrush - 1];
                        continue;
                    }

                    if (l == "endbrush")
                    {
                        b.blocBrush = blocbrush;
                        b.setBrush(typeb, sizes, blocset, p);

                        if (mask)
                        { b.setMask(bs, notbs, p); }

                        typeb = ""; sizes = ""; bs = "";
                        blocset = Block.Zero; blocbrush = Block.Zero;
                        notbs = false; mask = false;
                    }

                    if (l.IndexOf("=") == -1) { continue; }

                    string key = l.Split('=')[0].Trim();
                    string value = l.Split('=')[1].Trim();

                    switch (key)
                    {
                        case "type":
                            typeb = value;
                            break;
                        case "blocbrush":
                            try { blocbrush = byte.Parse(value); }
                            catch { }
                            break;
                        case "blocset":
                            try { blocset = byte.Parse(value); }
                            catch { }
                            break;
                        case "tailles":
                            sizes = value;
                            break;
                        case "mask":
                            if (value == "not") { notbs = true; }
                            else { notbs = false; }
                            break;
                        case "blocs":
                            bs = value;
                            mask = true;
                            break;
                        default:
                            continue;
                    }
                }

                Player.SendMessage(p, "brushs charge");
                return;
            }

            if (message.IndexOf(" ") == -1) { Help(p); return; }

            byte blocBrush = Block.Byte(message.Split(' ')[0]);
            if (blocBrush == Block.Zero) { Player.SendMessage(p, "Bloc '" + message.Split(' ')[0] + "' inconnu !"); return; }
            if (!Block.AnyBuild(blocBrush)) { Player.SendMessage(p, "Impossible d'appliquer un brush a ce bloc"); return; }

            message = message.Substring(message.IndexOf(' ') + 1);

            if (message.Split(' ')[0] == "del")
            {
                Brush br = new Brush();
                foreach (Brush b in p.brushs)
                {
                    if (b.blocBrush == blocBrush)
                    { br = b; break; }
                }

                if (br.blocBrush == Block.Zero) { Player.SendMessage(p, "Le bloc '" + Block.Name(blocBrush) + "' n'a pas de brush"); return; }

                br.clearBloc();
                br.clearBrush();
                br.clearMask();
                Player.SendMessage(p, "Brush du bloc '" + Block.Name(blocBrush) + " supprime");
                return;
            }

            bool bfind = false;
            bool newBrush = false;
            Brush brs = new Brush();
            foreach (Brush b in p.brushs)
            {
                if (b.blocBrush == blocBrush)
                { brs = b; bfind = true; break; }
            }

            if (!bfind)
            {
                foreach (Brush b in p.brushs)
                {
                    if (b.blocBrush == Block.Zero)
                    { brs = b; bfind = true; break; }
                }
                if (!bfind) { Player.SendMessage(p, "Vous ne pouvez pas creer de nouveau brush"); return; }

                brs.blocBrush = blocBrush;
                newBrush = true;
            }

            byte bloc = Block.Byte(message.Split(' ')[0]);
            if (bloc == Block.Zero) { Player.SendMessage(p, "Bloc '" + message.Split(' ')[0] + "' inconnu !"); return; }
            if (!Block.AnyBuild(bloc)) { Player.SendMessage(p, "Impossible d'appliquer un brush a ce bloc"); return; }

            message = message.Substring(message.IndexOf(' ') + 1);
            string type = message.Split(' ')[0].ToLower();
            string size = message.Substring(message.IndexOf(' ') + 1);

            if (!brs.setBrush(type, size, bloc, p)) { return; }

            if (newBrush)
            {
                Player.SendMessage(p, "Nouveau brush cree sur le bloc &b" + Block.Name(brs.blocBrush));
            }
            else
            { Player.SendMessage(p, "Brush du bloc &b" + Block.Name(brs.blocBrush) + Server.DefaultColor + " modifie"); }
        }

        private void list(Player p)
        {
            int nbBrush = 0;
            foreach (Brush b in p.brushs) { if (b.blocBrush != Block.Zero) { nbBrush++; } }

            Player.SendMessage(p, "Vous avez " + nbBrush + " brush actif");
            if (nbBrush == 0) { return; }

            foreach (Brush b in p.brushs)
            {
                if (b.blocBrush == Block.Zero){continue;}

                string blocksTxt = "";
                string sizeTxt = "";
                foreach (int size in b.size) { sizeTxt += size + ", "; }
                foreach (byte bloc in b.blocks) { blocksTxt += Block.Name(bloc) + ", "; }
                
                string message = "Bloc &b" + Block.Name(b.blocBrush) + Server.DefaultColor;
                if (b.type != "") 
                { 
                    message += " - brush &b" + b.type + " " + Block.Name(b.blocSet) + Server.DefaultColor;
                    if (sizeTxt != "") { message += " - taille &b" + sizeTxt.Remove(sizeTxt.Length - 2); }
                }
                Player.SendMessage(p, message);

                if (blocksTxt != "" && b.mask)
                {
                    blocksTxt = blocksTxt.Remove(blocksTxt.Length - 2);
                    if (b.not) { Player.SendMessage(p, "Mask tout sauf : &5" + blocksTxt); }
                    else { Player.SendMessage(p, "Mask : &5" + blocksTxt); }
                }
            }
        }

        public override void Help(Player p, string message = "")
        {
            if (message == "types")
            { types(p); return; }
            Player.SendMessage(p, "/brush [bloc select] [bloc] [type] [tailles] - Cree un brush");
            Player.SendMessage(p, "/brush [bloc select] del - Supprime un brush");
            Player.SendMessage(p, "/brush clear - Supprime tous les brushs");
            Player.SendMessage(p, "Un brush est execute a chaque bloc [bloc select] pose");
            Player.SendMessage(p, "Un mask peut etre applique a un brush (voir /mask)");
            Player.SendMessage(p, "Vous pouvez creer 9 brush differents");
            Player.SendMessage(p, "/help brush types - liste les types de brush");
            Player.SendMessage(p, "/brush list - liste les brushs cree");
            Player.SendMessage(p, "/brush save - Sauvegarde vos brushs");
            Player.SendMessage(p, "/brush load - charge la derniere sauvegarde des brushs");
        }

        private void types(Player p)
        {
            Player.SendMessage(p, "Les types disponible : (les tailles sont entre 1 et 8)");
            Player.SendMessage(p, "&2Un seul parametre de taille");
            Player.SendMessage(p, "&5sphere" + Server.DefaultColor + " - une sphere");
            Player.SendMessage(p, "&5hsphere" + Server.DefaultColor + " - une sphere creuse");
            Player.SendMessage(p, "&5cube" + Server.DefaultColor + " - un cube");
            Player.SendMessage(p, "&5hcube" + Server.DefaultColor + " - un cube creu");
            Player.SendMessage(p, "&5pyramide" + Server.DefaultColor + " - une pyramide (hauteur)");
            Player.SendMessage(p, "&5hpyramide" + Server.DefaultColor + " - une pyramide vide");
            Player.SendMessage(p, "&2Deux parametres de taille");
            Player.SendMessage(p, "&5cylindre" + Server.DefaultColor + " - un cylindre vertical (rayon et hauteur)");
            Player.SendMessage(p, "&5hcylindre" + Server.DefaultColor + " - un cylindre creu (rayon et hauteur)");
            Player.SendMessage(p, "&5lisse" + Server.DefaultColor + " - lisse la zone (largeur et hauteur)");
            Player.SendMessage(p, "&5surligne" + Server.DefaultColor + " - change les blocs de surface (diametre et hauteur de la surface)");
        }
    }
}
