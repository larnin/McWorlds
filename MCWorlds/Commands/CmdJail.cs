using System;
using System.IO;

namespace MCWorlds
{
    public class CmdJail : Command
    {
        public override string name { get { return "jail"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "mod"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdJail() { }

        public override void Use(Player p, string message)
        {
            if ((message.ToLower() == "create" || message.ToLower() == "") && p != null)
            {
                p.level.jailx = p.pos[0]; p.level.jaily = p.pos[1]; p.level.jailz = p.pos[2];
                p.level.jailrotx = p.rot[0]; p.level.jailroty = p.rot[1];
                Player.SendMessage(p, "Point d'enfermement place.");
            }
            else
            {
                Player who = Player.Find(message);
                if (who != null)
                {
                    if (!who.jailed)
                    {
                        if (p != null) if (who.group.Permission >= p.group.Permission) { Player.SendMessage(p, "Impossible d'emprisoner un joueur de rang superieur au votre."); return; }
                        if (who.level != p.level) Command.all.Find("goto").Use(who, p.level.name + " " + p.level.world);
                        Player.GlobalDie(who, false);
                        Player.GlobalSpawn(who, p.level.jailx, p.level.jaily, p.level.jailz, p.level.jailrotx, p.level.jailroty, true);
                        who.jailed = true;
                        Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " est &8enferme" + Server.DefaultColor + ".", false);
                    }
                    else
                    {
                        who.jailed = false;
                        Player.GlobalChat(null, who.color + who.Name() + Server.DefaultColor + " est &aredevenu libre" + Server.DefaultColor + ".", false);
                    }
                }
                else
                {
                    Player.SendMessage(p, "Impossible de trouver le joueur.");
                }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/jail [joueur] - Emprisonne un joueur, il lui sera impossible d'utiliser les commandes.");
            Player.SendMessage(p, "/jail [create] - Cree un point d'emprisonnement sur la map.");
        }
    }
}