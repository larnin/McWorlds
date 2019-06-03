using System;
using System.IO;

namespace MCWorlds
{
    public class CmdReveal : Command
    {
        public override string name { get { return "reveal"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdReveal() { }

        public override void Use(Player p, string message)
        {
            if (p == null) { Player.SendMessage(p, "Impossible depuis la console ou l'irc"); return; }

            if (message == "") message = p.name;

            if (message.ToLower() == "all")
            {
                if (p.group.Permission < LevelPermission.Operator) { Player.SendMessage(p, "Reserve aux OP+"); return; }

                foreach (Player who in Player.players)
                {
                    if (who.level == p.level)
                    {

                        who.Loading = true;
                        foreach (Player pl in Player.players) if (who.level == pl.level && who != pl) who.SendDie(pl.id);
                        foreach (PlayerBot b in PlayerBot.playerbots) if (who.level == b.level) who.SendDie(b.id);

                        Player.GlobalDie(who, true);
                        who.SendUserMOTD(); who.SendMap();

                        ushort x = (ushort)((0.5 + who.level.spawnx) * 32);
                        ushort y = (ushort)((1 + who.level.spawny) * 32);
                        ushort z = (ushort)((0.5 + who.level.spawnz) * 32);

                        if (!who.hidden) Player.GlobalSpawn(who, x, y, z, who.level.rotx, who.level.roty, true);
                        else unchecked { who.SendPos((byte)-1, x, y, z, who.level.rotx, who.level.roty); }

                        foreach (Player pl in Player.players)
                            if (pl.level == who.level && who != pl && !pl.hidden)
                                who.SendSpawn(pl.id, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]);

                        foreach (PlayerBot b in PlayerBot.playerbots)
                            if (b.level == who.level)
                                who.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);

                        who.Loading = false;

                        who.SendMessage("&bMap recharge par " + p.Name());
                        Player.SendMessage(p, "&4Rechanrgement de la map fini pour " + who.Name());
                    }
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            else
            {
                Player who = Player.Find(message);
                if (who == null) { Player.SendMessage(p, "Impossible de trouver le joueur."); return; }
                else if (who.group.Permission > p.group.Permission && p != who) { Player.SendMessage(p, "Impossible de recharger la map pour un joueur de rang superieur au votre."); return; }

                who.Loading = true;
                foreach (Player pl in Player.players) if (who.level == pl.level && who != pl) who.SendDie(pl.id);
                foreach (PlayerBot b in PlayerBot.playerbots) if (who.level == b.level) who.SendDie(b.id);

                Player.GlobalDie(who, true);
                who.SendUserMOTD(); who.SendMap();

                ushort x = (ushort)((0.5 + who.level.spawnx) * 32);
                ushort y = (ushort)((1 + who.level.spawny) * 32);
                ushort z = (ushort)((0.5 + who.level.spawnz) * 32);

                if (!who.hidden) Player.GlobalSpawn(who, x, y, z, who.level.rotx, who.level.roty, true);
                else unchecked { who.SendPos((byte)-1, x, y, z, who.level.rotx, who.level.roty); }

                foreach (Player pl in Player.players)
                    if (pl.level == who.level && who != pl && !pl.hidden)
                        who.SendSpawn(pl.id, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]);

                foreach (PlayerBot b in PlayerBot.playerbots)
                    if (b.level == who.level)
                        who.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);

                who.Loading = false;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                who.SendMessage("&bMap recharge par " + p.Name());
                Player.SendMessage(p, "&4Rechargement de la map fini pour " + who.name);
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/reveal <nom> - Revele la map pour un joueur.");
            Player.SendMessage(p, "/reveal all - Revele la map a tous");
        }
    }
}