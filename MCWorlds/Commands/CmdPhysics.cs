using System;

namespace MCWorlds
{
    public class CmdPhysics : Command
    {
        public override string name { get { return "physics"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "information"; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public CmdPhysics() { }

        public override void Use(Player p, string message)
        {
            if (message == "")
            {
                for ( int i = 0 ; i < Server.levels.Count ; i++)
                {
                    if (Server.levels[i].physics > 0)
                        Player.SendMessage(p, "&5" + Server.levels[i].name + Server.DefaultColor + "(" + Server.levels[i].world + ") physics au niveau &b" + Server.levels[i].physics + Server.DefaultColor + ". &cChecks: " + Server.levels[i].lastCheck + "; Sauvegardes: " + Server.levels[i].lastUpdate);
                }
                return;
            }
            try
            {
                int temp = 0; Level level = null;
                if (message.IndexOf(' ') == -1)
                {
                    temp = int.Parse(message);
                    if (p != null)
                    {
                        level = p.level;
                    }
                    else
                    {
                        level = Server.mainLevel;
                    }
                }
                else
                {
                    if (message.Split(' ').Length > 3) { Help(p); return; }
                    string lvl = "";
                    string world = "";
                    if (message.Split(' ').Length == 2)
                    { 
                        lvl = message.Split(' ')[0];
                        if (p == null) { world = Server.mainWorld; }
                        else { world = p.level.world; }
                        temp = System.Convert.ToInt16(message.Split(' ')[1]); }
                    else
                    { lvl = message.Split(' ')[0]; world = message.Split(' ')[1]; temp = System.Convert.ToInt16(message.Split(' ')[2]); }

                    level = Level.Find(lvl,world);
                    if (level == null) { Player.SendMessage(p, "Map introuvable"); return; }
                }

                if (p != null)
                {
                    if (level.world.ToLower() != p.name.ToLower() && p.group.Permission < LevelPermission.Operator)
                    { Player.SendMessage(p, "Vous ne pouvez pas changer le niveau de physiques dans cette map"); return; }

                    if (!p.vip)
                    {
                        if (temp >= 2 && p.group.Permission <= LevelPermission.Builder)
                        { Player.SendMessage(p, "Vous n'avez pas le rang pour mettre ce niveau de physics"); return; }
                    }
                    else
                    {
                        if (temp > 3 && p.group.Permission < LevelPermission.Operator)
                        { Player.SendMessage(p, "Vous n'avez pas le rang pour mettre ce niveau de physics"); return; }
                    }
                }

                string mes = "";

                if (temp >= 0 && temp <= 4)
                {
                    level.setPhysics(temp);
                    switch (temp)
                    {
                        case 0:
                            level.ClearPhysics();
                            mes = "Les physiques sont maintenant &cDesactive" + Server.DefaultColor + " sur &b" + level.name + Server.DefaultColor + ".";
                            Server.s.Log("Physics are now OFF on " + level.name + " " + level.world + ".");
                            IRCBot.Say("Physics are now OFF on " + level.name + " " + level.world + ".");
                            break;

                        case 1:
                            mes ="Les physiques sont maintenant &aNormales" + Server.DefaultColor + " sur &b" + level.name + Server.DefaultColor + ".";
                            Server.s.Log("Physics are now ON on " + level.name + " " + level.world + ".");
                            IRCBot.Say("Physics are now ON on " + level.name + " " + level.world + ".");
                            break;

                        case 2:
                            mes = "Les physiques sont maintenant &aAvancees" + Server.DefaultColor + " sur &b" + level.name + Server.DefaultColor + ".";
                            Server.s.Log("Physics are now ADVANCED on " + level.name + " " + level.world + ".");
                            IRCBot.Say("Physics are now ADVANCED on " + level.name + " " + level.world + ".");
                            break;

                        case 3:
                            mes = "Les physiques sont maintenant &aHardcore" + Server.DefaultColor + " sur &b" + level.name + Server.DefaultColor + ".";
                            Server.s.Log("Physics are now HARDCORE on " + level.name + " " + level.world + ".");
                            IRCBot.Say("Physics are now HARDCORE on " + level.name + " " + level.world + ".");
                            break;

                        case 4:
                            mes = "Les physiques sont maintenant &aInstantanees" + Server.DefaultColor + " sur &b" + level.name + Server.DefaultColor + ".";
                            Server.s.Log("Physics are now INSTANT on " + level.name + " " + level.world + ".");
                            IRCBot.Say("Physics are now INSTANT on " + level.name + " " + level.world + ".");
                            break;
                    }

                    foreach (Player pl in Player.players)
                    {
                        if (pl.level.world == level.world) { Player.SendMessage(pl, mes); }
                    }


                    level.changed = true;
                }
                else
                {
                    Player.SendMessage(p, "Parametre invalide");
                }
            }
            catch
            {
                Player.SendMessage(p, "ENTREE INVALIDE !");
            }
        }

        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/physics - Liste les maps et leurs physics");
            Player.SendMessage(p, "/physics <0/1/2/3/4> - Modifie le niveau des physics de la map");
            Player.SendMessage(p, "/physics [map] <monde> <0/1/2/3/4> - Modifie le niveau des physiques sur une map");
            Player.SendMessage(p, "0-Off 1-On 2-Avancees 3-Hardcore 4-Instantanees");
        }
    }
}