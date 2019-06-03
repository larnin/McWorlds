using System;

namespace MCWorlds
{
    public class CmdRoll : Command
    {
        public override string name { get { return "roll"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return "other"; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdRoll() { }

        public override void Use(Player p, string message)
        {
            int min, max; Random rand = new Random();
            try { min = int.Parse(message.Split(' ')[0]); }
            catch { min = 1; }
            try { max = int.Parse(message.Split(' ')[1]); }
            catch { max = 7; }

            if (p == null) { Player.GlobalMessage( "Console lance un &a" + rand.Next(Math.Min(min, max), Math.Max(min, max) + 1).ToString() + Server.DefaultColor + " (" + Math.Min(min, max) + "|" + Math.Max(min, max) + ")"); }
            else { Player.GlobalMessage(p.color + p.Name() + Server.DefaultColor + " lance un &a" + rand.Next(Math.Min(min, max), Math.Max(min, max) + 1).ToString() + Server.DefaultColor + " (" + Math.Min(min, max) + "|" + Math.Max(min, max) + ")"); }
        }
        public override void Help(Player p, string message = "")
        {
            Player.SendMessage(p, "/roll - Donne un nombre entre 1 et 7");
            Player.SendMessage(p, "/roll <min> <max> - Donne un nombre entre [min] et [max].");
        }
    }
}