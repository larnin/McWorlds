using System;

namespace MCWorlds
{
   public class CmdUnflood : Command
   {
       public CmdUnflood() { }
      public override string name { get { return "unflood"; } }
      public override string shortcut { get { return ""; } }
      public override string type { get { return "other"; } }
      public override bool museumUsable { get { return false; } }
      public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

      public override void Use(Player p, string message)
      {
          if (message == "" || message.Split(' ').Length >= 2) { Help(p); return; }

            Command.all.Find("physics").Use(p, "0");
            p.level.Instant = true;
            Command.all.Find("replaceall").Use(p, message+" air");
            Command.all.Find("reveal").Use(p, "all");
            p.level.Instant = false;
            Command.all.Find("physics").Use(p, "1");
            Player.GlobalMessageLevel(p.level, "Unflooded!");
      }

      public override void Help(Player p, string message = "")
      {
         Player.SendMessage(p, "/unflood [liquide] - Supprime tous les blocs [liquide] de la map");
      }
   }
}