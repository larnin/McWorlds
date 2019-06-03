
using System;

namespace MCWorlds
{
    public class CmdAbort : Command
    {
        public override string name { get { return "abort"; } }
        public override string shortcut { get { return "a"; } }
        public override string type { get { return "build"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdAbort() { }

        public override void Use(Player p, string message)
        {
            if (message.Split(' ').Length > 1) { Help(p); return; }
            
            Player pl = p;
            if (p != null)
            {
                if (message != "")
                {
                    if (p.group.Permission < LevelPermission.Operator)
                    { Player.SendMessage(p, "Vous ne pouvez pas annuler les actions d'un autre joueur"); return; }
                    pl = Player.Find(message);
                    if (pl == null) { Player.SendMessage(p, "Joueur introuvable"); return; }
                    if (pl.group.Permission >= p.group.Permission) { Player.SendMessage(p, "Vous ne pouvez pas annuler les actions d'un joueur de rang superieur ou egal au votre"); }
                }
            }
            else
            {
                if (message == "") { Player.SendMessage(p, "Impossible depuis l'irc ou la console"); return; }
                pl = Player.Find(message);
                if (pl == null) { Player.SendMessage(p, "Joueur introuvable"); return; }
            }

            pl.ClearBlockchange();
            pl.painting = false;
            pl.BlockAction = 0;
            pl.staticCommands = false; 
            pl.deleteMode = false;
            pl.modeType = 0;
            pl.aiming = false;
            pl.onTrain = false;
            pl.nuke = false;
            pl.examine = false;
            pl.poseRedstone = false;
            pl.posePiston = false;
            pl.skill = false;
            pl.gamemode = false;

            if ( p == pl)
            Player.SendMessage(p, "Toutes les actions ont ete abandonnee");
            else
            {
                Player.SendMessage(p, "Les actions de " + pl.color + pl.name + Server.DefaultColor + " ont ete abandonnees");
                if (p != null) { Player.SendMessage(pl, "Vos actions ont ete annule par " + p.color + p.Name()); }
                else { Player.SendMessage(pl, "Vos actions ont ete annule par un joueur sur la console ou l'irc"); }
            }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/abort - Abandonne vous actions");
            Player.SendMessage(p, "/abort [joueur] - Abandonne les actions du joueur.");
        }
    }
}